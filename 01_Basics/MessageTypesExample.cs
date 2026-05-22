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
    /// Microsoft Agent Framework Tutorial, Example: Deep Dive into Message Types
    /// </summary>
    public class MessageTypesExample
    {
        private readonly AIAgent _mafAgent;

        public MessageTypesExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            _mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(
                    instructions: "You are a helpful assistant that gives concise answers.",
                    name: "Message Types Agent");
        }

        public async Task RunAsync()
        {
            // 1. Plain string message (simplest form)
            Console.WriteLine("=== String Message ===");
            Console.WriteLine(await _mafAgent.RunAsync("What is 2 + 2?"));

            // 2. ChatMessage with TextContent
            Console.WriteLine("\n=== ChatMessage (TextContent) ===");
            var textMessage = new ChatMessage(ChatRole.User, "What is the capital of Poland?");
            Console.WriteLine(await _mafAgent.RunAsync(textMessage));

            // 3. ChatMessage with multiple content parts (TextContent + DataContent for inline image)
            Console.WriteLine("\n=== ChatMessage (TextContent + DataContent) ===");

            var multipartMessage = new ChatMessage(ChatRole.User,
            [
                new TextContent("Describe what you see in this tiny image."),
                await DataContent.LoadFromAsync(Path.Combine(AppContext.BaseDirectory, "astronaut.jpg"))
            ]);

            Console.WriteLine(await _mafAgent.RunAsync(multipartMessage));

            // 4. ChatMessage with multiple content parts (TextContent + UriContent for hosted web image)
            Console.WriteLine("\n=== ChatMessage (TextContent + UriContent) ===");
            var urlMultipartMessage = new ChatMessage(ChatRole.User,
            [
                new TextContent("What is in this picture?"),
                // this is how you could protect sensitive data, you can replace it with any public image URL
                new UriContent(new Uri("https://sadeployedinazuremaf.blob.core.windows.net/samples/apollo_11.png?sasTokenGoesHere"))
            ]);
            Console.WriteLine(await _mafAgent.RunAsync(urlMultipartMessage));

            var functionCallContent = new FunctionCallContent(callId: Guid.NewGuid().ToString(), name: "Name", arguments: new Dictionary<string, object?>());
            var functionCallResult = new FunctionResultContent(callId: Guid.NewGuid().ToString(), result: "Result");
        }
    }
}
