using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client.Models;

namespace Example;

class Program
{
    private record ProductDataWithVectors(float[] Vector, Product Data);

    static async Task<List<ProductDataWithVectors>> GetProductsAsync(string filename)
    {
        try
        {
            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return []; // Return an empty list if the file doesn't exist
            }

            using FileStream fs = new FileStream(
                filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            // Deserialize directly from the stream for better performance, especially with large files
            var data = await JsonSerializer.DeserializeAsync<List<ProductDataWithVectors>>(fs) ?? [];

            return data;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"Error deserializing JSON: {ex.Message}");
            return []; // Return an empty list on deserialization error
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            return []; // Return an empty list on any other error
        }
    }

    static async Task Main()
    {
        // Read products from JSON file and unmarshal into Product class
        var products = await GetProductsAsync("products.json");

        // Use the C# client to store all products with a product class
        Console.WriteLine("Products to store: " + products.Count);

        // Connect to Weaviate Cloud
        var WCD_HOST = Environment.GetEnvironmentVariable("WCD_HOST")
            ?? throw new InvalidOperationException("WCD_HOST environment variable is not set");
        var WCD_API_KEY = Environment.GetEnvironmentVariable("WCD_API_KEY")
            ?? throw new InvalidOperationException("WCD_API_KEY environment variable is not set");

        var weaviate = Weaviate.Client.Connect.Cloud(WCD_HOST, WCD_API_KEY);

        var collection = weaviate.Collections.Use<Product>("Product");

        // Delete any existing "Product" class
        try
        {
            await collection.Delete();
            Console.WriteLine("Deleted existing 'Product' collection");
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error deleting collections: {e.Message}");
        }

        var productCollection = new Collection()
        {
            Name = "Product",
            Description = "Product catalog with various tech items",
            Properties = Property.FromCollection<Product>(),
            VectorConfig = new VectorConfig("default", new Vectorizer.Text2VecWeaviate()),
        };

        collection = await weaviate.Collections.Create<Product>(productCollection);

        await foreach (var c in weaviate.Collections.List())
        {
            Console.WriteLine($"Collection: {c.Name}");
        }

        // Batch Insertion Demo
        var batchInsertions = await collection.Data.InsertMany(add =>
        {
            products.ForEach(p => add(p.Data, vectors: new() { { "default", p.Vector } }));
        });

        // Get all objects and sum up the price property
        var result = await collection.Query.List(limit: 250);
        var retrieved = result.Objects.ToList();
        Console.WriteLine("Products retrieved: " + retrieved.Count());
        var totalPrice = retrieved.Sum(p => p.As<Product>()?.Price ?? 0);
        Console.WriteLine($"Total price of all products: ${totalPrice:F2}");

        // Delete object
        var firstObj = retrieved.First();
        if (firstObj.ID is Guid id)
        {
            await collection.Data.Delete(id);
            Console.WriteLine($"Deleted product: {firstObj.As<Product>()?.Name}");
        }

        result = await collection.Query.List(limit: 5);
        retrieved = result.Objects.ToList();
        Console.WriteLine("Products retrieved after deletion: " + retrieved.Count());

        firstObj = retrieved.First();
        if (firstObj.ID is Guid id2)
        {
            var fetched = await collection.Query.FetchObjectByID(id: id2);
            Console.WriteLine(
                "Product retrieved via gRPC matches: " + ((fetched?.ID ?? Guid.Empty) == id2)
            );
            Console.WriteLine($"Product details: {fetched?.As<Product>()}");
        }

        // Fetch multiple products by IDs
        {
            var idList = retrieved
                .Where(p => p.ID.HasValue)
                .Take(3)
                .Select(p => p.ID!.Value)
                .ToHashSet();

            var fetched = await collection.Query.FetchObjectsByIDs(idList);
            Console.WriteLine($"Multiple products retrieved via gRPC:");
            foreach (var obj in fetched.Objects)
            {
                var product = obj.As<Product>();
                Console.WriteLine($"  - {product?.Name} (${product?.Price:F2})");
            }
        }

        // Query near vector - find similar products
        Console.WriteLine("\nQuerying similar products to vector [0.5f, 0.6f, 0.7f, 0.8f, 0.9f]:");
        var queryNearVector = await collection.Query.NearVector(
            vector: [0.5f, 0.6f, 0.7f, 0.8f, 0.9f],
            distance: 0.5f,
            limit: 5,
            fields: ["name", "description", "brand", "price"],
            metadata: MetadataOptions.Score | MetadataOptions.Distance
        );

        foreach (var productObj in queryNearVector.Objects)
        {
            var product = productObj.As<Product>();
            Console.WriteLine($"\nProduct: {product?.Name}");
            Console.WriteLine($"Brand: {product?.Brand}");
            Console.WriteLine($"Price: ${product?.Price:F2}");
            Console.WriteLine($"Metadata: ${productObj?.Metadata}");
        }

        // Find products by price range
        Console.WriteLine("\nFinding products under $100:");
        var affordableProducts = retrieved
            .Where(p => p.As<Product>()?.Price < 100)
            .Select(p => p.As<Product>());

        foreach (var product in affordableProducts)
        {
            Console.WriteLine($"  - {product?.Name}: ${product?.Price:F2}");
        }

        // Find premium products
        Console.WriteLine("\nFinding premium products (over $200):");
        var premiumProducts = retrieved
            .Where(p => p.As<Product>()?.Price > 200)
            .Select(p => p.As<Product>());

        foreach (var product in premiumProducts)
        {
            Console.WriteLine($"  - {product?.Name}: ${product?.Price:F2}");
        }
    }
}
