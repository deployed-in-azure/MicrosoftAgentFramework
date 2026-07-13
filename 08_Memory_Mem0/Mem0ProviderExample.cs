using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace _08_Memory_Mem0
{
    public class Mem0ProviderExample : IDisposable
    {
        private readonly AIAgent _mafAgent;
        private readonly Mem0MemoryProvider _mem0MemoryProvider;

        public Mem0ProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!)
                .AsBuilder()
                .Use(inner => new InspectingChatClient(inner))
                .Build();

            _mem0MemoryProvider = new Mem0MemoryProvider(
                    apiKey: Environment.GetEnvironmentVariable("MEM0_API_KEY")!,
                    sessionState =>
                    {
                        var executionContext = (sessionState?.GetSessionExecutionContext()) ?? throw new InvalidOperationException("Execution context is not initialized");

                        return new Mem0ProviderState(
                            storageScope: new Mem0ProviderScope
                            {
                                RunId = executionContext.RunId,
                                UserId = executionContext.UserId,
                                AppId = executionContext.ApplicationId,
                                AgentId = executionContext.AgentId
                            },
                            searchScope: new Mem0ProviderScope
                            {
                                UserId = executionContext.UserId,
                                AppId = executionContext.ApplicationId
                            });
                    });

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent which uses Mem0",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None }
                },
                AIContextProviders = [_mem0MemoryProvider]
            });
        }

        public async Task RunAsync()
        {
            AgentSession firstSession = await _mafAgent.CreateSessionWithExecutionContext(new SessionExecutionContext()
            {
                RunId = Guid.NewGuid().ToString(),
                UserId = "Michal 123",
                ApplicationId = "travel_assistant",
                AgentId = "travel_assistant_agent"
            });

            Console.WriteLine("[User]: I live in Poland close to Krakow and I like to travel.");
            var response1 = await _mafAgent.RunAsync("I live in Poland close to Krakow and I like to travel", firstSession);
            Console.WriteLine($"[Agent]: {response1}");


            Console.WriteLine("\n========================================================");

            AgentSession secondSession = await _mafAgent.CreateSessionWithExecutionContext(new SessionExecutionContext()
            {
                RunId = Guid.NewGuid().ToString(),
                UserId = "Michal 123",
                ApplicationId = "travel_assistant",
                AgentId = "travel_assistant_agent"
            });

            Console.WriteLine("[User]: What 3 places should I visit in the city I live next to and what food should I eat?");
            var response2 = await _mafAgent.RunAsync("What 3 places should I visit in the city I live next to and what food should I eat?", secondSession);
            Console.WriteLine($"[Agent]: {response2}");

            Console.WriteLine("\n========================================================");

            AgentSession thirdSession = await _mafAgent.CreateSessionWithExecutionContext(new SessionExecutionContext()
            {
                RunId = Guid.NewGuid().ToString(),
                UserId = "Michal 123",
                ApplicationId = "gym_coach_assistant",
                AgentId = "gym_coach_assistant_agent"
            });

            Console.WriteLine("[User]: Based on the country I live in, should my strength tracking default to kilograms or pounds?");
            var response3 = await _mafAgent.RunAsync("Based on the country I live in, should my strength tracking default to kilograms or pounds?", thirdSession);
            Console.WriteLine($"[Agent]: {response3}");

            AgentSession fourthSession = await _mafAgent.CreateSessionWithExecutionContext(new SessionExecutionContext()
            {
                RunId = Guid.NewGuid().ToString(),
                UserId = "Michal 123",
                ApplicationId = "gym_coach_assistant",
                AgentId = "gym_coach_assistant_agent"
            });

            Console.WriteLine("\n========================================================");

            Console.WriteLine("[User]: prepare training for this week, each should have just 3 exercises");
            var response4 = await _mafAgent.RunAsync("prepare training A and B for this week, each should have just 3 exercises", fourthSession);
            Console.WriteLine($"[Agent]: {response4}");
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

        public void Dispose() => _mem0MemoryProvider?.Dispose();
    }
}
