using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text.Json;

namespace _03_ServiceManagedChatHistory
{
    public class ResponsesApiWithStoredOutputExample
    {
        private readonly ChatClientAgent _mafAgent;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public ResponsesApiWithStoredOutputExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responsesClient = openAiClient
                .GetResponsesClient();

            _mafAgent = responsesClient
                .AsAIAgent(new ChatClientAgentOptions()
                {
                    Name = "Agent which does not store any history on its own",
                    ChatOptions = new ChatOptions { Instructions = "You are a helpful assisant." },
                },
                model: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);
        }

        public async Task RunAsync()
        {
            Console.Write("Enter a Previous Response ID to resume (or press Enter to start a new conversation): ");
            var previousResponseId = Console.ReadLine();

            var session = !string.IsNullOrWhiteSpace(previousResponseId)
                ? await _mafAgent.CreateSessionAsync(previousResponseId)
                : await _mafAgent.CreateSessionAsync();

            if (!string.IsNullOrWhiteSpace(previousResponseId))
            {
                Console.WriteLine($"Resumed from response: {previousResponseId}");
            }
            else
            {
                Console.WriteLine("Started a new conversation.");
            }

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var agentResponse = await _mafAgent.RunAsync(message: input, session);
                Console.WriteLine($"Agent: {agentResponse}");
                Console.WriteLine($"Response id: {agentResponse.ResponseId}");

                if (session is ChatClientAgentSession chatClientAgentSession)
                {
                    Console.WriteLine($"Conversation id: {chatClientAgentSession.ConversationId}");
                    Console.WriteLine(JsonSerializer.Serialize(chatClientAgentSession, _serializerOptions));
                }
            }
        }
    }
}
