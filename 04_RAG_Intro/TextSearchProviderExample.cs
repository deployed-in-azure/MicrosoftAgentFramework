using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;

namespace _04_RAG_Intro
{
    public class TextSearchProviderExample
    {
        private readonly AIAgent _mafAgent;

        public TextSearchProviderExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Context Provider test",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None }, // effort, summary
                },
                AIContextProviders = [
                    new TextSearchProvider(SearchMethodAsync, new TextSearchProviderOptions
                    {
                        SearchTime = TextSearchProviderOptions.TextSearchBehavior.OnDemandFunctionCalling,
                        RecentMessageMemoryLimit = 2,
                        FunctionToolDescription = "This tool contains information about work policies and benefits in various offices and locations."
                    }),
                    new EmptyAiContextProvider()
                ]
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
                Console.WriteLine($"Agent: {agentResponse}");
            }
        }

        private Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchMethodAsync(
            string query,
            CancellationToken cancellationToken)
        {
            Console.WriteLine($"\n     Search method query:\n {query}\n");

            List<TextSearchProvider.TextSearchResult> results = [];

            if (query.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("work", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new()
                {
                    RawRepresentation = null,
                    SourceName = "Corporate Remote Work Policy 2026",
                    SourceLink = "https://internal.company.com/policies/remote-2026",
                    Text = "The 2026 remote work policy allows employees to work from anywhere within Poland for up to 3 days per week. Mondays are mandatory office days."
                });
            }

            return Task.FromResult<IEnumerable<TextSearchProvider.TextSearchResult>>(results);
        }

        public class EmptyAiContextProvider : AIContextProvider
        {
            protected override async ValueTask<AIContext> InvokingCoreAsync(
                InvokingContext context,
                CancellationToken cancellationToken = default)
            {
                var aiContext = await base.InvokingCoreAsync(context, cancellationToken);
                return aiContext;
            }
        }
    }
}
