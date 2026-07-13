# Microsoft Agent Framework

Practical examples for the Microsoft Agent Framework in .NET. Learn how to build, trace, and scale composable AI agents integrated with various Azure services and native Azure hosting.

## Blog Posts

### 1. Microsoft Agent Framework Tutorial: Get Started with AI Agents in .NET

Learn how to use the new Microsoft Agent Framework in .NET. A complete, hands-on tutorial with 10 practical C# examples from Hello World to RAG and telemetry.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_CLIENT_URI`: Your Microsoft Foundry endpoint URL.
- `AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/microsoft-agent-framework-tutorial-csharp/)

### 2. Chat History in Microsoft Agent Framework: Moving to Azure Production Storage

Store Chat History in Microsoft Agent Framework using Cosmos DB or Blob Storage. Real C# examples showing how to persist state at scale.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_CLIENT_URI`: Your Microsoft Foundry endpoint URL.
- `AZURE_OPEN_AI_CHAT_CLIENT_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AZURE_COSMOS_ACCOUNT_URI`: Your Azure Cosmos DB account uri
- `AZURE_COSMOS_DB_NAME`: Azure Cosmos DB database name
- `AZURE_COSMOS_CONTAINER_NAME`: Azure Cosmos DB container name
- `AZURE_STORAGE_ACCOUNT_URI`: Your Azure Storage Account uri

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource
- the `Cosmos DB Built-in Data Contributor` RBAC role assigned to access the Azure Cosmos DB.
- the `Storage Blob Data Contributor` RBAC role assigned to access the Azure Storage Account blobs.

Ensure that the container you create in Azure Cosmos DB uses such a hierarchical partition key: `/tenantId, /userId, /conversationId`

[Read the blog post to find more details](https://deployedinazure.com/chat-history-in-microsoft-agent-framework/)

### 3. Chat History in Microsoft Agent Framework: Service-Managed Chat History

Learn Service-Managed chat history in Microsoft Agent Framework. Explore Project Conversations in Microsoft Foundry and OpenAI Responses API stateful patterns.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_CONVERSATION_CLIENT_URI`: Your Microsoft Foundry endpoint URL (used for Project Conversations example).
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for Responses API examples).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/service-managed-chat-history-patterns/)

### 4. RAG in Microsoft Agent Framework: The Ultimate Guide to AIContextProvider

Learn how to control RAG in Microsoft Agent Framework. Master AIContextProvider and TextSearchProvider for clean, production-ready .NET pipelines.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for Responses API examples).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/rag-in-microsoft-agent-framework/)

### 5. Injecting Neo4j Graph Context into Microsoft Agent Framework: Neo4jContextProvider vs Tool Call

Master Neo4jContextProvider vs tool calls in Microsoft Agent Framework to build flexible, production-ready .NET GraphRAG architectures.

[Reading this blog post is helpful to understand the topics discussed here](https://deployedinazure.com/graph-rag-csharp-neo4j-introduction/)

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AZURE_OPEN_AI_URI`: Your Azure OpenAI endpoint URL (used for the embedding client).
- `AZURE_OPEN_AI_EMBEDDING_MODEL`: Your embedding model deployment name e.g. `text-embedding-ada-002`
- `NEO4J_URI`: Your Neo4j database URI e.g. `neo4j://127.0.0.1:7687`
- `NEO4J_USERNAME`: Your Neo4j username
- `NEO4J_PASSWORD`: Your Neo4j password
- `NEO4J_INDEX_NAME`: Your Neo4j vector index name (required for the `Neo4jContextProvider` example)

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/neo4jcontextprovider-vs-tool-call/)

### 6. Foundry IQ MCP Server vs HTTP: Enterprise RAG for MAF

Connect Microsoft Agent Framework to enterprise data in minutes. Compare the Foundry IQ MCP server with raw HTTP for optimal .NET RAG apps.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL.
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AZURE_AI_SEARCH_URI`: Your Azure AI Search endpoint URL (used as the Foundry IQ endpoint).
- `AZURE_AI_SEARCH_KNOWLEDGE_BASE`: Your Foundry IQ knowledge base name.

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource
- the `Search Index Data Reader` RBAC role assigned to access the Azure AI Search

[Read the blog post to find more details](https://deployedinazure.com/foundry-iq-mcp-server-vs-http-rag-maf/)

### 7. Long-Term Agent Memory in Microsoft Agent Framework with ChatHistoryMemoryProvider

Design a robust memory architecture for autonomous AI agents that goes beyond simple, single session conversations.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AZURE_OPEN_AI_URI`: Your Azure OpenAI endpoint URL (used for the embedding client).
- `AZURE_OPEN_AI_EMBEDDING_MODEL`: Your embedding model deployment name e.g. `text-embedding-ada-002`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/agent-memory-chathistorymemoryprovider/)

### 8. Getting Started with Mem0 in .NET | Long-Term AI Memory in Microsoft Agent Framework

Learn to build long-term AI memory in .NET using Mem0 and Microsoft Agent Framework. Stop writing custom database plumbing from scratch.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `MEM0_API_KEY`: Your Mem0 API key.

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/getting-started-mem0-dot-net-agent-framework/)
