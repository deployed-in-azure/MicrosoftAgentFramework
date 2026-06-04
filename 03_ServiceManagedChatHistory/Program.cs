namespace _03_ServiceManagedChatHistory
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            await new ConversationStoredInFoundryExample().RunAsync();
            //await new ResponsesApiWithStoredOutputExample().RunAsync();
            //await new ResponsesApiWithStoredOutputDisabledExample().RunAsync();

            Console.ReadKey();
        }
    }
}
