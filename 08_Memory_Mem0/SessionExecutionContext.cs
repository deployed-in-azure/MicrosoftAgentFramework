namespace _08_Memory_Mem0
{
    public record SessionExecutionContext
    {
        public string? RunId { get; init; }
        public required string UserId { get; init; }
        public string? ApplicationId { get; init; }
        public string? AgentId { get; init; }
    }
}
