# Minimal Weaviate C# Project

A simple console application demonstrating basic Weaviate operations.

## Quick Start

### Prerequisites

- VS Code with Dev Containers extension
- Docker Desktop

### Run the project:**

1. Clone/create the project files above
2. Open VS Code in the project directory
3. Press Ctrl+Shift+P -> "Dev Containers: Reopen in Container"
4. Wait for container to build and Weaviate to start

Once inside the container, run:

```
dotnet run --project WeaviateProject.csproj
```

### Covered

Connection:
- Connect to local instance
- Connect to Weaviate Cloud
- Get meta info

Collections:
- Create collection 
- Check collection exists
- Delete collection
- List all collections

Objects:
- Insert object with vectors
- Insert object without vectors
- Delete one object
- TODO - Delete many objects 

Queries:
- Fetch by ID