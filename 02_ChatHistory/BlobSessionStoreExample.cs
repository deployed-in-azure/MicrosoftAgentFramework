using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _02_ChatHistory
{
    public class BlobSessionStoreExample
    {
        private readonly AIAgent _mafAgent;
        private readonly BlobContainerClient _blobContainerClient;

        public BlobSessionStoreExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            var blobServiceClient = new BlobServiceClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_STORAGE_ACCOUNT_URI")!),
                new DefaultAzureCredential());

            _blobContainerClient = blobServiceClient.GetBlobContainerClient("sessions");

            _mafAgent = chatClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "Blob Session Store Agent",
                    ChatOptions = new ChatOptions { Instructions = "You are a helpful assisant." }
                });
        }

        public async Task RunAsync()
        {
            var blobStore = new BlobSessionStore(_blobContainerClient);

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) break;

                var agentSession = await blobStore.LoadSessionAsync(
                    tenantId: FakeUserData.TENANT_ID,
                    userId: FakeUserData.USER_ID,
                    conversationId: FakeUserData.CONVERSATION_ID,
                    _mafAgent);

                var response = await _mafAgent.RunAsync(input, agentSession);
                Console.WriteLine($"Agent: {response}");

                await blobStore.SaveSessionAsync(agentSession, _mafAgent);
            }
        }
    }
}
