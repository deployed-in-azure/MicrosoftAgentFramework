using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Diagnostics;

namespace _09_Memory_FoundryMemory
{
    internal class FoundryMemoryProviderExample
    {
        private readonly AIAgent _mafAgent;

        public FoundryMemoryProviderExample()
        {
            var credential = new DefaultAzureCredential();
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), credential);

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!)
                .AsBuilder()
                .Use(inner => new InspectingChatClient(inner))
                .Build();

            var foundryMemoryProvider = new FoundryMemoryProvider(
                new AIProjectClient(new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_NAME")!), credential),
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                stateInitializer: _ => new(new FoundryMemoryProviderScope("Michal-123")));

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent powered by Foundry Memory",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant that always responds in English.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.Low, Output = ReasoningOutput.None }
                },
                AIContextProviders = [foundryMemoryProvider]
            });
        }

        public async Task RunAsync()
        {
            var foundryMemoryProvider = _mafAgent.GetService<FoundryMemoryProvider>();

            AgentSession firstSession = await _mafAgent.CreateSessionAsync();
            await foundryMemoryProvider!.EnsureStoredMemoriesDeletedAsync(firstSession);
            Console.WriteLine(await _mafAgent.RunAsync("I live in Poland", firstSession));

            Console.WriteLine("\n---------------\n");

            var start = Stopwatch.StartNew();

            await foundryMemoryProvider!.WhenUpdatesCompletedAsync(pollingInterval: TimeSpan.FromSeconds(1));

            start.Stop();
            Console.WriteLine($"Memory update completed in {start.Elapsed.TotalSeconds}s");

            Console.WriteLine("\n---------------\n");

            AgentSession secondSession = await _mafAgent.CreateSessionAsync();
            Console.WriteLine(await _mafAgent.RunAsync("What is the capital of the country I live in?", secondSession));
        }

        internal class InspectingChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
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
}
