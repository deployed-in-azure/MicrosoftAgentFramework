using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _02_ChatHistory
{
    public class BlobChatHistoryExample
    {
        private readonly AIAgent _mafAgent;

        public BlobChatHistoryExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            var blobServiceClient = new BlobServiceClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_URI")!),
                new DefaultAzureCredential());

            var blobContainerClient = blobServiceClient.GetBlobContainerClient("chat-history");

            _mafAgent = chatClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "Agent which stores chat history in Blob storage",
                    ChatOptions = new ChatOptions { Instructions = "You are a helpful assisant." },
                    ChatHistoryProvider = new BlobChatHistoryProvider(
                        blobContainerClient,
                        stateInitializer: (agentSession) =>
                        {
                            var (tenantId, userId, conversationId) = agentSession!.GetUserContext();
                            return new BlobChatHistoryProvider.State(conversationId!, tenantId, userId);
                        })
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
