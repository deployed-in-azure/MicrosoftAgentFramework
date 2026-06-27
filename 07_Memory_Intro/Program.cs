namespace _07_Memory_Intro
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new ChatHistoryMemoryProviderExample().RunAsync();

            Console.ReadKey();
        }
    }
}
