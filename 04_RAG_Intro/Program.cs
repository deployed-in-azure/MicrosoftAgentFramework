namespace _04_RAG_Intro
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new SimpleAiContextProviderExample().RunAsync();
            //await new KeywordSearchAiContextProviderExample().RunAsync();
            //await new TextSearchProviderExample().RunAsync();
            //await new HistoryReadingAiContextProviderExample().RunAsync();

        }
    }
}
