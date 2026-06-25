using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents.KnowledgeBases;
using Azure.Search.Documents.KnowledgeBases.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using System.ComponentModel;
using System.Text;

namespace _06_RAG_FoundryIQ
{
    public class PullContextViaHttpExample
    {
        private readonly AIAgent _mafAgent;
        private readonly KnowledgeBaseRetrievalClient _knowledgeBaseRetrievalClient;

        public PullContextViaHttpExample()
        {
            var credential = new DefaultAzureCredential();
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), credential);

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _knowledgeBaseRetrievalClient = new KnowledgeBaseRetrievalClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_URI")!),
                knowledgeBaseName: Environment.GetEnvironmentVariable("AZURE_AI_SEARCH_KNOWLEDGE_BASE")!,
                credential);

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent powered by Foundry IQ data via HTTP",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful agent which provides useful information about Contoso and returns references to data source if provided",
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.None, Output = ReasoningOutput.None },
                    Tools = [AIFunctionFactory.Create(GetDataFromKnowledgeBaseAsync)]
                }
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

        [Description("Searches for information about Contoso's Microsoft cloud architecture, including services, configurations, and design decisions.")]
        public async Task<string> GetDataFromKnowledgeBaseAsync([Description("The search query used to retrieve relevant information about MSFT cloud architecture for Contoso from the knowledge base.")] string query)
        {
            var outputMode = KnowledgeRetrievalOutputMode.ExtractiveData;

            var kbRetrievalRequest = new KnowledgeBaseRetrievalRequest()
            {
                Intents =
                {
                    new KnowledgeRetrievalSemanticIntent(query)
                },
                KnowledgeSourceParams =
                {
                    //new SearchIndexKnowledgeSourceParams("ksourceName")
                    //{
                    //    //AlwaysQuerySource = true,
                    //    //IncludeReferences = true,
                    //    //IncludeReferenceSourceData = true,
                    //    //RerankerThreshold = 0.70f,

                    //    FilterAddOn = "Country eq 'Poland' and City eq 'Krakow'"
                    //},
                    //new AzureBlobKnowledgeSourceParams("ks-blob-storage")
                    //{
                    //    //AlwaysQuerySource = false,
                    //    //IncludeReferences = true,
                    //    //IncludeReferenceSourceData = true,
                    //    //RerankerThreshold = 3.0f
                    //},
                },
                OutputMode = outputMode
            };

            var result = await _knowledgeBaseRetrievalClient.RetrieveAsync(kbRetrievalRequest);
            var json = result.GetRawResponse().Content.ToString();

            if (outputMode == KnowledgeRetrievalOutputMode.ExtractiveData)
            {
                var sb = new StringBuilder();
                sb.AppendLine("# Result");

                foreach (KnowledgeBaseMessage message in result.Value.Response)
                {
                    foreach (KnowledgeBaseMessageTextContent textContent in message.Content.OfType<KnowledgeBaseMessageTextContent>())
                    {
                        sb.AppendLine(textContent.Text);
                    }
                }

                sb.AppendLine("\n# References");

                foreach (KnowledgeBaseReference reference in result.Value.References)
                {
                    sb.AppendLine($"Id (ref_id): {reference.Id}");

                    if (reference is KnowledgeBaseAzureBlobReference blobReference)
                    {
                        sb.AppendLine($"Source URL: {blobReference.BlobUrl}");
                    }
                    sb.AppendLine("---");
                }

                var context = sb.ToString();
                return context;
            }

            var synthesisedAnswer = (result.Value.Response[0].Content[0] as KnowledgeBaseMessageTextContent)!.Text;
            return synthesisedAnswer;
        }
    }
}
