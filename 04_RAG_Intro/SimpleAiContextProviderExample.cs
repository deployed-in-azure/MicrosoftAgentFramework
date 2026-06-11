using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text;

namespace _04_RAG_Intro
{
    public class SimpleAiContextProviderExample
    {
        private readonly AIAgent _mafAgent;

        public SimpleAiContextProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responsesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _mafAgent = new ChatClientAgent(responsesClient, new ChatClientAgentOptions()
            {
                Name = "Simple Context Provider Test",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                },
                AIContextProviders = [new SimpleKeywordContextProvider()]
            });
        }

        public async Task RunAsync()
        {
            var session = await _mafAgent.CreateSessionAsync();

            while (true)
            {
                Console.Write("You: ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                {
                    break;
                }

                var agentResponse = await _mafAgent.RunAsync(message: input, session);
                Console.WriteLine($"Agent: {agentResponse}\n");
            }
        }
    }

    public class SimpleKeywordContextProvider : AIContextProvider
    {
        protected override ValueTask<AIContext> ProvideAIContextAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("\n[ProvideAIContextAsync] - Intercepting query to fetch context...");

            var lastUserMessage = context.AIContext.Messages?
                .LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

            var customContext = new AIContext();
            var contextBuilder = new StringBuilder();

            if (lastUserMessage.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                lastUserMessage.Contains("work", StringComparison.OrdinalIgnoreCase))
            {
                contextBuilder.AppendLine("Source: Corporate Remote Work Policy 2026\n" +
                    "Content: The 2026 remote work policy allows employees to work from anywhere within Poland for up to 3 days per week. Mondays are mandatory office days.");
            }

            if (lastUserMessage.Contains("bike", StringComparison.OrdinalIgnoreCase) ||
                lastUserMessage.Contains("benefit", StringComparison.OrdinalIgnoreCase))
            {
                contextBuilder.AppendLine("Source: Employee Benefits Guide\n" +
                    "Content: The corporate bicycle benefit covers up to 2000 PLN for purchasing or maintaining cross and mountain bikes.");
            }

            if (contextBuilder.Length > 0)
            {
                Console.WriteLine("   -> Matching context discovered! Injecting grounding data.");

                customContext.Messages = [
                    new ChatMessage(ChatRole.User, $"Use this background data to accurately answer the user:\n{contextBuilder}")
                ];
            }
            else
            {
                Console.WriteLine("   -> No matching keywords found. Proceeding with base prompt.");
            }

            return ValueTask.FromResult(customContext);
        }
    }
}