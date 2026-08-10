using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.ComponentModel;
using System.Text.Json;

namespace _11_Skills
{
    public class ClassSkillsExample
    {
        private readonly AIAgent _mafAgent;

        public ClassSkillsExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            var classWarrantySkill = new WarrantyCoverageCheckerSkill();
            var skillsProvider = new AgentSkillsProvider(classWarrantySkill);

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent with class skill",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant that always responds concisely."
                },
                AIContextProviders = [skillsProvider, new EmptyAiContextProvider()]
            })
            .AsBuilder()
            .UseToolApproval(new ToolApprovalAgentOptions
            {
                AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
            })
            .Build();
        }

        public async Task RunAsync()
        {
            var userQuery = "I bought a Laptop on 2025-01-15. Is it still covered under warranty?";
            Console.WriteLine($"User: {userQuery}\n");

            var session = await _mafAgent.CreateSessionAsync();
            var response = await _mafAgent.RunAsync(userQuery, session);
            Console.WriteLine($"Agent: {response}");
        }
    }

    internal class WarrantyCoverageCheckerSkill : AgentClassSkill<WarrantyCoverageCheckerSkill>
    {
        public override AgentSkillFrontmatter Frontmatter { get; } = new(
            name: "warranty-coverage-checker",
            description: "Evaluates hardware warranty eligibility based on purchase date and product tier, calculating remaining days and coverage status.");

        protected override string Instructions => """
            # Warranty Coverage Checker Guidelines

            Use this skill when a user asks whether a product is still covered under warranty.

            ## Workflow Execution Steps
            1. Extract the product category (e.g., Laptops, Headphones).
            2. Read `product_warranties` to find the coverage duration in months.
            3. Execute the `calculate_warranty_status` script with the purchase date and warranty duration in months to compute remaining days and coverage status.
            """;

        [AgentSkillResource("product_warranties")]
        public string ProductWarranties => """
            {
              "Laptop": { "warrantyMonths": 24 },
              "Headphones": { "warrantyMonths": 12 }
            }
            """;

        [AgentSkillScript("calculate_warranty_status")]
        [Description("Calculates remaining warranty coverage days and status.")]
        public string CalculateWarrantyStatus(string purchaseDate, int warrantyMonths)
        {
            if (DateTime.TryParse(purchaseDate, out var date))
            {
                var expiryDate = date.AddMonths(warrantyMonths);
                var remainingDays = (expiryDate - DateTime.UtcNow).Days;
                var isCovered = remainingDays > 0;

                return JsonSerializer.Serialize(new
                {
                    purchaseDate = date.ToString("yyyy-MM-dd"),
                    expiryDate = expiryDate.ToString("yyyy-MM-dd"),
                    remainingDays = Math.Max(0, remainingDays),
                    isCovered
                });
            }
            return JsonSerializer.Serialize(new { error = "Invalid date format" });
        }
    }
}