using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;
using WeaviateProject.Constants;

public static class Step2_CreateCollection
{
    public static async Task<CollectionClient<Product>> Run(WeaviateClient client, string collectionName)
    {
        Console.WriteLine("\n--- Step 2: Creating Collection ---");

        // Clean up previous runs by deleting the collection if it exists
        if (await client.Collections.Exists(collectionName))
        {
            await client.Collections.Delete(collectionName);
            Console.WriteLine($"Deleted existing collection: '{collectionName}'");
        }

        // Define the collection schema
        var productCollection = new Collection
        {
            Name = collectionName,
            Description = CollectionConstants.CollectionDescription,
            Properties = [.. Property.FromCollection<Product>()],
            // Use Weaviate's built-in vectorizer
            // Requires a text-vectorization module (e.g., text2vec-transformers) in your Docker Compose
            VectorConfig = new VectorConfig(CollectionConstants.VectorName, new Vectorizer.Text2VecContextionary())
        };

        var collection = await client.Collections.Create<Product>(productCollection);
        Console.WriteLine($"Successfully created collection: '{collectionName}'");
        return collection;
    }
}