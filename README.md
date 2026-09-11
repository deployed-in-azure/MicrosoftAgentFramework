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

### 9. Mastering Microsoft Foundry Memory: Long-Term Context for AI Agents

Give your AI agents long-term context with Microsoft Foundry Memory Store. Learn how to use FoundryMemoryProvider in MAF with C#.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `FOUNDRY_PROJECT_NAME`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`
- `FOUNDRY_MEMORY_STORE_NAME`: Your Foundry Memory Store name e.g. `default-memory-store`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/microsoft-foundry-memory/)

### 10. AI Memory Scopes in Microsoft Foundry: 4 Architectural Patterns for .NET

Explore 4 core architectural patterns for AI memory scopes in Microsoft Foundry, Mem0, and the Microsoft Agent Framework. Learn how to isolate, share, and coordinate persistent state across users and agents in .NET.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-4o-mini`
- `FOUNDRY_PROJECT_NAME`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`

Ensure that the following memory stores exist in your Foundry project:
- `gym-store`: Used for gym/fitness agent memory.
- `diet-store`: Used for dietitian agent memory.
- `default-memory-store`: Used as the shared store across agents.

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/ai-memory-scopes-microsoft-foundry-dotnet/)

### 11. Modular Agent Skills in Microsoft Agent Framework | Progressive Disclosure Pattern

Master the Progressive Disclosure pattern in Microsoft Agent Framework to build modular, portable AI skills in C#. Includes full video guide.

To run the example, set the following environment variables:
- `AZURE_OPEN_AI_RESPONSES_CLIENT_URI`: Your Azure OpenAI endpoint URL (used for the Responses API chat client).
- `AZURE_OPEN_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

[Read the blog post to find more details](https://deployedinazure.com/modular-agent-skills-progressive-disclosure/)

### 12. Building & Deploying Hosted Agents in Microsoft Foundry using Microsoft Agent Framework

Master enterprise hosted agent architectures to deploy secure, isolated runtimes for your agentic applications in Microsoft Foundry. I'll show you how to move from basic prompt agents to fully managed containerized workloads using C# and Microsoft Agent Framework.

This example shows the code-first (`azure.ai.agent` / `remote_build`) hosting model, where `azd` builds and deploys your C# project directly as a Foundry-hosted agent exposing the Responses protocol.

To run the example, set the following environment variables:
- `FOUNDRY_PROJECT_ENDPOINT`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`
- `AZURE_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AGENT_NAME`: The name to register the hosted agent under in Microsoft Foundry.

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

Deploy the hosted agent with `azd deploy`, using the provided `azure.yaml`.

