namespace _08_Memory_Mem0
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            using var example = new Mem0ProviderExample(); await example.RunAsync();

            Console.ReadKey();
        }
    }
}
