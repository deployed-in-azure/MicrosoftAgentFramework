using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Neo4j.AgentFramework.GraphRAG;
using OpenAI.Responses;

namespace _05_RAG_Neo4j
{
    public class Neo4jContextProviderExample
    {
        private readonly AIAgent _mafAgent;

        public Neo4jContextProviderExample()
        {
            var defaultCredential = new DefaultAzureCredential();

            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                defaultCredential);

            var responsesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            var embeddingGenerator = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_URI")!), defaultCredential)
                .GetEmbeddingClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_EMBEDDING_MODEL")!)
                .AsIEmbeddingGenerator();

            _mafAgent = new ChatClientAgent(responsesClient, new ChatClientAgentOptions()
            {
                Name = "Agent using Neo4j (as a context provider)",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant.",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                },
                AIContextProviders = [Neo4jContextProvider.Create(
                    uri: Environment.GetEnvironmentVariable("NEO4J_URI")!,
                    username: Environment.GetEnvironmentVariable("NEO4J_USERNAME")!,
                    password: Environment.GetEnvironmentVariable("NEO4J_PASSWORD")!,
                    new Neo4jContextProviderOptions()
                    {
                        IndexName = Environment.GetEnvironmentVariable("NEO4J_INDEX_NAME")!,
                        IndexType = IndexType.Vector,
                        EmbeddingGenerator = embeddingGenerator,
                        TopK = 3,
                        RetrievalQuery = """
                            WITH node, score, [label IN labels(node) WHERE label <> 'Entity'] AS filteredLabels
                            RETURN 
                                "Name: " + node.name + "\n" +
                                "Description: " + node.description + "\n" +
                                "Tags: " + reduce(s = "", x IN filteredLabels | s + (CASE WHEN s = "" THEN "" ELSE ", " END) + x) AS text, 
                                score
                            """
                    })
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
                Console.WriteLine($"Agent: {agentResponse}\n");
            }
        }
    }
}
