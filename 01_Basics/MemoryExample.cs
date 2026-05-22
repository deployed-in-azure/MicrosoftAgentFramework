using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using OpenAI.Chat;
using System.Text.Json;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Persistent Memory and Session Serialization
    /// </summary>
    public class MemoryExample
    {
        private readonly AIAgent _mafAgent;

        public MemoryExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());

            _mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(
                    instructions: "You are a helpful assistant that provides concise answers (max. 10 words).",
                    name: "The Agent with memory");
        }

        public async Task RunAsync()
        {
            var agentSession = await _mafAgent.CreateSessionAsync();
            Console.WriteLine(await _mafAgent.RunAsync("My name is Michal and I help people to build future-ready AI solutions deployed in Azure.", agentSession));
            Console.WriteLine(await _mafAgent.RunAsync("What is my name and what do I do?", agentSession));

            var serializedSession = await _mafAgent.SerializeSessionAsync(agentSession);
            Console.WriteLine();
            Console.WriteLine(JsonSerializer.Serialize(serializedSession, new JsonSerializerOptions { WriteIndented = true }));

            Console.WriteLine("-----\n Persist session to database");
            Console.WriteLine("-----\n Read session from database and continue \n-----");

            var deserializedSession = await _mafAgent.DeserializeSessionAsync(serializedSession);
            Console.WriteLine(await _mafAgent.RunAsync("Can you repeat please?", deserializedSession));
        }
    }
}
