using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text.Json;

namespace _03_ServiceManagedChatHistory
{
    public class ResponsesApiWithStoredOutputDisabledExample
    {
        private readonly ChatClientAgent _mafAgent;
        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };
        private readonly bool _includeReasoningEncryptedContent = true;

        public ResponsesApiWithStoredOutputDisabledExample()
        {
            AzureOpenAIClient openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(
                    Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!, 
                    includeReasoningEncryptedContent: _includeReasoningEncryptedContent);

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent which does not store any history on its own",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assisant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.Medium, Output = ReasoningOutput.Full }, // effort, summary
                }
            });
            }

        public async Task RunAsync()
        {
            var session = await _mafAgent.CreateSessionAsync();

            if (_includeReasoningEncryptedContent)
            {
                Console.WriteLine("Include reasoning encrypted content ENABLED");
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
                Console.WriteLine($"ReasoningTokenCount: {agentResponse.Usage?.ReasoningTokenCount ?? 0}");
                Console.WriteLine($"Response id: {agentResponse.ResponseId}");
                if (session is ChatClientAgentSession chatClientAgentSession)
                {
                    Console.WriteLine($"Conversation id: {chatClientAgentSession.ConversationId ?? "N/A"}");
                    Console.WriteLine(JsonSerializer.Serialize(chatClientAgentSession, _serializerOptions));
                }

                if (agentResponse.RawRepresentation is Microsoft.Extensions.AI.ChatResponse chatResponse &&
                    chatResponse.RawRepresentation is OpenAI.Responses.ResponseResult responseResult)
                {
                    foreach (var item in responseResult.OutputItems.OfType<OpenAI.Responses.ReasoningResponseItem>())
                    {
                        Console.WriteLine($"Reasoning response item > Encrypted content: {item.EncryptedContent}");
                    }
                }
            }
        }
    }
}
