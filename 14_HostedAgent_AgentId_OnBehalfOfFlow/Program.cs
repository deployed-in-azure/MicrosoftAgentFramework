using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using System.Net.Http.Json;

namespace _14_HostedAgent_AgentId_OnBehalfOfFlow
{
    internal class Program
    {
        private const string DEFAULT_GRAPH_SCOPE = "https://graph.microsoft.com/.default";
        private const string ALLOWED_SCOPE = "access_as_user";

        static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddInMemoryTokenCaches();

            builder.Services.AddAgentIdentities();

            var agentIdentityId = Environment.GetEnvironmentVariable("AgentIdentityId") ?? throw new InvalidOperationException("AGENT_IDENTITY_ID is not set.");
            var graphCallMethod = Environment.GetEnvironmentVariable("GraphCallMethod") ?? "GraphServiceClient";

            // let's intercept the OBO token exchange request and response
            builder.Services.AddTransient<MsalLoggingHandler>();
            builder.Services.ConfigureAll<HttpClientFactoryOptions>(options =>
            {
                options.HttpMessageHandlerBuilderActions.Add(b =>
                {
                    b.AdditionalHandlers.Add(b.Services.GetRequiredService<MsalLoggingHandler>());
                });
            });

            // Intercept the request to read the OBO token from the custom header
            builder.Services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events ??= new JwtBearerEvents();
                var originalOnMessageReceived = options.Events.OnMessageReceived;

                options.Events.OnMessageReceived = async context =>
                {
                    foreach (var header in context.Request.Headers)
                    {
                        Console.WriteLine($"[HeaderTrace] {header.Key}: {string.Join(", ", header.Value)}");
                    }

                    var customAuth = context.Request.Headers["BlueprintAuthorization"].FirstOrDefault();
                    var usedCustomHeader = !string.IsNullOrEmpty(customAuth) && customAuth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase);

                    if (usedCustomHeader)
                    {
                        context.Token = customAuth!.Substring("Bearer ".Length).Trim();
                    }

                    Console.WriteLine($"[Auth] BlueprintAuthorization header present: {!string.IsNullOrEmpty(customAuth)}, used as bearer token: {usedCustomHeader}, standard Authorization header present: {context.Request.Headers.ContainsKey("Authorization")}");

