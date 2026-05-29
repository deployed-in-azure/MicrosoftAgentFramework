using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _02_ChatHistory
{
    public class CosmosDbChatHistoryExample
    {
        private readonly AIAgent _mafAgent;

        public CosmosDbChatHistoryExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            _mafAgent = chatClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "Agent which stores chat history in Azure Cosmos DB",
                    ChatOptions = new ChatOptions { Instructions = "You are a helpful assisant." },
                    ChatHistoryProvider = new CosmosChatHistoryProvider(
                        accountEndpoint: Environment.GetEnvironmentVariable("AZURE_COSMOS_ACCOUNT_URI")!,
                        new DefaultAzureCredential(),
                        databaseId: Environment.GetEnvironmentVariable("AZURE_COSMOS_DB_NAME")!,
                        containerId: Environment.GetEnvironmentVariable("AZURE_COSMOS_CONTAINER_NAME")!,
                        stateInitializer: (agentSession) =>
                        {
                            var (tenantId, userId, conversationId) = agentSession.GetUserContext();
                            return new CosmosChatHistoryProvider.State(conversationId, tenantId, userId);
                        },
                        provideOutputMessageFilter: null,
                        storeInputRequestMessageFilter: null,
                        storeInputResponseMessageFilter: null)
                    {
                        MaxBatchSize = 100, // Gets or sets the maximum number of items per transactional batch operation
                        MaxItemCount = 100, // Gets or sets the maximum number of messages to return in a single query batch
                        MaxMessagesToRetrieve = 20, // Gets or sets the maximum number of messages to retrieve from the provider
                        MessageTtlSeconds = 86400 // Gets or sets the Time-To-Live (TTL) in seconds for messages
                    }
                });
        }

        public async Task RunAsync()
        {
            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var agentSession = await _mafAgent.CreateSessionWithUserContextAsync(
                    tenantId: FakeUserData.TENANT_ID,
                    userId: FakeUserData.USER_ID,
                    conversationId: FakeUserData.CONVERSATION_ID);

                var response = await _mafAgent.RunAsync(input, agentSession);
                Console.WriteLine($"Agent: {response}");
            }
        }
    }
}
