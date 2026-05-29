using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using System.Text.Json;

namespace _02_ChatHistory
{
    public class BlobChatHistoryProvider : ChatHistoryProvider
    {
        private readonly ProviderSessionState<State> _sessionState;
        private readonly BlobContainerClient _containerClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BlobChatHistoryProvider(
            BlobContainerClient containerClient,
            Func<AgentSession?, State> stateInitializer,
            string? stateKey = null)
        {
            _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));

            _sessionState = new ProviderSessionState<State>(
                stateInitializer ?? throw new ArgumentNullException(nameof(stateInitializer)),
                stateKey ?? GetType().Name);

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        protected override async ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(
            InvokingContext context,
            CancellationToken cancellationToken = default)
        {
            var state = _sessionState.GetOrInitializeState(context.Session);

            var blobName = BuildBlobPath(state);
            var blobClient = _containerClient.GetBlobClient(blobName);

            try
            {
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync(cancellationToken).ConfigureAwait(false);
                var history = JsonSerializer.Deserialize<List<ChatMessage>>(downloadResult.Content.ToString(), _jsonOptions);
                return history ?? [];
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return [];
            }
        }

        protected override async ValueTask StoreChatHistoryAsync(
            InvokedContext context,
            CancellationToken cancellationToken = default)
        {
            var state = _sessionState.GetOrInitializeState(context.Session);
            var blobName = BuildBlobPath(state);
            var blobClient = _containerClient.GetBlobClient(blobName);

            List<ChatMessage> updatedTimeline = new();

            try
            {
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync(cancellationToken);

                var existingMessages = JsonSerializer.Deserialize<List<ChatMessage>>(downloadResult.Content.ToString(), _jsonOptions);
                if (existingMessages != null)
                {
                    updatedTimeline.AddRange(existingMessages);
                }
            }
            catch (RequestFailedException ex) when (ex.Status == 404) { }

            var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
            updatedTimeline.AddRange(allNewMessages);

            if (updatedTimeline.Count == 0) return;

            var serializedPayload = JsonSerializer.Serialize(updatedTimeline, _jsonOptions);

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serializedPayload));
            await blobClient.UploadAsync(memoryStream, overwrite: true, cancellationToken);
        }

        private static string BuildBlobPath(State state)
        {
            return $"{state.TenantId}/users/{state.UserId}/{state.ConversationId}.json";
        }

        public sealed class State
        {
            public State(string conversationId, string? tenantId = null, string? userId = null)
            {
                if (string.IsNullOrWhiteSpace(conversationId))
                {
                    throw new ArgumentException("Conversation ID cannot be empty", nameof(conversationId));
                }
                    

                ConversationId = conversationId;
                TenantId = tenantId;
                UserId = userId;
            }

            public string ConversationId { get; }
            public string? TenantId { get; }
            public string? UserId { get; }
        }
    }
}
