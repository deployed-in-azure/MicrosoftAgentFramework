using Azure.AI.Extensions.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text.Json;

namespace _03_ServiceManagedChatHistory
{
    public class ConversationStoredInFoundryExample
    {
        private readonly AIProjectClient _aiProjectClient;
        private readonly ChatClientAgent _chatClientAgent;
        private readonly ProjectConversationsClient _projectConversationsClient;

        private readonly JsonSerializerOptions _serializerOptions = new() { WriteIndented = true };

        public ConversationStoredInFoundryExample()
        {
            _aiProjectClient = new AIProjectClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CONVERSATION_CLIENT_URI")!), 
                new DefaultAzureCredential());

            _chatClientAgent = _aiProjectClient.AsAIAgent(
                model: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!, 
                instructions: "You are a helpful assisant.", name: "Agent which stores the chat history in Microsoft Foundry");

            _projectConversationsClient = _aiProjectClient
                .GetProjectOpenAIClient()
                .GetProjectConversationsClient();
        }

        public async Task RunAsync()
        {
            Console.Write("Enter a conversation ID to resume (or press Enter to start a new conversation): ");
            var conversationId = Console.ReadLine();

            ProjectConversation projectConversation;
            if (!string.IsNullOrWhiteSpace(conversationId))
            {
                projectConversation = await _projectConversationsClient.GetProjectConversationAsync(conversationId);
                Console.WriteLine($"Resumed conversation: {projectConversation.Id}");
            }
            else
            {
                projectConversation = await _projectConversationsClient.CreateProjectConversationAsync();
                Console.WriteLine($"Started new conversation: {projectConversation.Id}");
            }
            var session = await _chatClientAgent.CreateSessionAsync(projectConversation.Id);

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                if (input == "history")
                {
                    await PrintTranscriptAsync(projectConversation.Id);
                    continue;
                }

                if (input == "delete")
                {
                    await _projectConversationsClient.DeleteConversationAsync(conversationId);
                    Console.WriteLine($"Conversation with ID `{conversationId}` deleted. Exit.");
                    break;
                }

                var agentResponse = await _chatClientAgent.RunAsync(message: input, session);
                Console.WriteLine($"Agent: {agentResponse}");
                Console.WriteLine($"Response id: {agentResponse.ResponseId}");

                if (session is ChatClientAgentSession chatClientAgentSession)
                {
                    Console.WriteLine($"Conversation id: {chatClientAgentSession.ConversationId}");

                    Console.WriteLine(JsonSerializer.Serialize(chatClientAgentSession, _serializerOptions));
                }
            }
        }

        public async Task PrintTranscriptAsync(string conversationId)
        {
            var itemsStream = _projectConversationsClient.GetProjectConversationItemsAsync(
                conversationId,
                itemKind: null,
                limit: 10,
                order: "asc",
                after: null,
                before: null);

            await foreach (AgentResponseItem agentItem in itemsStream)
            {
                ResponseItem standardItem = agentItem.AsResponseResultItem();

                if (standardItem is MessageResponseItem messageItem)
                {
                    var textParts = messageItem.Content
                        .Where(part => part.Kind == ResponseContentPartKind.OutputText || !string.IsNullOrEmpty(part.Text))
                        .Select(part => part.Text);

                    var fullMessageText = string.Join(Environment.NewLine, textParts);

                    Console.WriteLine($"[{messageItem.Role.ToString().ToUpper()}]: {fullMessageText}");
                }
            }

            Console.WriteLine("========================================================\n");
        }
    }
}
