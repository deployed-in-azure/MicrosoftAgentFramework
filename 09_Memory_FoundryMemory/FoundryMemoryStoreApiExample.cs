using Azure.AI.OpenAI;
using Azure.AI.Projects;
using Azure.AI.Projects.Memory;
using Azure.Identity;
using OpenAI.Responses;
using Spectre.Console;
using System.Diagnostics;

namespace _09_Memory_FoundryMemory
{
    internal class FoundryMemoryStoreApiExample
    {
        private readonly AIProjectClient _foundryProjectClient;
        private readonly string _scope = "Michal-0718-01";

        public FoundryMemoryStoreApiExample()
        {
            var credential = new DefaultAzureCredential();
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), credential);

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClientWithStoredOutputDisabled(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            _foundryProjectClient = new AIProjectClient(new Uri(Environment.GetEnvironmentVariable("FOUNDRY_PROJECT_NAME")!), credential);
        }

        public async Task RunAsync()
        {
            await WaitUntilScopeDeletedAsync();            

            var firstUpdateId = await UpdateAsync(
                message: "I live in Europe",
                updateDelay: TimeSpan.Zero,
                previousUpdateId: null);

            await WaitUntilUpdatedAsync(firstUpdateId);

            var secondUpdateId = await UpdateAsync(
                message: "I live in Poland",
                updateDelay: TimeSpan.Zero,
                previousUpdateId: firstUpdateId);

            await WaitUntilUpdatedAsync(secondUpdateId);

            var thirdUpdateId = await UpdateAsync(
                message: "I live next to Krakow",
                updateDelay: TimeSpan.FromSeconds(5),
                previousUpdateId: null);

            var fourthUpdateId = await UpdateAsync(
                message: "I live west of Krakow",
                updateDelay: TimeSpan.Zero,
                previousUpdateId: thirdUpdateId);

            await WaitUntilUpdatedAsync(fourthUpdateId);

            AnsiConsole.MarkupLine("\n[dim]Checking status of the superseded 3rd update...[/]");

            await GetUpdateResultAsync(thirdUpdateId, Stopwatch.StartNew(), true);

            var fifthUpdateId = await UpdateAsync(
                items:
                [
                    ResponseItem.CreateUserMessageItem("Help me plan a weekend getaway."),
                    ResponseItem.CreateAssistantMessageItem("Let me check flight deals to major international capitals."),
                    ResponseItem.CreateUserMessageItem("No, that is incorrect. When handling weekend travel plans, do not look for flights. Follow this procedure instead: First, check my current location. Second, calculate a 100km boundary. Third, query local driving destinations within that radius."),
                    ResponseItem.CreateAssistantMessageItem("Understood. I will adopt this location-first 100km local filtering workflow for all future weekend trip requests.")
                ],
                updateDelay: TimeSpan.Zero,
                previousUpdateId: null);

            await WaitUntilUpdatedAsync(fifthUpdateId);

            var memories = await _foundryProjectClient.MemoryStores.SearchMemoriesAsync(
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                options: new Azure.AI.Projects.Memory.MemorySearchOptions(_scope)
                {
                    Items = { ResponseItem.CreateUserMessageItem("Plan a short weekend trip.") },
                    ResultOptions = new Azure.AI.Projects.Memory.MemorySearchResultOptions()
                    {
                        MaxMemories = 10
                    }
                });

            AnsiConsole.Write(new Rule("[dim]Memory Search Results[/]").RuleStyle("dim"));

            foreach (var memory in memories.Value.Memories.Select(m => m.MemoryItem))
            {
                var panel = new Panel(
                    new Rows(
                        new Markup($"[dim]Id:[/]        {Markup.Escape(memory.MemoryId)}"),
                        new Markup($"[dim]Type:[/]      {Markup.Escape(memory.GetType().Name)}"),
                        new Markup($"[dim]Scope:[/]     {Markup.Escape(memory.Scope)}"),
                        new Markup($"[dim]Content:[/]   {Markup.Escape(memory.Content ?? "")}"),
                        new Markup($"[dim]UpdatedAt:[/] {memory.UpdatedAt}")
                    ))
                {
                    Border = BoxBorder.Rounded,
                    Padding = new Padding(1, 0)
                };

                AnsiConsole.Write(panel);
            }

            AnsiConsole.Write(new Rule().RuleStyle("dim"));

            return;
        }

        private async Task WaitUntilScopeDeletedAsync()
        {
            AnsiConsole.MarkupLine("\n[dim]Deleting scope...[/]");

            var deleteResult = await _foundryProjectClient.MemoryStores.DeleteScopeAsync(
                                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME"),
                                _scope);

            AnsiConsole.MarkupLine($"  [dim]IsDeleted:[/] {deleteResult.Value.IsDeleted}");
        }

        private async Task<string> UpdateAsync(IReadOnlyList<ResponseItem> items, TimeSpan updateDelay, string? previousUpdateId)
        {
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine($"[dim]Conversation ({items.Count} items)[/]");
            AnsiConsole.Write(new Rule());

            var options = new MemoryUpdateOptions(_scope)
            {
                UpdateDelay = (int)updateDelay.TotalMilliseconds,
                PreviousUpdateId = previousUpdateId,
            };

            foreach (var item in items)
            {
                options.Items.Add(item);
            }

            var updateResult = await _foundryProjectClient.MemoryStores.UpdateMemoriesAsync(
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                options);

            AnsiConsole.MarkupLine(
                $"  [dim]UpdateId:[/]         {Markup.Escape(updateResult.Value.UpdateId)}\n" +
                $"  [dim]Status:[/]           {updateResult.Value.Status}\n" +
                $"  [dim]SupersededBy:[/]     {Markup.Escape(updateResult.Value.SupersededBy ?? "None")}");

            await PrintCapturedMemoryOperationsAsync(updateResult.Value.UpdateId);

            return updateResult.Value.UpdateId;
        }

