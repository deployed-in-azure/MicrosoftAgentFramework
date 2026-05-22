using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI.Chat;
using System.ComponentModel;
using System.Text.Json;

namespace _01_Basics
{
    /// <summary>
    /// Microsoft Agent Framework Tutorial, Example: Extending Capabilities with Tools (Function Calling)
    /// </summary>
    public class ToolsExample
    {
        private readonly AIAgent _mafAgent;

        public ToolsExample()
        {
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CLIENT_URI")!),
                new DefaultAzureCredential());


            _mafAgent = openAiClient
                .GetChatClient(deploymentName: Environment.GetEnvironmentVariable("AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME")!)
                .AsAIAgent(
                    instructions: """
                        You are a brief, helpful travel assistant. 
                        Use the provided tools to check weather, convert currency, and log expenses when requested. 
                        For multi-step requests, chain the necessary tools sequentially. 
                        Keep responses friendly, accurate, and concise.
                    """,
                    name: "Travel Companion Agent",
                    description: "A smart, helpful travel assistant that assists users with weather checks, currency conversions, and expense logging during their trips.",
                    tools: [
                        AIFunctionFactory.Create(GetWeather),
                        AIFunctionFactory.Create(ConvertToUsd),
                        AIFunctionFactory.Create(LogExpense)
                    ]);
        }

        public async Task RunAsync()
        {
            Console.WriteLine(await _mafAgent.RunAsync("Hey, I'm heading out for a walk in Tokyo right now. Do I need to bring a coat or an umbrella?"));
            Console.WriteLine("---\n");

            Console.WriteLine(await _mafAgent.RunAsync("I'm looking at a souvenir online that costs 4500 Japanese Yen. How much is that going to cost me in US dollars?"));
            Console.WriteLine("---\n");

            Console.WriteLine(await _mafAgent.RunAsync("Please add a 15.50 charge for a museum ticket to my travel expense tracker."));
            Console.WriteLine("---\n");

            var agentSession = await _mafAgent.CreateSessionAsync();

            var response = await _mafAgent.RunAsync("""
                I am currently in London and want to go see a street performance, but only if it's not raining. 
                If the weather is clear, I plan to buy a ticket that costs 25 GBP. 
                Check the weather for me first. 
                If it's good, figure out how much that ticket costs in USD and log it to my budget as 'Street Concert'.
                """,
                agentSession);

            Console.WriteLine(response.Text);
            Console.WriteLine($"*** Messages count: {response.Messages.Count} ***");

            Console.WriteLine();
            Console.WriteLine(JsonSerializer.Serialize(await _mafAgent.SerializeSessionAsync(agentSession), new JsonSerializerOptions { WriteIndented = true }));
        }

        [Description("Gets the current weather forecast for a specific city to help the user pack or plan.")]
        private string GetWeather([Description("The name of the city, e.g., 'London', 'Paris'")] string city)
        {
            // Mock implementation for tutorial purposes
            var random = new Random();
            string[] conditions = { "Sunny", "Rainy", "Cloudy", "Windy" };
            string condition = conditions[random.Next(conditions.Length)];
            int temperature = random.Next(15, 28);

            return $"The current weather in {city} is {condition} and {temperature}°C.";
        }

        [Description("Converts an amount from a foreign currency to US Dollars (USD).")]
        private string ConvertToUsd(
            [Description("The amount of money to convert")] double amount,
            [Description("The 3-letter currency code being converted from, e.g., 'EUR', 'GBP'")] string fromCurrency)
        {
            // Mock static exchange rates for simplicity
            double rate = fromCurrency.ToUpper() switch
            {
                "EUR" => 1.10,
                "GBP" => 1.30,
                "JPY" => 0.0065,
                _ => 1.00
            };

            double convertedAmount = Math.Round(amount * rate, 2);
            return $"{amount} {fromCurrency.ToUpper()} is approximately {convertedAmount} USD (Rate: {rate}).";
        }

        [Description("Logs a travel expense to the user's budget spreadsheet or database.")]
        private string LogExpense(
            [Description("The item or service purchased, e.g., 'Umbrella', 'Dinner'")] string item,
            [Description("The cost of the item in USD")] double amountInUsd)
        {
            // Mock action: In a real app, this would write to a database, API, or CSV file

            return $"Successfully logged '{item}' costing ${amountInUsd} to your travel budget.";
        }
    }
}
