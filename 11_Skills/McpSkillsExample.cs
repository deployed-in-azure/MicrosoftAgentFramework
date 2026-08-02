using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using OpenAI.Responses;

namespace _11_Skills
{
    internal class McpSkillsExample
    {
        private AIAgent? _mafAgent;

        public async Task RunAsync(params string[] arguments)
        {
            if (arguments.Length != 0 && arguments[0] == "--run-mcp-server-with-skills")
            {
                var builder = Host.CreateApplicationBuilder();

                builder.Logging.ClearProviders();
                builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

                builder.Services
                    .AddMcpServer(o => o.ServerInfo = new() { Name = "mcp-server-with-skills", Version = "0.0.1" })
                    .WithStdioServerTransport()
                    .WithResources<McpSkillResources>();

                await builder.Build().RunAsync();

                return;
            }

            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            await using var mcpClient = await McpClient.CreateAsync(new StdioClientTransport(new()
            {
                Name = "mcp-server-with-skills",
                Command = "dotnet",
                Arguments = [typeof(Program).Assembly.Location, "--run-mcp-server-with-skills"],
            }));

            var skillsProvider = new AgentSkillsProviderBuilder()
                .UseMcpSkills(mcpClient)
                .Build();

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent with class skill",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant that always responds concisely."
                },
                AIContextProviders = [skillsProvider, new EmptyAiContextProvider()]
            })
            .AsBuilder()
            .UseToolApproval(new ToolApprovalAgentOptions
            {
                AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
            })
            .Build();


            var userQuery = "I bought a Laptop on 2025-01-15. Is it still covered under warranty?";
            Console.WriteLine($"User: {userQuery}\n");

            var session = await _mafAgent.CreateSessionAsync();
            var response = await _mafAgent.RunAsync(userQuery, session);
            Console.WriteLine($"Agent: {response}");
        }
    }
}
