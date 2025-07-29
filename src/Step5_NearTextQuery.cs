using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using WeaviateProject.Constants;

namespace WeaviateProject;

public static class Step5_NearTextQuery
{
    public static async Task Run(CollectionClient<Product> collection)
    {
        Console.WriteLine("\n--- Step 5: Performing a Near Text Query ---");
        Console.WriteLine($"Searching for products similar to: '{QueryConstants.NearTextQuery}'");

        var queryResult = await collection.Query.NearText(
            QueryConstants.NearTextQuery,
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