[Read the blog post to find more details](https://deployedinazure.com/intro-to-hosted-agents-in-microsoft-foundry/)

### 13. Building & Deploying Hosted Agents in Microsoft Foundry using Microsoft Agent Framework (Docker)

Master enterprise hosted agent architectures to deploy secure, isolated runtimes for your agentic applications in Microsoft Foundry. I'll show you how to move from basic prompt agents to fully managed containerized workloads using C# and Microsoft Agent Framework.

This example shows the container-first (`azure.ai.agent` / Docker) hosting model, packaging the agent in a `Dockerfile` so you have full control over the runtime image before `azd` deploys it as a Foundry-hosted agent exposing the Responses protocol.

To run the example, set the following environment variables:
- `FOUNDRY_PROJECT_ENDPOINT`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`
- `AZURE_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

Deploy the hosted agent with `azd deploy`, using the provided `azure.yaml`.

[Read the blog post to find more details](https://deployedinazure.com/intro-to-hosted-agents-in-microsoft-foundry/)

### 14. HostedAgent with Agent Identity and On-Behalf-Of Flow (Unsuccessful Attempt)

This example demonstrates an **unsuccessful attempt** to implement the On-Behalf-Of (OBO) flow using Microsoft.Identity.Web (in-process) authentication model with Hosted Agents in Microsoft Foundry.

The implementation tried to leverage Microsoft.Identity.Web's token acquisition capabilities to perform OBO token exchange for calling Microsoft Graph on behalf of the authenticated user. 
However, this approach encountered a critical limitation: **Microsoft Foundry removes all custom HTTP headers** passed through the platform, including the custom `BlueprintAuthorization` header that was being used to transmit the bearer token which audience denotes the agent blueprint client id.

The challenge required passing 2 headers:
1. **Blueprint Authorization header**: For passing the user's token to the agent with "aud=BlueprintClientId" (removed by Foundry)
2. **Standard Authorization header**: For authenticating with the Foundry data-plane gateway at `ai.azure.com`

Since Foundry strips custom headers at the gateway level, the OBO flow could not be properly implemented in this hosted environment. This example serves as a cautionary tale and reference point for understanding Foundry's header handling behavior.

**Note**: See Project 15 for a working self-hosted implementation of the OBO flow.

To run this example (it works locally), set the following environment variables:
- `FOUNDRY_PROJECT_ENDPOINT`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`
- `AZURE_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AzureAd__Instance`: Your Azure AD instance e.g. `https://login.microsoftonline.com/`
- `AzureAd__TenantId`: Your Azure AD tenant ID
- `AzureAd__ClientId`: Your Agent Blueprint client ID
- `AzureAd__ClientCredentials__0__SourceType`: The credential source type for the agent identity (it could be client secret when working locally)
- `AzureAd__ClientCredentials__0__ClientSecret`: Your application's client secret (only needed when working locally)
- `DownstreamApis__GraphApi__BaseUrl`: Microsoft Graph API base URL (`https://graph.microsoft.com/v1.0/`)
- `DownstreamApis__GraphApi__Scopes__0`: Scopes for Graph API access (`https://graph.microsoft.com/.default`)
- `AgentIdentityId`: The Agent Identity ID for your agent (child of the Blueprint client ID)
- `GraphCallMethod`: The method to call Graph API (e.g., `GraphServiceClient`, `DownstreamApi`, `HttpClient`, `ManualHttpClient`)
- `AZURE_TOKEN_CREDENTIALS`: "dev" (needed just when running locally)

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource

### 15. Agent On Behalf Of Flow in Entra Agent ID with Microsoft.Identity.Web & Microsoft Agent Framework

This example demonstrates a **working implementation** of the On-Behalf-Of (OBO) flow using Microsoft.Identity.Web (in-process) authentication model with self-hosted agents via the Microsoft Agent Framework.

Unlike the hosted Foundry scenario, self-hosting allows full control over HTTP headers and authentication flows. This implementation successfully performs OBO token exchange to call Microsoft Graph on behalf of the authenticated user, demonstrating clean patterns for:
- Acquiring user tokens from the incoming request context
- Performing OBO token exchange using Microsoft.Identity.Web
- Calling downstream APIs (Microsoft Graph) with the exchanged token
- Multiple token acquisition patterns: `GraphServiceClient`, `DownstreamApi`, `HttpClient`, and manual HTTP client approaches

This is the recommended approach for scenarios where you need reliable OBO token flow without platform-level header stripping constraints.

**Prerequisites:**

To enable the OBO flow to work correctly, you need to configure your Entra ID applications as follows:

- **Expose a scope in your Agent Blueprint app registration**: Define a scope (e.g., `access_as_user`) that represents permission to call the agent on behalf of users.
- **Add API Permissions to your client app registration**: Add the exposed scope from the Agent Blueprint as an API permission to the app registration that will obtain the initial user token.
- **Token audience requirement**: The token obtained from your client app must contain the `aud` (audience) claim that points to your `AzureAd__ClientId` (the Agent Blueprint client ID). This ensures the token is intended for your agent.
- **Requested scope**: When acquiring the initial token in your client app, request the scope you exposed in the Agent Blueprint (e.g., `access_as_user`). This scope will then be used in the OBO token exchange to obtain a token for calling Microsoft Graph.

**Obtaining the Initial Token (Example with Implicit Flow):**

If you have the implicit flow enabled for your client app registration, you can obtain an access token by directing a user to the following authorization URL:

```
https://login.microsoftonline.com/<tenant_id>/oauth2/v2.0/authorize?client_id=<client_app_reg_client_id>&response_type=token&redirect_uri=https://jwt.ms&scope=api://<blueprint_client_id>/access_as_user
```

Replace:
- `<tenant_id>`: Your Azure AD tenant ID
- `<client_app_reg_client_id>`: The app registration ID that has permission to the Blueprint's exposed scope
- `<blueprint_client_id>`: Your Agent Blueprint client ID (same as `AzureAd__ClientId`)

The returned token will have the correct `aud` claim pointing to your Blueprint client ID and can be used to invoke the agent with the OBO flow (Authorization Header: Bearer <token>).

To run the example, set the following environment variables:
- `FOUNDRY_PROJECT_ENDPOINT`: Your Microsoft Foundry project endpoint URL e.g. `https://<resource>.services.ai.azure.com/api/projects/<project-name>`
- `AZURE_AI_MODEL_DEPLOYMENT_NAME`: Your LLM deployment name e.g. `gpt-5.4-mini`
- `AzureAd__Instance`: Your Azure AD instance e.g. `https://login.microsoftonline.com/`
- `AzureAd__TenantId`: Your Azure AD tenant ID
- `AzureAd__ClientId`: Your Agent Blueprint client ID
- `AzureAd__ClientCredentials__0__SourceType`: The credential source type for the agent identity (e.g. ClientSecret when running locally or SignedAssertionFromManagedIdentity when hosted in Azure)
- `AzureAd__ClientCredentials__0__ClientSecret`: Your application's client secret (only needed when working locally)
- `DownstreamApis__GraphApi__BaseUrl`: Microsoft Graph API base URL (`https://graph.microsoft.com/v1.0/`)
- `DownstreamApis__GraphApi__Scopes__0`: Scopes for Graph API access (`https://graph.microsoft.com/.default`)
- `AgentIdentityId`: The Agent Identity ID for your agent (child of the Blueprint client ID)
- `GraphCallMethod`: The method to call Graph API (e.g., `GraphServiceClient`, `DownstreamApi`, `HttpClient`, `ManualHttpClient`)
- `AZURE_TOKEN_CREDENTIALS`: "dev" (needed just when running locally)

Ensure that your identity has:
- the `Foundry User` RBAC role assigned to access the Microsoft Foundry resource
