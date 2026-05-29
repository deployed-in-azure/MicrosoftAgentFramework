namespace _02_ChatHistory
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new SimpleInMemoryChatHistoryExample().RunAsync();
            //await new BlobChatHistoryExample().RunAsync();
            //await new CosmosDbChatHistoryExample().RunAsync();
            //await new BlobSessionStoreExample().RunAsync();
        }
    }
}
