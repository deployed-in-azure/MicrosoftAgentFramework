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
