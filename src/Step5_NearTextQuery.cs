using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step5_NearTextQuery
{
    public static async Task Run(CollectionClient<Product> collection)
    {
        Console.WriteLine("\n--- Step 5: Performing a Near Text Query ---");
        string concept = "modern tech gadgets";
        Console.WriteLine($"Searching for products similar to: '{concept}'");

        var queryResult = await collection.Query.NearText(
            concept,
            distance: 0.25f,
            limit: 3,
            metadata: MetadataOptions.Distance);

        Console.WriteLine("Search Results:");
        foreach (var obj in queryResult.Objects)
        {
            var product = obj.As<Product>();
            Console.WriteLine($"- {product?.Name} (Distance: {obj.Metadata.Distance:F4})");
        }
    }
}