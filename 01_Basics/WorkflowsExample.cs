using Microsoft.Agents.AI.Workflows;

namespace _01_Basics
{
/// <summary>
/// Microsoft Agent Framework Tutorial, Example: Structured Processing with Workflows
/// </summary>
    public class WorkflowsExample
    {
        public async Task RunAsync()
        {
            Func<string, string> trimFunc = s => string.Join(' ', s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
            var trim = trimFunc.BindAsExecutor("TrimExtraWhitespaceExecutor");

            var wordCount = new WordCountExecutor();

            var builder = new WorkflowBuilder(trim);
            builder.AddEdge(trim, wordCount).WithOutputFrom(wordCount);
            var workflow = builder.Build();

            await using var run = await InProcessExecution.RunAsync(workflow, "  Deployed   in    Azure   ");
            foreach (var completedEvent in run.NewEvents.OfType<ExecutorCompletedEvent>())
            {
                Console.WriteLine($"Executor [{completedEvent.ExecutorId}] > Result: {completedEvent.Data}");
            }
        }

        private class WordCountExecutor() : Executor<string, string>("WordCountExecutor")
        {
            public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken cancellationToken = default)
            {
                int count = message.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
                return ValueTask.FromResult($"'{message}' contains {count} word(s).");
            }
        }
    }
}
