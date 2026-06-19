using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.ComponentModel;

namespace _05_RAG_Neo4j
{
    public class CustomToolExample : IAsyncDisposable
    {
        private readonly AIAgent _mafAgent;
        private readonly GraphDb _graphDb;
        private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;

        public CustomToolExample()
        {
            var credential = new DefaultAzureCredential();

            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), credential);

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _graphDb = new GraphDb(Environment.GetEnvironmentVariable("NEO4J_INDEX_NAME")!);

            _embeddingGenerator = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_URI")!), credential)
                .GetEmbeddingClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_EMBEDDING_MODEL")!)
                .AsIEmbeddingGenerator();

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Context Provider test",
                ChatOptions = new ChatOptions
                {
                    Instructions = "Agent using Neo4j (as an ordinary tool)",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                    Tools = [AIFunctionFactory.Create(GetKnowledgeGraphContextAsync)]
                }
            });
        }

        [Description("Queries the knowledge graph to retrieve design principles, best practices, trade-offs, and structural recommendations from the Azure Well-Architected Framework (WAF).")]
        public async Task<string> GetKnowledgeGraphContextAsync(
            [Description("The architectural topic, core concept, or specific Azure Well-Architected Framework pillar/keyword to search for.")] string query)
        {
            var queryVector = await _embeddingGenerator.GenerateVectorAsync(query);
            var graphSearchResult = await _graphDb.GetTopEntitiesAsync(
                queryVector.ToArray(), 
                topK: 3, 
                traversalDepth: 5, 
                minPathScore: 0.85f);

            var context = FormatKnowledgeGraphAsContext(graphSearchResult);
            return context;
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

        private static string FormatKnowledgeGraphAsContext(GraphSearchResult searchResult)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("## Entities");
            foreach (var entity in searchResult.AllEntities)
            {
                sb.AppendLine($"- {entity.Name} ({entity.Type}): {entity.Description}");
            }

            sb.AppendLine();
            sb.AppendLine("## Relationships");
            foreach (var rel in searchResult.Relationships)
            {
                sb.AppendLine($"- {rel.Source} --[{rel.Label}]--> {rel.Target} (weight: {rel.Weight:F2}): {rel.Description}");
            }

            return sb.ToString();
        }

        public async ValueTask DisposeAsync()
        {
            if (_graphDb is IAsyncDisposable disposableDb)
            {
                await disposableDb.DisposeAsync();
            }
        }
    }
}
