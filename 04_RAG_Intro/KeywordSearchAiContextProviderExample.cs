using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace _04_RAG_Intro
{
    public class KeywordSearchAiContextProviderExample
    {
        private readonly AIAgent _mafAgent;

        public KeywordSearchAiContextProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responsesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _mafAgent = new ChatClientAgent(responsesClient, new ChatClientAgentOptions()
            {
                Name = "Fully customized Context Provider Test",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                },
                AIContextProviders = [new CustomKeywordSearchContextProvider()]
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

    public class CustomKeywordSearchContextProvider : AIContextProvider
    {
        public CustomKeywordSearchContextProvider() : base(
            provideInputMessageFilter: messages => messages,
            storeInputRequestMessageFilter: null,
            storeInputResponseMessageFilter: null) { }

        protected override async ValueTask<AIContext> InvokingCoreAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("\n[1. InvokingCoreAsync] - Pipeline orchestration wrapper triggered.");

            var aiContext = await base.InvokingCoreAsync(context, cancellationToken);
            return aiContext;
        }

        protected override ValueTask<AIContext> ProvideAIContextAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[2. ProvideAIContextAsync] - Custom business lookup logic processing...");

            var lastUserMessage = context.AIContext.Messages?
                .LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

            var customContext = new AIContext();
            var contextBuilder = new System.Text.StringBuilder();

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
                Console.WriteLine("   -> Match successfully discovered. Injecting text as transient prompt context.");
                customContext.Messages = [new ChatMessage(ChatRole.User, $"Use this background data:\n{contextBuilder}")];
            }
            else
            {
                Console.WriteLine("   -> No matched keywords found. Passing clean context arrays.");
            }

            return ValueTask.FromResult(customContext);
        }

        protected override async ValueTask InvokedCoreAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[3. InvokedCoreAsync] - Post-turn pipeline interceptor starting...");
            await base.InvokedCoreAsync(context, cancellationToken);
        }

        protected override ValueTask StoreAIContextAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
        {
            Console.WriteLine("[4. StoreAIContextAsync] - Turn completed. Evaluating outcome state...");

            if (context.InvokeException is not null)
            {
                Console.WriteLine($"   -> Session exception flag recorded: {context.InvokeException.Message}");
            }
            else
            {
                Console.WriteLine($"   -> Session step completed. Succeeded processing {context.ResponseMessages.Count()} response messages.");
            }

            return ValueTask.CompletedTask;
        }
    }
}