namespace _11_Skills
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new FileSkillsExample().RunAsync();
            //await new CodeSkillsExample().RunAsync();
            //await new ClassSkillsExample().RunAsync();
            //await new McpSkillsExample().RunAsync(args);

            Console.ReadKey();
        }
    }
}
