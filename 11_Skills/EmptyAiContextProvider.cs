using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Spectre.Console;

namespace _11_Skills
{
    internal class EmptyAiContextProvider : AIContextProvider
    {
        protected override ValueTask<AIContext> ProvideAIContextAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            var toolAprovalResponse = context.AIContext.Messages?.Select(msg => msg.Contents.FirstOrDefault()).OfType<ToolApprovalResponseContent>().FirstOrDefault();
            if (toolAprovalResponse is not null)
            {
                var table = new Table()
                    .Border(TableBorder.Rounded)
                    .AddColumn("Field")
                    .AddColumn("Value");

                table.AddRow("Approved", $"[green]{toolAprovalResponse.Approved}[/]");
                table.AddRow("Reason", Markup.Escape(toolAprovalResponse.Reason ?? "-"));

                if (toolAprovalResponse.ToolCall is FunctionCallContent functionCall)
                {
                    var arguments = functionCall.Arguments is not null
                        ? string.Join(", ", functionCall.Arguments.Select(kvp => $"{kvp.Key}={kvp.Value}"))
                        : null;

                    table.AddRow("Name", $"[green]{Markup.Escape(functionCall.Name)}[/]");
                    table.AddRow("Arguments", Markup.Escape(arguments ?? "-"));
                }

                AnsiConsole.Write(new Panel(table)
                    .Header("[bold yellow]Tool Approval[/]")
                    .Border(BoxBorder.Rounded)
                    .Expand());
            }

            return base.ProvideAIContextAsync(context, cancellationToken);
        }
    }
}
