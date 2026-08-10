using Azure;
using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Spectre.Console;
using System.Diagnostics;
using System.Text.Json;

namespace _11_Skills
{
    public class FileSkillsExample
    {
        private readonly AIAgent _mafAgent;

        public FileSkillsExample()
        {
            var openAiClient = new AzureOpenAIClient(new Uri(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_RESPONSES_CLIENT_URI")!), new DefaultAzureCredential());

            var responesClient = openAiClient
                .GetResponsesClient()
                .AsIChatClient(Environment.GetEnvironmentVariable("AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME")!);

            var skillsProvider = new AgentSkillsProvider(
                Path.Combine(AppContext.BaseDirectory, "skills"),
                scriptRunner:  MyScriptRunnerAsync,
                fileOptions: new AgentFileSkillsSourceOptions()
                {
                    //SearchDepth = 2,
                    //ResourceFilter = context => context.RelativeFilePath.StartsWith("references/"),
                    //AllowedResourceExtensions = [".md", ".txt"],
                    //ScriptFilter = context => context.RelativeFilePath.StartsWith("tools/"),
                    //AllowedScriptExtensions = [".py", ".cs"]
                });

            _mafAgent = new ChatClientAgent(responesClient, new ChatClientAgentOptions()
            {
                Name = "Agent with skills",
                ChatOptions = new ChatOptions
                {
                    Instructions = "You are a helpful assistant that always responds concisely."
                },
                AIContextProviders = [skillsProvider, new EmptyAiContextProvider()]
            })
            .AsBuilder()
            .UseToolApproval(new ToolApprovalAgentOptions
            {
                //AutoApprovalRules = [AgentSkillsProvider.AllToolsAutoApprovalRule],
                AutoApprovalRules = [AgentSkillsProvider.ReadOnlyToolsAutoApprovalRule]
            })
            .Build();
        }

        public async Task RunAsync()
        {
            //var userQuery = "I bought a Laptop on 2025-01-15. Is it still covered under warranty?";

            // use that prompt to invoke other combination of resources and scripts within the same skill folder
            var userQuery = "My Laptop's standard warranty is expiring soon. Calculate the cost to purchase a 3-year extended warranty.";
            Console.WriteLine($"User: {userQuery}\n");

            var session = await _mafAgent.CreateSessionAsync();
            var response = await _mafAgent.RunAsync(userQuery, session);

            response = await ApproveToolCallsIfNeeded(response, session);

            Console.WriteLine($"Agent: {response}");
        }

        private async Task<object?> MyScriptRunnerAsync(
            AgentFileSkill skill,
            AgentFileSkillScript script,
            JsonElement? arguments, // Contains the raw JSON arguments requested by the LLM
            IServiceProvider? serviceProvider,
            CancellationToken cancellationToken)
        {
            var scriptPath = script.FullPath;

            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    ArgumentList = { "-NoProfile", "-ExecutionPolicy", "Bypass", "-File", scriptPath },
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WorkingDirectory = Path.GetDirectoryName(scriptPath)
                }
            };

            if (arguments is { ValueKind: JsonValueKind.Array } json)
            {
                foreach (var element in json.EnumerateArray())
                {
                    if (element.ValueKind != JsonValueKind.String)
                    {
                        throw new InvalidOperationException("All array elements must be strings.");
                    }

                    process.StartInfo.ArgumentList.Add(element.GetString()!);
                }
            }

            process.Start();

            var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
            var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);
            await Task.WhenAll(outputTask, errorTask);

            if (process.ExitCode != 0)
            {
                AnsiConsole.MarkupLine($"[red]Script `{Markup.Escape(script.Name)}` failed with exit code {process.ExitCode}.[/]");
                AnsiConsole.WriteLine(errorTask.Result);
                return $"Script failed with exit code {process.ExitCode}. Error: {errorTask.Result}";
            }

            AnsiConsole.Write(new Panel(Markup.Escape(outputTask.Result))
                .Header($"[green]{Markup.Escape(script.Name)} result[/]")
                .Border(BoxBorder.Rounded));

            return outputTask.Result;
        }

        private async Task<AgentResponse> ApproveToolCallsIfNeeded(AgentResponse response, AgentSession session)
        {
            var approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            while (approvalRequests.Count > 0)
            {
                var userInputResponses = approvalRequests
                    .ConvertAll(request =>
                    {
                        var toolCall = (FunctionCallContent)request.ToolCall;
                        var arguments = string.Join(", ", toolCall.Arguments?.Select(a => $"{a.Key}: {a.Value}") ?? []);
                        bool approved = AnsiConsole.Confirm($"Approve [yellow]`{toolCall.Name}`[/] tool with arguments: [grey]{Markup.Escape(arguments)}[/]?");
                        return new ChatMessage(ChatRole.User, [request.CreateResponse(approved, reason: "Approved by Michal")]);
                    });

                response = await _mafAgent.RunAsync(userInputResponses, session);
                approvalRequests = response.Messages.SelectMany(m => m.Contents).OfType<ToolApprovalRequestContent>().ToList();
            }

            return response;
        }
    }
}