                    if (originalOnMessageReceived != null)
                    {
                        await originalOnMessageReceived(context);
                    }
                };
            });

            var httpContextAccessor = new HttpContextAccessor();
            builder.Services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);

            AIFunction graphTool;
            switch (graphCallMethod)
            {
                case "GraphServiceClient":
                    builder.Services.AddMicrosoftGraph();

                    graphTool = CreateGraphServiceClientTool(httpContextAccessor, agentIdentityId);
                    break;
                case "DownstreamApi":
                    builder.Services.AddDownstreamApis(builder.Configuration.GetSection("DownstreamApis"));

                    graphTool = CreateDownstreamApiTool(httpContextAccessor, agentIdentityId);
                    break;
                case "HttpClient":
                    builder.Services.AddHttpClient("GraphApi", client =>
                    {
                        client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                    })
                    .AddMicrosoftIdentityMessageHandler(options =>
                    {
                        options.Scopes = [DEFAULT_GRAPH_SCOPE];
                        options.WithAgentIdentity(agentIdentityId);
                    });

                    graphTool = CreateHttpClientTool(httpContextAccessor, agentIdentityId, "GraphApi");
                    break;
                case "ManualHttpClient":
                    builder.Services.AddHttpClient("GraphApiManual", client =>
                    {
                        client.BaseAddress = new Uri("https://graph.microsoft.com/v1.0/");
                    })
                    .AddHttpMessageHandler(sp =>
                    {
                        var handlerOptions = new MicrosoftIdentityMessageHandlerOptions
                        {
                            Scopes = [DEFAULT_GRAPH_SCOPE]
                        };

                        handlerOptions.WithAgentIdentity(agentIdentityId);

                        return new MicrosoftIdentityMessageHandler(sp.GetRequiredService<IAuthorizationHeaderProvider>(), handlerOptions);
                    });

                    graphTool = CreateHttpClientTool(httpContextAccessor, agentIdentityId, "GraphApiManual");
                    break;
                default:
                    throw new InvalidOperationException($"Unsupported GraphCallMethod '{graphCallMethod}'. Expected 'GraphServiceClient', 'DownstreamApi', 'HttpClient' or 'ManualHttpClient'.");
            }

            Console.WriteLine($"Using '{graphCallMethod}' approach to call Microsoft Graph on behalf of the signed-in user.");

            var agent = new AIProjectClient(new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")!), new DefaultAzureCredential())
                .AsAIAgent(
                    model: Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME")!,
                    instructions: "You are an assistant that can look up the signed-in user's Microsoft Graph profile on their behalf.",
                    name: "Microsoft Graph Profile Assistant",
                    description: "An AI assistant that can retrieve the signed-in user's Microsoft Graph profile via an On-Behalf-Of flow.",
                    tools: [graphTool]);

            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .RequireScope(ALLOWED_SCOPE)
                .Build());

            builder.Services.AddFoundryResponses(agent);
            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapFoundryResponses();

            await app.RunAsync();
        }

        /// <summary>
        /// Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange,
        /// using the strongly-typed <see cref="GraphServiceClient"/>.
        /// </summary>
        private static AIFunction CreateGraphServiceClientTool(IHttpContextAccessor httpContextAccessor, string agentIdentityId) =>
            AIFunctionFactory.Create(async () =>
            {
                var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context is available to perform the On-Behalf-Of exchange.");

                try
                {
                    // per tool authorization check to ensure the signed-in user has the required scope to call this tool
                    httpContext.VerifyUserHasAnyAcceptedScope(ALLOWED_SCOPE);
                }
                catch (UnauthorizedAccessException)
                {
                    // The user does not have the required scope to call this tool
                }

                // redundant call (just for the demo) which shows how to pull the token using OBO flow
                // you can grab it and check it using jwt.ms to see the claims and scopes
                //var authHeader = await httpContext.RequestServices.GetRequiredService<IAuthorizationHeaderProvider>()
                //    .CreateAuthorizationHeaderForUserAsync(
                //        scopes: [DEFAULT_GRAPH_SCOPE],
                //        new AuthorizationHeaderProviderOptions().WithAgentIdentity(agentIdentityId));

                var me = await httpContext.RequestServices.GetRequiredService<GraphServiceClient>()
                    .Me
                    .GetAsync(r => r.Options.WithAuthenticationOptions(options =>
                    {
                        options.WithAgentIdentity(agentIdentityId);
                    }));

                return new
                {
                    me?.DisplayName,
                    me?.UserPrincipalName,
                };
            },
            name: "GetMyGraphProfile",
            description: "Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange using the agent's identity.");

        /// <summary>
        /// Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange,
        /// using the generic <see cref="IDownstreamApi"/> abstraction.
        /// </summary>
        private static AIFunction CreateDownstreamApiTool(IHttpContextAccessor httpContextAccessor, string agentIdentityId) =>
            AIFunctionFactory.Create(async () =>
            {
                var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context is available to perform the On-Behalf-Of exchange.");

                var me = await httpContext.RequestServices.GetRequiredService<IDownstreamApi>()
                        .GetForUserAsync<Microsoft.Graph.Models.User>(
                            serviceName: "GraphApi",
                            options =>
                            {
                                options.RelativePath = "me";
                                options.WithAgentIdentity(agentIdentityId);
                            });

                return new { me?.DisplayName, me?.UserPrincipalName };
            },
            name: "GetMyGraphProfile",
            description: "Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange using the agent's identity.");

        /// <summary>
        /// Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange,
        /// using a named <see cref="HttpClient"/> configured with <see cref="MicrosoftIdentityMessageHandler"/>.
        /// </summary>
        private static AIFunction CreateHttpClientTool(
            IHttpContextAccessor httpContextAccessor, 
            string agentIdentityId,
            string httpClientName) => AIFunctionFactory.Create(async () =>
            {
                var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context is available to perform the On-Behalf-Of exchange.");

                var httpClient = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient(httpClientName);
                var me = await httpClient.GetFromJsonAsync<Microsoft.Graph.Models.User>(requestUri: "me");

                return new
                {
                    me?.DisplayName,
                    me?.UserPrincipalName,
                };
            },
            name: "GetMyGraphProfile",
            description: "Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange using the agent's identity.");
    }

    public class MsalLoggingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var isTokenEndpoint = request.RequestUri != null && request.RequestUri.AbsolutePath.EndsWith("/oauth2/v2.0/token");

            if (isTokenEndpoint && request.Content != null)
            {
                var requestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                var decodedBody = System.Web.HttpUtility.UrlDecode(requestBody);
                var formattedParams = string.Join("\n", decodedBody.Split('&'));

                Console.WriteLine("\n=== OBO TOKEN EXCHANGE REQUEST ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine("BODY:");
                Console.WriteLine(formattedParams);
                Console.WriteLine("==================================\n");
            }
            else
            {
                Console.WriteLine($"[HttpTrace] {request.Method} {request.RequestUri}");
            }

            HttpResponseMessage response;
            try
            {
                response = await base.SendAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[HttpTrace] Request to {request.RequestUri} threw: {ex}");
                throw;
            }

            if (!isTokenEndpoint)
            {
                Console.WriteLine($"[HttpTrace] {request.Method} {request.RequestUri} -> {(int)response.StatusCode}");
            }

            if (isTokenEndpoint && response.IsSuccessStatusCode && response.Content != null)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var bearerToken = System.Text.Json.JsonDocument.Parse(responseBody).RootElement.GetProperty("access_token").GetString();

                Console.WriteLine("\n=== OBO TOKEN EXCHANGE RESPONSE ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine("BEARER TOKEN:");
                Console.WriteLine(bearerToken);
                Console.WriteLine("====================================\n");
            }
            else if (isTokenEndpoint && !response.IsSuccessStatusCode && response.Content != null)
            {
                var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                Console.WriteLine("\n=== OBO TOKEN EXCHANGE FAILED ===");
                Console.WriteLine($"URL: {request.RequestUri}");
                Console.WriteLine($"STATUS: {(int)response.StatusCode}");
                Console.WriteLine($"BODY: {errorBody}");
                Console.WriteLine("==================================\n");
            }

            return response;
        }
    }
}
