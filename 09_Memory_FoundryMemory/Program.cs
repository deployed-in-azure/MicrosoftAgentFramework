namespace _09_Memory_FoundryMemory
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new FoundryMemoryProviderExample().RunAsync();
            //await new FoundryMemoryStoreApiExample().RunAsync();

            Console.ReadKey();
        }
    }
}
