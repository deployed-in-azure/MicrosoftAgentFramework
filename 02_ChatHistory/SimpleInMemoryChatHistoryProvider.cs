using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace _02_ChatHistory
{
    public class SimpleInMemoryChatHistoryProvider : ChatHistoryProvider
    {
        private readonly ProviderSessionState<State> _sessionState;

        public SimpleInMemoryChatHistoryProvider(
            Func<AgentSession?, State>? stateInitializer = null,
            string? stateKey = null)
            : base(
                provideOutputMessageFilter: null,
                storeInputRequestMessageFilter: null,
                storeInputResponseMessageFilter: null)
        {
            _sessionState = new ProviderSessionState<State>(
                stateInitializer ?? (_ => new State()),
                stateKey ?? GetType().Name);
        }

        public override IReadOnlyList<string> StateKeys => [_sessionState.StateKey];

        protected override ValueTask<IEnumerable<ChatMessage>> ProvideChatHistoryAsync(InvokingContext context, CancellationToken cancellationToken = default)
        {
            var messages = _sessionState.GetOrInitializeState(context.Session).Messages;
            return new(messages);
        }

        protected override ValueTask StoreChatHistoryAsync(InvokedContext context, CancellationToken cancellationToken = default)
        {
            var state = _sessionState.GetOrInitializeState(context.Session);

            var allNewMessages = context.RequestMessages.Concat(context.ResponseMessages ?? []);
            state.Messages.AddRange(allNewMessages);

            _sessionState.SaveState(context.Session, state);

            return default;
        }

        public class State
        {
            public List<ChatMessage> Messages { get; set; } = [];
        }
    }
}
