using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step4_FetchById
{
    public static async Task Run(CollectionClient<Product> collection, Guid productId)
    {
        Console.WriteLine($"Querying for product with ID: {productId}");

        var productObject = await collection.Query.FetchObjectByID(productId);

        if (productObject != null)
        {
            var product = productObject.As<Product>();
            Console.WriteLine($"Found Product: {product?.Name} (${product?.Price})");
        }
        else
        {
            Console.WriteLine("Product not found.");
        }
    }
}
