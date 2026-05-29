using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Agents.AI;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace _02_ChatHistory
{
    public class BlobSessionStore
    {
        private readonly BlobContainerClient _containerClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public BlobSessionStore(BlobContainerClient containerClient)
        {
            _containerClient = containerClient ?? throw new ArgumentNullException(nameof(containerClient));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()            
            };
        }

        public async Task<AgentSession> LoadSessionAsync(string tenantId, string userId, string conversationId, AIAgent agent)
        {
            var blobPath = BuildBlobPath(tenantId, userId, conversationId);
            var blobClient = _containerClient.GetBlobClient(blobPath);

            try
            {
                BlobDownloadResult downloadResult = await blobClient.DownloadContentAsync();
                var jsonText = downloadResult.Content.ToString();

                using var doc = JsonDocument.Parse(jsonText);

                return await agent.DeserializeSessionAsync(doc.RootElement, _jsonOptions);
            }
            catch (RequestFailedException ex) when (ex.Status == 404)
            {
                return await agent.CreateSessionWithUserContextAsync(tenantId, userId, conversationId);
            }
        }

        public async Task SaveSessionAsync(AgentSession session, AIAgent agent)
        {
            ArgumentNullException.ThrowIfNull(session);
            ArgumentNullException.ThrowIfNull(agent);

            var (tenantId, userId, conversationId) = session.GetUserContext();
            var blobPath = BuildBlobPath(tenantId!, userId!, conversationId!);
            var blobClient = _containerClient.GetBlobClient(blobPath);

            var serializedSession = await agent.SerializeSessionAsync(session, _jsonOptions);

            using var memoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(serializedSession.GetRawText()));
            await blobClient.UploadAsync(memoryStream, overwrite: true);
        }

        private static string BuildBlobPath(string tenantId, string userId, string conversationId)
        {
            return $"{tenantId}/users/{userId}/{conversationId}.json";
        }
    }
}
