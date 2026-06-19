namespace _05_RAG_Neo4j
{
    public record KnowledgeGraphRelationship
    {
        public required string Source { get; init; }
        public required string Target { get; init; }
        public required string Label { get; init; }
        public required string Description { get; init; }
        public float Weight { get; init; } = 0.5f;
    }
}
