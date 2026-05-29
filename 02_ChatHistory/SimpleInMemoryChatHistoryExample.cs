using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _02_ChatHistory
{
    public class SimpleInMemoryChatHistoryExample
    {
        private readonly AIAgent _mafAgent;

        public SimpleInMemoryChatHistoryExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            _mafAgent = chatClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "Agent with a simple in memory history provider",
                    ChatOptions = new ChatOptions { Instructions = "You are a helpful assisant." },
                    ChatHistoryProvider = new SimpleInMemoryChatHistoryProvider()
                });
        }

        public async Task RunAsync()
        {
            var agentSession = await _mafAgent.CreateSessionAsync();

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var response = await _mafAgent.RunAsync(input, agentSession);
                Console.WriteLine($"Agent: {response}");
            }
        }
    }
}
