using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.AspNetCore.Builder;

namespace _17_HostedAgent_FoundryToolbox
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var projectEndpoint = new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_ENDPOINT") ?? throw new InvalidOperationException("FOUNDRY_PROJECT_ENDPOINT is not set."));
            var deploymentName = Environment.GetEnvironmentVariable("AZURE_AI_MODEL_DEPLOYMENT_NAME") ?? throw new InvalidOperationException("AZURE_AI_MODEL_DEPLOYMENT_NAME is not set.");
            var toolboxName = Environment.GetEnvironmentVariable("TOOLBOX_NAME") ?? throw new InvalidOperationException("TOOLBOX_NAME is not set.");

            var credential = new DefaultAzureCredential();

            var agent = new AIProjectClient(projectEndpoint, credential)
                .AsAIAgent(
                    model: deploymentName,
                    instructions: """
                    You are a helpful agent. Be concise, practical, and precise.
                    If you need a tool that is not in your current list, call tool_search with a description of what you need before responding that you can't help.
                    """,
                    name: "foundry-toolbox-agent");

            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddFoundryResponses(agent);

            // register one or more toolboxes with the agent
            builder.Services.AddFoundryToolboxes(credential, toolboxOptions => 
            {
                // set the API version to use a developer endpoint for testing
                //toolboxOptions.ApiVersion = "23";

                // default
                //toolboxOptions.StrictMode = true;
            }, 
            toolboxNames: [toolboxName]);

            var app = builder.Build();
            app.MapFoundryResponses();
            app.Run();
        }
    }
}
