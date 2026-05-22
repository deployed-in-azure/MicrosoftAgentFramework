using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Multi-Turn Conversation State with Agent Sessions
    /// </summary>
    public class AgentSessionExample
    {
        private readonly AIAgent _mafAgent;

        public AgentSessionExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            _mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(
                    instructions: "You are a helpful assistant that provides concise answers (max. 10 words).",
                    name: "The Session Agent",
                    description: "This agent keeps the conversation history within a session");
        }

        public async Task RunAsync()
        {
            var initialPrompt = "My name is Michal and I help people to build future-ready AI solutions deployed in Azure.";
            var followUpQuestion = "What is my name and what do I do?";

            Console.WriteLine("Without SESSION");
            await CallAgentAndLogTokensUsage(initialPrompt);
            await CallAgentAndLogTokensUsage(followUpQuestion);

            Console.WriteLine("\n---");

            var agentSession = await _mafAgent.CreateSessionAsync();
            await CallAgentAndLogTokensUsage(initialPrompt, agentSession);
            await CallAgentAndLogTokensUsage(followUpQuestion, agentSession);
        }

        private async Task CallAgentAndLogTokensUsage(string message, AgentSession? agentSession = null)
        {
            var agentResponse = await _mafAgent.RunAsync(message, agentSession);

            Console.WriteLine(agentResponse.Text);
            Console.WriteLine($"Input Tokens: {agentResponse.Usage!.InputTokenCount}, Output Tokens: {agentResponse.Usage.OutputTokenCount}\n\n");
        }
    }
}
