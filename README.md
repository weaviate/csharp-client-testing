# Weaviate C# Client - User testing

A project for testing basic Weaviate C# client operations:
- Connect to Weaviate
- Populate the database and vectorize data
- Perform simple retrieval, aggregations and vector searches
- Delete the data

## Quick Start

### Prerequisites

- VS Code with Dev Containers extension
- Docker Desktop

### Run the project:**

1. Clone this repository
2. Open VS Code in the project directory
3. Press Ctrl+Shift+P -> "Dev Containers: Reopen in Container"
4. Wait for container to build and Weaviate to start

Once inside the container in VS Code, open a `bash` terminal and run:

```
dotnet run --project WeaviateProject.csproj
```
