using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Local Grounded Context and Simple RAG
    /// </summary>
    public class SimpleRagExample
    {
        private readonly AIAgent _mafAgent;

        public SimpleRagExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            var agentOptions = new ChatClientAgentOptions
            {
                ChatOptions = new()
                {
                    Instructions = "You are a helpful support specialist. Answer questions using ONLY the provided context and cite the source document name."
                },
                AIContextProviders = [new TextSearchProvider(SearchMethodAsync, new TextSearchProviderOptions
                {
                    SearchTime = TextSearchProviderOptions.TextSearchBehavior.BeforeAIInvoke
                })]
            };

            _mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(agentOptions);
        }

        public async Task RunAsync()
        {
            // MAF automatically calls SearchMethodAsync under the hood before running the LLM
            Console.WriteLine(await _mafAgent.RunAsync("Can I work remotely on Mondays in 2026?"));
        }

        private Task<IEnumerable<TextSearchProvider.TextSearchResult>> SearchMethodAsync(
            string query,
            CancellationToken cancellationToken)
        {
            List<TextSearchProvider.TextSearchResult> results = [];

            // Simple keyword-based matching simulating a basic database row lookup
            if (query.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("work", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new()
                {
                    SourceName = "Corporate Remote Work Policy 2026",                    
                    SourceLink = "https://internal.company.com/policies/remote-2026",
                    Text = "The 2026 remote work policy allows employees to work from anywhere within Poland for up to 3 days per week. Mondays are mandatory office days."
                });
            }

            if (query.Contains("bike", StringComparison.OrdinalIgnoreCase) ||
                query.Contains("benefit", StringComparison.OrdinalIgnoreCase))
            {
                results.Add(new()
                {
                    SourceName = "Employee Benefits Guide",
                    SourceLink = "https://internal.company.com/benefits/bike",
                    Text = "The corporate bicycle benefit covers up to 2000 PLN for purchasing or maintaining cross and mountain bikes."
                });
            }

            return Task.FromResult<IEnumerable<TextSearchProvider.TextSearchResult>>(results);
        }
    }
}