using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Orchestrating Conversation Flows with Chat Roles
    /// </summary>
    public class ChatRolesExample
    {
        private readonly AIAgent _mafAgent;

        public ChatRolesExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            _mafAgent = chatClient
                .AsAIAgent(
                    instructions: "You are a helpful assistant that gives concise answers.",
                    name: "Chat Roles Agent");
        }

        public async Task RunAsync()
        {
            // 1. User role — standard end-user message
            Console.WriteLine("=== User Role ===");
            var userMessage = new ChatMessage(ChatRole.User, "What is the speed of light?");
            Console.WriteLine(await _mafAgent.RunAsync(userMessage));

            // 2. Assistant role — inject a prior assistant turn to steer conversation context
            Console.WriteLine("\n=== Assistant Role (injected prior turn) ===");
            var priorUserMessage = new ChatMessage(ChatRole.User, "What programming language should I learn first?");
            var priorAssistantMessage = new ChatMessage(ChatRole.Assistant, "I recommend Python because it has a simple syntax and a large ecosystem.");
            var followUpMessage = new ChatMessage(ChatRole.User, "What are some good beginner Python projects?");

            Console.WriteLine(await _mafAgent.RunAsync([priorUserMessage, priorAssistantMessage, followUpMessage]));

            // 3. System role — override agent behaviour at runtime with an additional system message
            Console.WriteLine("\n=== System Role (runtime persona override) ===");
            var systemOverride = new ChatMessage(ChatRole.System, "From now on respond only using uppercase.");
            var rhymeRequest = new ChatMessage(ChatRole.User, "Say a joke.");

            Console.WriteLine(await _mafAgent.RunAsync([systemOverride, rhymeRequest]));

            var toolRole = ChatRole.Tool;
        }
    }
}
