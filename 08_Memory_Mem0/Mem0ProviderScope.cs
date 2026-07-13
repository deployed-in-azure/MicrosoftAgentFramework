namespace _08_Memory_Mem0
{
    public record Mem0ProviderScope
    {
        public required string UserId { get; init; }
        public string? AppId { get; init; }
        public string? AgentId { get; init; }
        public string? RunId { get; init; }
    }
}
