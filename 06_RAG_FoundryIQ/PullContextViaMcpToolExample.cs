using Azure.AI.OpenAI;
using Azure.Core;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI.Responses;

namespace _06_RAG_FoundryIQ
{
    public class PullContextViaMcpToolExample : IAsyncDisposable
    {
        private AIAgent? _mafAgent;
        private McpClient? _mcpClient;

        private async Task InitializeAgentAsync()
        {
            if (_mafAgent != null) return;

            var mcpUrl = $"{Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_URI")!}/knowledgebases/{Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_KNOWLEDGE_BASE")!}/mcp?api-version=2026-04-01";

            var httpClient = new HttpClient();
            var tokenResult = await new DefaultAzureCredential().GetTokenAsync(new TokenRequestContext(["https://search.azure.com/.default"]));
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenResult.Token);

            var clientTransport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(mcpUrl),
                Name = "ContosoSearchKBMcpServer"
            }, httpClient, ownsHttpClient: true);

            _mcpClient = await McpClient.CreateAsync(clientTransport);

            var discoveredMcpTools = await _mcpClient.ListToolsAsync();
            var retrieveTool = discoveredMcpTools.Single() as AIFunction;
            retrieveTool = new DescriptionOverridingFunction(retrieveTool, "Searches for information about Contoso's Microsoft cloud architecture, including services, configurations, and design decisions");

        var responsesClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), new DefaultAzureCredential())
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!)
                .AsBuilder()
                .Use(inner => new InspectingChatClient(inner))
                .Build();

            _mafAgent = new ChatClientAgent(responsesClient, new ChatClientAgentOptions()
            {
                Name = "Agent powered by Foundry IQ data via MCP",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are an expert cloud architect for Contoso. Always leverage the search_knowledge_base_retrieve tool to fetch real-world data files before answering.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                    Tools = [retrieveTool]
                }
            });
        }

        public async Task RunAsync()
        {
            await InitializeAgentAsync();
            var session = await _mafAgent!.CreateSessionAsync();

            while (true)
            {
                Console.Write("You: ");

                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var agentResponse = await _mafAgent.RunAsync(message: input, session);
                Console.WriteLine($"Agent: {agentResponse}\n");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_mcpClient != null)
            {
                await _mcpClient.DisposeAsync();
            }
        }
    }

    internal class DescriptionOverridingFunction(AIFunction innerFunction, string customDescription) 
        : DelegatingAIFunction(innerFunction)
    {
        public override string Description => customDescription;
    }

    internal class InspectingChatClient(IChatClient innerClient) 
        : DelegatingChatClient(innerClient)
    {
        public override async Task<ChatResponse> GetResponseAsync(
            IEnumerable<ChatMessage> messages,
            ChatOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            var response = await base.GetResponseAsync(messages, options, cancellationToken);
            return response;
        }
    }
}