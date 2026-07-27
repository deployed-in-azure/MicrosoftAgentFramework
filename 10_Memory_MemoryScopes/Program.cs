using Spectre.Console;

namespace _10_Memory_MemoryScopes
{
    internal class Program
    {
        private const string GYM_STORE_NAME = "gym-store";
        private const string DIET_STORE_NAME = "diet-store";
        private const string SHARED_STORE_NAME = "default-memory-store";

        static async Task Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("Memory Scopes").Color(Color.SteelBlue1));

            //await PerUserPerAgentExample();
            //await PerUserAcrossAgentsExample();

            //await PerAgentAcrossUsersExample();
            await CoordinateMultipleStoresExample();

            Console.ReadKey();
        }

        public static async Task PerUserPerAgentExample()
        {
            AnsiConsole.Write(new Rule("[bold yellow]Example 1 - Per User, Per Agent[/]").RuleStyle("yellow"));
            AnsiConsole.MarkupLine("[white]Memory isolated to a single user and a single agent domain.[/]\n");

            var pattern1Example = new FoundryMemoryScopesExample(
                agentName: "Gym Trainer",
                instructions: "You are a personal gym trainer. Never use more than 60 words in your responses.",
                storeConfigs:
                [
                    (GYM_STORE_NAME, "user-michal")
                ]
            );

            await pattern1Example.EnsureStoredMemoriesDeletedAsync();

            // Conversation 1: Teach the agent in Session 1
            await pattern1Example.RunConversationAsync(
            [
                "I cannot do deadlift but I prefer FBW (max. 3 exercises) and power exercies like squats, soldier press, bench press etc. and I can train only 2 times per week."
            ]);

            // Conversation 2: Teach the agent in Session 2
            await pattern1Example.RunConversationAsync(
            [
                "My left shoulder pinches whenever I do heavy overhead presses."
            ]);

            // Conversation 3: Test cross-session retrieval in Session 3
            await pattern1Example.RunConversationAsync(
            [
                "Prepare a training plan for this week. A friend of mine recommended overhead presses!"
            ]);
        }

        public static async Task PerUserAcrossAgentsExample()
        {
            AnsiConsole.Write(new Rule("[bold yellow]Example 2 - Per User, Across Agents[/]").RuleStyle("yellow"));
            AnsiConsole.MarkupLine("[white]User-centric memory shared across multiple distinct agents.[/]\n");

            // Agent 1
            var gymAgent = new FoundryMemoryScopesExample(
                agentName: "Gym Trainer",
                instructions: "You are a personal gym trainer. Never use more than 60 words in your responses.",
                storeConfigs: [(SHARED_STORE_NAME, "user-michal")]
            );

            // Agent 2
            var dietitianAgent = new FoundryMemoryScopesExample(
                agentName: "Dietitian",
                instructions: "You are a personal dietitian. Never use more than 60 words in your responses.",
                storeConfigs: [(SHARED_STORE_NAME, "user-michal")]
            );

            await gymAgent.EnsureStoredMemoriesDeletedAsync();

            // Conversation 1: Gym Trainer writes context to UserProfileStore
            await gymAgent.RunConversationAsync(
            [
                "I am strictly lactose intolerant and recovering from a knee injury."
            ]);

            // Conversation 2: Dietitian reads directly from the SAME UserProfileStore
            await dietitianAgent.RunConversationAsync(
            [
                "What post-workout snack do you recommend for me today?"
            ]);
        }

        public static async Task PerAgentAcrossUsersExample()
        {
            AnsiConsole.Write(new Rule("[bold yellow]Example 3 - Per Agent, Across Users[/]").RuleStyle("yellow"));
            AnsiConsole.MarkupLine("[white]Agent-centric knowledge aggregated anonymously across all users.[/]\n");

            // Single agent managing shared facility knowledge for all gym members
            var teamAgent = new FoundryMemoryScopesExample(
                agentName: "Gym Assistant",
                instructions: "You are a gym assistant managing facility schedules and rules. Never use more than 60 words in your responses.",
                storeConfigs:
                [
                    (GYM_STORE_NAME, "shared-gym-team-01")
                ]
            );

            await teamAgent.EnsureStoredMemoriesDeletedAsync();

            // Conversation 1: Member 1 adds shared facility information
            await teamAgent.RunConversationAsync(
            [
                "User 1: Please remember that the heavy lifting zone is reserved for private coaching every Tuesday at 10 AM."
            ]);

            // Conversation 2: Member 2 asks about facility availability in a new session
            await teamAgent.RunConversationAsync(
            [
                "User 2: Is the heavy lifting zone open for general use on Tuesday at 10 AM?"
            ]);
        }

        public static async Task CoordinateMultipleStoresExample()
        {
            AnsiConsole.Write(new Rule("[bold yellow]Example 4 - Multi-Store Coordination[/]").RuleStyle("yellow"));
            AnsiConsole.MarkupLine("[white]Single agent synthesizing context from multiple distinct memory stores.[/]\n");

            // Single agent connected to multiple distinct memory stores with the same user scope
            var healthAgent = new FoundryMemoryScopesExample(
                agentName: "Health Coordinator",
                instructions: "You are a holistic health assistant coordinating both fitness and nutrition plans. Never use more than 60 words in your responses.",
                storeConfigs:
                [
                    (GYM_STORE_NAME, "user-michal"),
                    (DIET_STORE_NAME, "user-michal")
                ]
            );

            await healthAgent.EnsureStoredMemoriesDeletedAsync();

            // Session 1: Teach the agent facts across both domain stores
            await healthAgent.RunConversationAsync(
            [
                "I train heavy leg day on Mondays and I follow a strict low-carb diet."
            ]);

            // Session 2: Verify the single agent can query and synthesize context from both stores
            await healthAgent.RunConversationAsync(
            [
                "Based on my workout schedule and dietary rules, what should my Monday post-workout meal look like?"
            ]);
        }
    }
}
