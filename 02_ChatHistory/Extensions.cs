using Microsoft.Agents.AI;

namespace _02_ChatHistory
{
    public static class Extensions
    {
        private const string TenantIdKey = "App:UserContext:TenantId";
        private const string UserIdKey = "App:UserContext:UserId";
        private const string ConversationIdKey = "App:UserContext:ConversationId";

        public static async ValueTask<AgentSession> CreateSessionWithUserContextAsync(
            this AIAgent agent,
            string tenantId,
            string userId,
            string conversationId)
        {
            var session = await agent.CreateSessionAsync();

            session.StateBag.SetValue(TenantIdKey, tenantId);
            session.StateBag.SetValue(UserIdKey, userId);
            session.StateBag.SetValue(ConversationIdKey, conversationId);

            return session;
        }

        public static (string? TenantId, string? UserId, string? ConversationId) GetUserContext(this AgentSession session)
        {
            if (session == null)
            {
                return (null, null, null);
            }

            session.StateBag.TryGetValue<string>(TenantIdKey, out var tenantId);
            session.StateBag.TryGetValue<string>(UserIdKey, out var userId);
            session.StateBag.TryGetValue<string>(ConversationIdKey, out var conversationId);

            return (tenantId, userId, conversationId);
        }
    }
}
