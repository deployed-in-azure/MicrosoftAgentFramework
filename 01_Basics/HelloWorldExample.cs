using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Hello World Agent
    /// </summary>
    public class HelloWorldExample
    {
        private readonly AIAgent _mafAgent;

        public HelloWorldExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            _mafAgent = chatClient
                .AsAIAgent(
                    instructions: "Always respond with 'Hello world'",
                    name: "The Hello World Agent",
                    description: "This is a very simple agent");
        }

        public async Task RunAsync()
        {
            var agentResponse = await _mafAgent.RunAsync(message: "Hello");
            Console.WriteLine(agentResponse);

            Console.WriteLine($"Input Tokens: {agentResponse.Usage!.InputTokenCount}, Output Tokens: {agentResponse.Usage.OutputTokenCount}");

            await foreach (var agentResponseUpdate in _mafAgent.RunStreamingAsync("Hello again"))
            {
                Console.Write(agentResponseUpdate);
            }
        }
    }
}
