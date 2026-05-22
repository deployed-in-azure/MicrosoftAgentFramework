using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Structured Output Agent
    /// </summary>
    public class StructuredOutputExample
    {
        private readonly AIAgent _mafAgent;

        public StructuredOutputExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var chatClient = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!);

            _mafAgent = chatClient
                .AsAIAgent(
                    instructions: "You are a helpful assistant",
                    name: "The Structured Output Agent");
        }

        public async Task RunAsync()
        {
            var response = await _mafAgent.RunAsync<MovieReview>(
                "I just watched Inception. It was mind-blowing! Definitely a 5 out of 5 stars.");

            Console.WriteLine($"Movie: {response.Result!.Title}");
            Console.WriteLine($"Sentiment: {response.Result.Sentiment}");
            Console.WriteLine($"Stars: {response.Result.Stars}");
        }
    }

    public class MovieReview
    {
        public string Title { get; set; } = string.Empty;
        public string Sentiment { get; set; } = string.Empty;
        public int Stars { get; set; }
    }
}
