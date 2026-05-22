using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using OpenTelemetry;
using OpenTelemetry.Trace;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Enterprise Observability and OpenTelemetry Tracing
    /// </summary>
    public class ObservabilityExample
    {
        public async Task RunAsync()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            string sourceName = Guid.NewGuid().ToString("N");
            var tracerProviderBuilder = Sdk.CreateTracerProviderBuilder()
                .AddSource(sourceName)
                .AddConsoleExporter();

            using var tracerProvider = tracerProviderBuilder.Build();

            var mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(
                    instructions: "You are a helpful assistant that gives concise answers.",
                    name: "Agent with OpenTelemetry")
                .AsBuilder()
                .UseOpenTelemetry(sourceName)
                .Build();

            Console.WriteLine(await mafAgent.RunAsync("Hello, how are you?"));
        }
    }
}
