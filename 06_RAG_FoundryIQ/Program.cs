namespace _06_RAG_FoundryIQ
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new PullContextViaHttpExample().RunAsync();
            //await using var example = new PullContextViaMcpToolExample(); await example.RunAsync();
        }
    }
}
