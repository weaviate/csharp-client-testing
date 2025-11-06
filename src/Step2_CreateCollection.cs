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
        // Clean up previous runs by deleting the collection if it exists
        if (await client.Collections.Exists(collectionName))
        {
            await client.Collections.Delete(collectionName);
            Console.WriteLine($"Deleted existing collection: '{collectionName}'");
        }

        // Define the collection schema
        var productCollection = new CollectionConfig
        {
            Name = collectionName,
            Description = CollectionConstants.CollectionDescription,
            Properties = [.. Property.FromClass<Product>()],
            // Use Weaviate Cloud's built-in vectorizer
            VectorConfig = new VectorConfig(CollectionConstants.VectorName, new Vectorizer.Text2VecWeaviate())
        };

        var collection = await client.Collections.Create<Product>(productCollection);
        Console.WriteLine($"Successfully created collection: '{collectionName}'");
        return collection;
    }
}