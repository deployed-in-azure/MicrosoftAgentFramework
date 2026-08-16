using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.AspNetCore.Builder;

namespace _13_Hosting_Intro_Docker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var projectEndpoint = new Uri(
                Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT")
                ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set."));

            var deployment =
                Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME")
                ?? throw new InvalidOperationException("AZURE_AI_MODEL_DEPLOYMENT_NAME is not set.");

            var agent = new AIProjectClient(projectEndpoint, new DefaultAzureCredential())
                .AsAIAgent(
                    model: deployment,
                    instructions: """
                    You are an enthusiastic basketball expert. Share interesting, engaging, and accurate facts, trivia, and historical highlights about the NBA. Keep your answers concise, entertaining, and clear.
                    """,
                    name: "nba-facts-agent",
                    description: "An AI assistant that shares interesting facts, trivia, and history about the NBA.");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddFoundryResponses(agent);

            var app = builder.Build();
            app.MapFoundryResponses();
            app.Run();
        }
    }
}
