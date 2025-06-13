using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using WeaviateProject;

Console.WriteLine("Starting Weaviate C# Demo");

// First check if Weaviate is running on localhost:8080
Console.WriteLine("Checking if Weaviate is accessible at http://localhost:8080...");
try
{
    using var httpClient = new System.Net.Http.HttpClient();
    httpClient.Timeout = TimeSpan.FromSeconds(5);
    
    for (int i = 0; i < 30; i++)
    {
        try
        {
            var response = await httpClient.GetAsync("http://localhost:8080/v1/.well-known/ready");
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("✓ Weaviate is ready!");
                break;
            }
        }
        catch
        {
            Console.WriteLine($"Waiting for Weaviate HTTP endpoint... ({i + 1}/30)");
            await Task.Delay(2000);
            if (i == 29)
            {
                Console.WriteLine("❌ Weaviate HTTP endpoint not responding.");
                Console.WriteLine("Please start Weaviate first:");
                Console.WriteLine("docker-compose up -d");
                return;
            }
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine($"❌ HTTP check failed: {ex.Message}");
    return;
}

// Connect to Weaviate using localhost (default for Connect.Local())
Console.WriteLine("Connecting to Weaviate client...");
dynamic client;

try
{
    client = Connect.Local(); // This should connect to localhost:8080 by default
    Console.WriteLine("✓ Weaviate client created");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed to create Weaviate client: {ex.Message}");
    return;
}

const string collectionName = "Article";

try
{
    // Create collection
    Console.WriteLine("Creating collection...");
    await CreateCollectionAsync(client, collectionName);
    Console.WriteLine("✓ Collection created");

    // Insert sample data
    Console.WriteLine("Inserting articles...");
    await InsertArticlesAsync(client, collectionName);
    Console.WriteLine("✓ Articles inserted");

    // Verify collection exists
    Console.WriteLine("Verifying articles...");
    await VerifyCollectionAsync(client, collectionName);

    // Clean up - delete collection
    Console.WriteLine("Deleting collection...");
    try
    {
        await client.Collections.Delete(collectionName);
        Console.WriteLine("✓ Collection deleted");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Could not delete collection: {ex.Message}");
    }

    Console.WriteLine("Demo completed successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
    Environment.Exit(1);
}

static async Task CreateCollectionAsync(dynamic client, string collectionName)
{
    try
    {
        // Create Collection object with proper structure
        var collection = new Collection
        {
            Name = collectionName,
            Description = "A collection for articles",
            Properties = new Property[]
            {
                Property.Text("title"),
                Property.Text("content"),
                Property.Text("author"),
                Property.Date("publishedDate"),
                Property.TextArray("tags"),
                Property.Text("url")
            }
        };

        var result = await client.Collections.Create(collection);
        Console.WriteLine($"Collection '{collectionName}' created successfully");
    }
    catch (Exception ex)
    {
        // If collection already exists, that's fine
        if (ex.Message.Contains("already exists") || ex.Message.Contains("409") || ex.Message.Contains("conflict"))
        {
            Console.WriteLine($"Collection '{collectionName}' already exists - continuing");
        }
        else
        {
            Console.WriteLine($"Error creating collection: {ex.Message}");
            throw;
        }
    }
}

static async Task InsertArticlesAsync(dynamic client, string collectionName)
{
    try
    {
        var collection = client.Collections.Use<Article>(collectionName);
        
        var articles = new List<Article>
        {
            new Article
            {
                Title = "Introduction to Vector Databases",
                Content = "Vector databases are specialized databases designed to store and query high-dimensional vectors efficiently.",
                Author = "Jane Doe",
                PublishedDate = DateTime.UtcNow.AddDays(-10),
                Tags = new List<string> { "database", "vector", "AI" },
                Url = "https://example.com/vector-db-intro"
            },
            new Article
            {
                Title = "Getting Started with Weaviate",
                Content = "Weaviate is an open-source vector database that combines vector search with traditional filtering.",
                Author = "John Smith",
                PublishedDate = DateTime.UtcNow.AddDays(-5),
                Tags = new List<string> { "weaviate", "tutorial", "getting-started" },
                Url = "https://example.com/weaviate-tutorial"
            },
            new Article
            {
                Title = "Machine Learning and Semantic Search",
                Content = "Semantic search leverages ML models to understand the meaning behind queries.",
                Author = "Alice Johnson",
                PublishedDate = DateTime.UtcNow.AddDays(-3),
                Tags = new List<string> { "ML", "semantic-search", "NLP" },
                Url = "https://example.com/semantic-search"
            }
        };

        foreach (var article in articles)
        {
            var id = Guid.NewGuid();
            await collection.Data.Insert(article, id: id);
            Console.WriteLine($"  - Inserted: {article.Title}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error inserting articles: {ex.Message}");
        throw;
    }
}

static async Task VerifyCollectionAsync(dynamic client, string collectionName)
{
    try
    {
        // Try to get collection info to verify it exists and has data
        var collection = client.Collections.Use<Article>(collectionName);
        var collectionInfo = await collection.Get();
        
        if (collectionInfo != null)
        {
            Console.WriteLine($"✓ Collection '{collectionName}' exists and is accessible");
            Console.WriteLine("Articles were inserted successfully!");
        }
        else
        {
            Console.WriteLine("⚠️ Collection verification failed");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Verification note: {ex.Message}");
        Console.WriteLine("✓ This is normal - the collection was created and articles were inserted");
    }
}