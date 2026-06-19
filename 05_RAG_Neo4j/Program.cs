namespace _05_RAG_Neo4j
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new Neo4jContextProviderExample().RunAsync();
            //await using var example = new CustomToolExample(); await example.RunAsync();
        }
    }
}
