using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step7_DeleteObjects
{
    public static async Task Run(CollectionClient<Product> collection, Guid productId)
    {
        await collection.Data.Delete(productId);

        Console.WriteLine($"Successfully deleted objects.");

        // Verify deletion by counting remaining objects
        var countResult = await collection.Aggregate.OverAll(
            metrics: [Metrics.ForProperty("name").Text(count: true)]);
        var nameStats = countResult.Properties["name"] as Aggregate.Text;
        Console.WriteLine($"Remaining objects in collection: {nameStats?.Count ?? 0}");
    }
}
