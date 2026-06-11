using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.Text;

namespace _04_RAG_Intro
{
    public enum RAGBehavior
    {
        Always,
        OnDemandAsATool
    }

    public class HistoryReadingAiContextProviderExample
    {
        private readonly AIAgent _mafAgent;

        public HistoryReadingAiContextProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responsesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _mafAgent = new ChatClientAgent(responsesClient, new ChatClientAgentOptions()
            {
                Name = "History Reading Context Provider",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant. If specialized knowledge tools are available, call them when necessary to provide accurate information.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                },
                AIContextProviders = [
                    new HistoryReadingKeywordContextProvider(
                        searchAsync: SearchIndexAsync,
                        behavior: RAGBehavior.OnDemandAsATool,
                        historyWindowSize: 2)
                ],
                ChatHistoryProvider = new InMemoryChatHistoryProvider(new InMemoryChatHistoryProviderOptions()
                {
                    StorageInputRequestMessageFilter = messages => messages
                        .Where(msg => msg.GetAgentRequestMessageSourceType() != AgentRequestMessageSourceType.AIContextProvider)
                })
            });
        }

        private static async Task<string> SearchIndexAsync(string query, CancellationToken cancellationToken)
        {
            Console.WriteLine($"\n     SearchIndexAsync query:\n {query}\n");

            var contextBuilder = new StringBuilder();

            if (query.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("work", StringComparison.OrdinalIgnoreCase))
            {
                contextBuilder.AppendLine("Source: Corporate Remote Work Policy 2026\n" +
                    "Content: The 2026 remote work policy allows employees to work from anywhere within Poland for up to 3 days per week. Mondays are mandatory office days.");
            }

            if (query.Contains("bike", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("benefit", StringComparison.OrdinalIgnoreCase))
            {
                contextBuilder.AppendLine("Source: Employee Benefits Guide\n" +
                    "Content: The corporate bicycle benefit covers up to 2000 PLN for purchasing or maintaining cross and mountain bikes.");
            }

            return await Task.FromResult(contextBuilder.ToString());
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

    public class HistoryReadingKeywordContextProvider : AIContextProvider
    {
        private readonly Func<string, CancellationToken, Task<string>> _searchAsync;
        private readonly RAGBehavior _behavior;
        private readonly int _historyWindowSize;

        public HistoryReadingKeywordContextProvider(
            Func<string, CancellationToken, Task<string>> searchAsync,
            RAGBehavior behavior = RAGBehavior.OnDemandAsATool,
            int historyWindowSize = 3)
            : base(
                  provideInputMessageFilter: messages => messages.Where(msg =>
                    msg.GetAgentRequestMessageSourceType() == AgentRequestMessageSourceType.External ||
                    msg.GetAgentRequestMessageSourceType() == AgentRequestMessageSourceType.ChatHistory),
                  storeInputRequestMessageFilter: null,
                  storeInputResponseMessageFilter: null)
        {
            _searchAsync = searchAsync ?? throw new ArgumentNullException(nameof(searchAsync));
            _behavior = behavior;
            _historyWindowSize = historyWindowSize;
        }

        public override IReadOnlyList<string> StateKeys => [];

        protected override async ValueTask<AIContext> ProvideAIContextAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            var customContext = new AIContext();

            if (_behavior == RAGBehavior.Always)
            {
                Console.WriteLine("\n[RAG Mode: Always] - Intercepting pipeline and executing proactive background search...");

                var availableMessages = context.AIContext.Messages ?? Enumerable.Empty<ChatMessage>();

                var recentTurnTexts = availableMessages
                    .Where(m => m.Role == ChatRole.User)
                    .TakeLast(_historyWindowSize)
                    .Select(m => m.Text)
                    .Where(t => !string.IsNullOrWhiteSpace(t));

                string combinedHistoryText = string.Join(" ", recentTurnTexts);

                if (!string.IsNullOrWhiteSpace(combinedHistoryText))
                {
                    string searchResults = await _searchAsync(combinedHistoryText, cancellationToken);

                    if (!string.IsNullOrWhiteSpace(searchResults))
                    {
                        Console.WriteLine($"   -> Match identified using passive history analysis! Injecting context rows.");
                        customContext.Messages = [
                            new ChatMessage(ChatRole.User, $"[ADDITIONAL CONTEXT] Use this background data to accurately answer the user:\n{searchResults}")
                        ];
                    }
                }
            }
            else if (_behavior == RAGBehavior.OnDemandAsATool)
            {
                Console.WriteLine("\n[RAG Mode: OnDemandAsATool] - Bypassing upfront search. Registering dynamic operational tool...");

                // Wrap our data delegate as a first-class function schema visibility payload for the LLM core
                var searchTool = AIFunctionFactory.Create(
                    async (string query, CancellationToken ct) =>
                    {
                        Console.WriteLine($"\n[On-Demand Tool Triggered] - LLM requested search query: '{query}'");
                        return await _searchAsync(query, ct);
                    },
                    name: "SearchCorporateKnowledgeBase",
                    description: "Queries the core internal database repository to fetch remote work schedules, office policies, and employee monetary benefits."
                );

                customContext.Tools = [searchTool];
            }

            return customContext;
        }

        protected override ValueTask<AIContext> InvokingCoreAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            return base.InvokingCoreAsync(context, cancellationToken);
        }

        protected override ValueTask InvokedCoreAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            return base.InvokedCoreAsync(context, cancellationToken);
        }

        protected override ValueTask StoreAIContextAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            return base.StoreAIContextAsync(context, cancellationToken);
        }
    }
}