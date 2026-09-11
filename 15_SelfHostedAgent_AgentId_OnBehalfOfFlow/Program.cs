using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http;
using Microsoft.Graph;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.Resource;
using OpenAI;
using OpenAI.Responses;
using System.ClientModel.Primitives;

namespace _15_SelfHostedAgent_AgentId_OnBehalfOfFlow
{
    public class Program
    {
        private const string DEFAULT_GRAPH_SCOPE = "https://graph.microsoft.com/.default";
        private const string ALLOWED_SCOPE = "access_as_user";

        public static async Task Main(string[] args)
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

            var httpContextAccessor = new HttpContextAccessor();
            builder.Services.AddSingleton<IHttpContextAccessor>(httpContextAccessor);
            builder.Services.AddDownstreamApis(builder.Configuration.GetSection("DownstreamApis"));

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

            var openAiClient = new OpenAIClient(
                new BearerTokenPolicy(new DefaultAzureCredential(), "https://ai.azure.com/.default"),
                new OpenAIClientOptions
                {
                    Endpoint = new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")!)
                });

            var responsesClient = openAiClient
                .GetResponsesClient();

            var agent = responsesClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "microsoft-graph-profile-assistant",
                    ChatOptions = new ChatOptions 
                    { 
                        Instructions = "You are an assistant that can look up the signed-in user's Microsoft Graph profile on their behalf.",
                        Tools = [graphTool]
                    },
                },
                model: Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME")!);

            var hostedAgentBuilder = builder.AddAIAgent(agent.Name!, (_, _) => agent);

            builder.Services.AddAuthorizationBuilder()
                .SetFallbackPolicy(new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .RequireScope(ALLOWED_SCOPE)
                    .Build());

            var app = builder.Build();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapOpenAIResponses(hostedAgentBuilder);

            app.MapGet("/", () => "Up and running!").AllowAnonymous();

            await app.RunAsync();
        }

        private static AIFunction CreateDownstreamApiTool(IHttpContextAccessor httpContextAccessor, string agentIdentityId) => AIFunctionFactory.Create(async () =>
        {
            var httpContext = httpContextAccessor.HttpContext ?? throw new InvalidOperationException("No active HTTP context is available to perform the On-Behalf-Of exchange.");

            var me = await httpContext.RequestServices.GetRequiredService<IDownstreamApi>()
                    .GetForUserAsync<Microsoft.Graph.Models.User>(
                        serviceName: "GraphApi", // DownstreamApis__GraphApi__BaseUrl
                        options =>
                        {
                            options.RelativePath = "me";
                            options.WithAgentIdentity(agentIdentityId);
                        });

            return new { me?.DisplayName, me?.UserPrincipalName };
        },
        name: "GetMyGraphProfile",
        description: "Gets the signed-in user's Microsoft Graph profile via an On-Behalf-Of token exchange using the agent's identity.");

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
}
