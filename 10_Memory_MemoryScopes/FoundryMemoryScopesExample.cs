using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry;
using Microsoft.Extensions.AI;
using OpenAI.Responses;
using Spectre.Console;

namespace _10_Memory_MemoryScopes
{
    internal class FoundryMemoryScopesExample
    {
        private readonly AIAgent _agent;
        private readonly List<FoundryMemoryProvider> _memoryProviders = [];
        private readonly string _agentName;
        private int _conversationCount = 0;

        public FoundryMemoryScopesExample(
            string agentName,
            string instructions,
            IReadOnlyList<(string StoreName, string ScopeId)> storeConfigs)
        {
            _agentName = agentName;
            var credential = new DefaultAzureCredential();
            var openAiClient = new AzureOpenAIClient(
                new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!),
                credential
            );

            var responseClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!)
                .AsBuilder()
                .Use(inner => new InspectingChatClient(inner))
                .Build();

            var projectClient = new AIProjectClient(
                new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_NAME")!),
                credential
            );

            foreach (var (storeName, scopeId) in storeConfigs)
            {
                _memoryProviders.Add(new FoundryMemoryProvider(
                    projectClient,
                    storeName,
                    stateInitializer: _ => new(new FoundryMemoryProviderScope(scopeId)),
                    new FoundryMemoryProviderOptions()
                    {
                        StateKey = $"{typeof(FoundryMemoryProvider).Name}-{storeName}"
                    }
                ));
            }

            _agent = new ChatClientAgent(responseClient, new ChatClientAgentOptions()
            {
                Name = agentName,
                ChatOptions = new ChatOptions
                {
                    Instructions = instructions,
                    Reasoning = new ReasoningOptions() { Effort = ReasoningEffort.Low, Output = ReasoningOutput.None }
                },
                AIContextProviders = _memoryProviders
            });
        }

        public async Task EnsureStoredMemoriesDeletedAsync()
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("red"))
                .StartAsync("[white]Clearing memory stores...[/]", async _ =>
                {
                    AgentSession session = await _agent.CreateSessionAsync();
                    await Task.WhenAll(_memoryProviders.Select(p => p.EnsureStoredMemoriesDeletedAsync(session)));
                });

            AnsiConsole.MarkupLine("[red]Memory stores cleared.[/]\n");
        }

        public async Task RunConversationAsync(IReadOnlyList<string> prompts)
        {
            _conversationCount++;
            AgentSession session = await _agent.CreateSessionAsync();

            AnsiConsole.Write(
                new Rule($"[bold steelblue1]Conversation {_conversationCount}[/] [white]| {Markup.Escape(_agentName)}[/]")
                    .RuleStyle("steelblue1")
                    .LeftJustified()
            );

            foreach (var prompt in prompts)
            {
                AnsiConsole.MarkupLine($"[bold cyan] User:[/] {Markup.Escape(prompt)}");

                string response = string.Empty;
                await AnsiConsole.Status()
                    .Spinner(Spinner.Known.Dots2)
                    .SpinnerStyle(Style.Parse("green"))
                    .StartAsync($"[white]{Markup.Escape(_agentName)} is thinking...[/]", async _ =>
                    {
                        var agentResponse = await _agent.RunAsync(prompt, session);
                        response = agentResponse?.ToString() ?? string.Empty;
                    });

                AnsiConsole.MarkupLine($"[bold green]Agent:[/] {Markup.Escape(response)}");
                AnsiConsole.WriteLine();
            }

            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("yellow"))
                .StartAsync("[white]Indexing memory...[/]", async _ =>
                {
                    await Task.WhenAll(_memoryProviders.Select(p => p.WhenUpdatesCompletedAsync(pollingInterval: TimeSpan.FromSeconds(1))));
                });

            AnsiConsole.MarkupLine("[yellow]Memory indexed.[/]\n");
        }

        internal class InspectingChatClient(IChatClient innerClient) : DelegatingChatClient(innerClient)
        {
            public override async Task<ChatResponse> GetResponseAsync(
                IEnumerable<ChatMessage> messages,
                ChatOptions? options = null,
                CancellationToken cancellationToken = default)
            {
                return await base.GetResponseAsync(messages, options, cancellationToken);
            }
        }
    }
}