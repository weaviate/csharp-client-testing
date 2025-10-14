using System;
using System.Threading.Tasks;
using Weaviate.Client;

namespace WeaviateProject;

public static class Step8_DeleteCollection
{
    public static async Task Run(WeaviateClient client, string collectionName)
    {
        await client.Collections.Delete(collectionName);

        Console.WriteLine($"Successfully deleted collection: '{collectionName}'");
    }
}
