namespace _05_RAG_Neo4j
{
    public record KnowledgeGraphEntity
    {
        public required string Name { get; init; }
        public required string Type { get; init; }
        public required string Description { get; init; }
        public float[] Embedding { get; init; } = [];
    }
}
