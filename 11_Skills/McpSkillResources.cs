using ModelContextProtocol.Server;
using System.ComponentModel;

namespace _11_Skills
{
    [McpServerResourceType]
    internal sealed class McpSkillResources
    {
        private const string IndexJson = """
            {
                "$schema": "https://schemas.agentskills.io/discovery/0.2.0/schema.json",
                "skills": [
                    {
                        "name": "warranty-coverage-checker",
                        "type": "skill-md",
                        "description": "Evaluates hardware warranty eligibility based on purchase date and product tier, calculating remaining days and coverage status. Use when asked whether a product is still covered under warranty.",
                        "url": "skill://warranty-coverage-checker/SKILL.md"
                    }
                ]
            }
            """;

        private const string SkillMd = """
            ---
            name: warranty-coverage-checker
            description: Evaluates hardware warranty eligibility based on purchase date and product tier, calculating remaining days and coverage status. Use when asked whether a product is still covered under warranty.
            ---

            ## Usage

            When a user asks whether a product is still covered under warranty:

            1. Extract the product category (e.g., Laptops, Headphones).
            2. Look up the coverage duration in months for that category.
            3. Compute the expiry date as purchase date + warranty months.
            4. Compare the expiry date to today to determine remaining days and coverage status.

            | Product    | Warranty (months) |
            |------------|--------------------|
            | Laptop     | 24                 |
            | Headphones | 12                 |
            """;

        [McpServerResource(UriTemplate = "skill://index.json", Name = "Skill Index", MimeType = "application/json")]
        [Description("SEP-2640 skill discovery index")]
        public static string GetIndex() => IndexJson;

        [McpServerResource(UriTemplate = "skill://warranty-coverage-checker/SKILL.md", Name = "Warranty Coverage Checker Skill", MimeType = "text/markdown")]
        [Description("Warranty coverage checker skill instructions")]
        public static string GetSkillMd() => SkillMd;
    }
}
