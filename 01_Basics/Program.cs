namespace _01_Basics
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new HelloWorldExample().RunAsync();
            //await new StructuredOutputExample().RunAsync();
            //await new MessageTypesExample().RunAsync();
            //await new ChatRolesExample().RunAsync();
            //await new AgentSessionExample().RunAsync();
            //await new MemoryExample().RunAsync();
            //await new ObservabilityExample().RunAsync();
            //await new ToolsExample().RunAsync();
            //await new SimpleRagExample().RunAsync();
            //await new WorkflowsExample().RunAsync();

            Console.ReadKey();
        }
    }
}
