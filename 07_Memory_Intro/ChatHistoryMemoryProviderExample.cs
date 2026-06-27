using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Connectors.InMemory;
using OpenAI.Responses;

namespace _07_Memory_Intro
{
    public class ChatHistoryMemoryProviderExample
    {
        private readonly AIAgent _mafAgent;

        public ChatHistoryMemoryProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!)
                .AsBuilder()
                .Use(inner => new InspectingChatClient(inner))
                .Build();

            IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_URI")!), new DefaultAzureCredential())
                .GetEmbeddingClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_EMBEDDING_MODEL")!)
                .AsIEmbeddingGenerator();

            VectorStore vectorStore = new InMemoryVectorStore(new InMemoryVectorStoreOptions()
            {
                EmbeddingGenerator = embeddingGenerator
            });

            var agentName = "Agent powered by Foundry IQ data via HTTP";
            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = agentName,
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None }
                },
                AIContextProviders = [new ChatHistoryMemoryProvider(
                    vectorStore,
                    collectionName: "vectorized_chat_history",
                    vectorDimensions: 1536,
                    session => new ChatHistoryMemoryProvider.State(
                        storageScope: new() 
                        { 
                            UserId = "123", 
                            SessionId = Guid.NewGuid().ToString(), 
                            AgentId = agentName,
                            ApplicationId = "Memory:Intro"
                        },
                        searchScope: new() 
                        { 
                            UserId = "123",
                            //SessionId = null,
                            //AgentId = null,
                            //ApplicationId = null
                        }),
                    new ChatHistoryMemoryProviderOptions()
                    {
                        MaxResults = 3,
                        SearchTime = ChatHistoryMemoryProviderOptions.SearchBehavior.BeforeAIInvoke,
                        SearchInputMessageFilter = null,
                        StorageInputRequestMessageFilter = null,
                        StorageInputResponseMessageFilter = msgs => []
                    }
                )]
            });
        }

        public async Task RunAsync()
        {
            AgentSession firstSession = await _mafAgent.CreateSessionAsync();
            Console.WriteLine(await _mafAgent.RunAsync("I live in Poland", firstSession));

            Console.WriteLine("\n---------------\n");

            AgentSession secondSession = await _mafAgent.CreateSessionAsync();
            Console.WriteLine(await _mafAgent.RunAsync("What is the capital of the country I live in", secondSession));
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