        private async Task<string> UpdateAsync(string message, TimeSpan updateDelay, string? previousUpdateId)
        {
            AnsiConsole.Write(new Rule());
            AnsiConsole.MarkupLine($"[green]User:[/] [green]{Markup.Escape(message)}[/]");
            AnsiConsole.Write(new Rule());

            var updateResult = await _foundryProjectClient.MemoryStores.UpdateMemoriesAsync(
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                options: new MemoryUpdateOptions(_scope)
                {
                    UpdateDelay = (int) updateDelay.TotalMilliseconds,
                    PreviousUpdateId = previousUpdateId,
                    Items =
                    {
                        ResponseItem.CreateUserMessageItem(message)
                    }
                });

            AnsiConsole.MarkupLine(
                $"  [dim]UpdateId:[/]         {Markup.Escape(updateResult.Value.UpdateId)}\n" +
                $"  [dim]Status:[/]           {updateResult.Value.Status}\n" +
                $"  [dim]SupersededBy:[/]     {Markup.Escape(updateResult.Value.SupersededBy ?? "None")}");

            await PrintCapturedMemoryOperationsAsync(updateResult.Value.UpdateId);

            return updateResult.Value.UpdateId;
        }

        private async Task WaitUntilUpdatedAsync(string updateId, int pollingIntervalInMilliseconds = 500)
        {
            AnsiConsole.MarkupLine("\n[dim]Polling status...[/]");

            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                var (status, elapsed) = await GetUpdateResultAsync(updateId, stopwatch);
                if (status is MemoryStoreUpdateStatus.Completed or MemoryStoreUpdateStatus.Superseded)
                {
                    stopwatch.Stop();
                    AnsiConsole.MarkupLine($"\n[dim]Completed in [/][bold]{elapsed.TotalSeconds:F2}s[/]");
                    break;
                }

                if (status == MemoryStoreUpdateStatus.Failed)
                {
                    throw new InvalidOperationException($"Memory update operation '{updateId}' failed");
                }

                if (status is MemoryStoreUpdateStatus.Queued or MemoryStoreUpdateStatus.InProgress)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(pollingIntervalInMilliseconds));
                }
                else
                {
                    throw new InvalidOperationException($"Unknown update status '{status}' for update '{updateId}'.");
                }
            }

            await PrintCapturedMemoryOperationsAsync(updateId);
        }

        private async Task<(MemoryStoreUpdateStatus Status, TimeSpan Elapsed)> GetUpdateResultAsync(
            string updateId,
            Stopwatch stopwatch,
            bool printFullUpdateResultInfo = false)
        {
            var updateResult = await _foundryProjectClient.MemoryStores.GetUpdateResultAsync(
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                updateId);

            var status = updateResult.Value.Status;
            var elapsed = stopwatch.Elapsed;

            if (printFullUpdateResultInfo)
            {
                AnsiConsole.MarkupLine(
                $"  [dim]UpdateId:[/]         {Markup.Escape(updateResult.Value.UpdateId)}\n" +
                $"  [dim]Status:[/]           {updateResult.Value.Status}\n" +
                $"  [dim]SupersededBy:[/]     {Markup.Escape(updateResult.Value.SupersededBy ?? "None")}");
            }
            else
            {
                AnsiConsole.MarkupLine($"  [dim]Status:[/] {status}  [dim]({elapsed.TotalSeconds:F2}s)[/]");
            }

            return (status, elapsed);
        }

        private async Task PrintCapturedMemoryOperationsAsync(string updateId)
        {
            var updateResult = await _foundryProjectClient.MemoryStores.GetUpdateResultAsync(
                Environment.GetEnvironmentVariable("FOUNDRY_MEMORY_STORE_NAME")!,
                updateId);

            if (updateResult.Value.Details?.MemoryOperations is null or { Count: 0 })
            {
                AnsiConsole.MarkupLine($"\n  [dim]No memory operations found for update '{Markup.Escape(updateId)}'.[/]");
                return;
            }

            foreach (var memoryOperation in updateResult.Value.Details?.MemoryOperations ?? [])
            {
                AnsiConsole.MarkupLine(
                    $"  [dim]Kind:[/]      {memoryOperation.Kind}\n" +
                    $"  [dim]Type:[/]      {memoryOperation.GetType().Name}\n" +
                    $"  [dim]MemoryId:[/]  {Markup.Escape(memoryOperation.MemoryItem.MemoryId)}\n" +
                    $"  [dim]Scope:[/]     {Markup.Escape(memoryOperation.MemoryItem.Scope)}\n" +
                    $"  [dim]Content:[/]   {Markup.Escape(memoryOperation.MemoryItem.Content ?? "")}\n" +
                    $"  [dim]UpdatedAt:[/] {memoryOperation.MemoryItem.UpdatedAt}\n");
            }
        }
    }
}
