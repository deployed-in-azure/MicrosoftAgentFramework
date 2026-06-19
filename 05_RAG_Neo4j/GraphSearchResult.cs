namespace _05_RAG_Neo4j
{
    public record GraphSearchResult
    {
        public required IReadOnlyList<KnowledgeGraphEntity> SeedEntities { get; init; }
        public required IReadOnlyList<KnowledgeGraphEntity> TraversedEntities { get; init; }
        public required IReadOnlyList<KnowledgeGraphRelationship> Relationships { get; init; }
        public required int TraversalDepth { get; init; }

        public IReadOnlyList<KnowledgeGraphEntity> AllEntities =>
            SeedEntities.Concat(TraversedEntities).DistinctBy(e => e.Name).ToList();
    }
}
