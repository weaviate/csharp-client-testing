using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step6_AggregationQuery
{
    public static async Task Run(CollectionClient<Product> collection)
    {
        Console.WriteLine("\n--- Step 6: Running an Aggregation Query ---");

        var aggregationResult = await collection.Aggregate.OverAll(
            metrics:
            [
                Metrics.ForProperty("price").Number(count: true, minimum: true, maximum: true, mean: true)
            ]
        );

        var priceStats = aggregationResult.Properties["price"] as Aggregate.Number;

        if (priceStats != null)
        {
            Console.WriteLine("Product Price Statistics:");
            Console.WriteLine($"  Count: {priceStats.Count}");
            Console.WriteLine($"  Average Price: ${priceStats.Mean:F2}");
            Console.WriteLine($"  Maximum Price: ${priceStats.Maximum:F2}");
            Console.WriteLine($"  Minimum Price: ${priceStats.Minimum:F2}");
        }
    }
}
