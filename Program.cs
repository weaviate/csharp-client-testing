using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace Example;

class Program
{
    private record ProductDataWithVectors(float[] Vector, Product Data);

    static async Task Main()
    {
        Console.WriteLine("=== Weaviate C# Client Demo ===\n");

        // Connect to Weaviate
        var weaviate = await ConnectToWeaviate();

        // Clean up existing collections
        await DeleteExistingCollections(weaviate);

        // Create collections
        var (productHandle, categoryHandle) = await CreateCollections(weaviate);

        // Insert data
        var categoryIds = await InsertCategories(categoryHandle);
        var products = await GetProductsAsync("products.json");
        var insertedProductIds = await InsertProducts(productHandle, products, categoryIds);

        // Demonstrate various query methods
        Console.WriteLine("\n--- Query Operations ---");

        // Run all query demonstrations
        await QueryAffordableProducts(productHandle);
        await FetchProductById(productHandle, insertedProductIds.First());
        await SearchProductsByBM25(productHandle);
        await SearchSimilarProducts(productHandle);
        await IterateAllProducts(productHandle);
        await ExportCollectionConfig(weaviate);
        await CalculateStatistics(productHandle);

        Console.WriteLine("\n=== Demo completed successfully! ===");
    }

    private static async Task<WeaviateClient> ConnectToWeaviate()
    {
        var wcdHost =Environment.GetEnvironmentVariable("WEAVIATE_HOSTNAME");
        var wcdApiKey = Environment.GetEnvironmentVariable("WEAVIATE_API_KEY");

        WeaviateClient weaviate;
        if (string.IsNullOrEmpty(wcdHost) || string.IsNullOrEmpty(wcdApiKey))
        {
            Console.WriteLine("Connecting to local Weaviate instance at http://localhost:8080");
            weaviate = Weaviate.Client.Connect.Local();
            Console.WriteLine("Connected to local Weaviate");
        }
        else
        {
            Console.WriteLine($"Connecting to Weaviate Cloud at {wcdHost}");
            weaviate = Weaviate.Client.Connect.Cloud(wcdHost, wcdApiKey, addEmbeddingHeader:true);
            Console.WriteLine($"Connected to Weaviate Cloud ({wcdHost})");
        }
        return weaviate;
    }

    private static async Task DeleteExistingCollections(WeaviateClient weaviate)
    {
        Console.WriteLine("\n--- Cleaning Up Existing Collections ---");
        try
        {
            if (await weaviate.Collections.Exists("Product"))
            {
                await weaviate.Collections.Delete("Product");
                Console.WriteLine("Deleted existing 'Product' collection");
            }
            if (await weaviate.Collections.Exists("Category"))
            {
                await weaviate.Collections.Delete("Category");
                Console.WriteLine("Deleted existing 'Category' collection");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during cleanup: {e.Message}");
        }
    }

    private static async Task<(CollectionClient<Product>, CollectionClient<Category>)> CreateCollections(WeaviateClient weaviate)
    {
        Console.WriteLine("\n--- Creating Collections ---");

        // Create Category collection first (referenced by Product)
        var categoryCollection = new Collection()
        {
            Name = "Category",
            Description = "Product categories",
            Properties = Property.FromCollection<Category>(),
            VectorConfig = new VectorConfig("category_vector", new Vectorizer.Text2VecWeaviate()),
            InvertedIndexConfig = new InvertedIndexConfig
            {
                IndexTimestamps = true,
                IndexNullState = true,
                IndexPropertyLength = true
            }
        };

        var categoryHandle = await weaviate.Collections.Create<Category>(categoryCollection);
        Console.WriteLine("Created 'Category' collection");

        // Create Product collection
        var productCollection = new Collection()
        {
            Name = "Product",
            Description = "Product catalog with various tech items",
            Properties = Property.FromCollection<Product>(),
            VectorConfig = new VectorConfig("product_vector", new Vectorizer.None()),
            InvertedIndexConfig = new InvertedIndexConfig
            {
                IndexTimestamps = true,
                IndexPropertyLength = true,
                CleanupIntervalSeconds = 30,
            },
        };

        var productHandle = await weaviate.Collections.Create<Product>(productCollection);
        Console.WriteLine("Created 'Product' collection");

        // List all collections
        // Console.WriteLine("\nListing all collections:");
        // await foreach (var collection in weaviate.Collections.List())
        // {
        //     Console.WriteLine($"  - {collection.Name}: {collection.Description}");
        // }

        return (productHandle, categoryHandle);
    }

    private static async Task<Dictionary<string, Guid>> InsertCategories(CollectionClient<Category> categoryHandle)
    {
        Console.WriteLine("\n--- Inserting Categories ---");
        var categories = new List<(Category category, float[] vector)>
        {
            (new Category { Name = "Electronics", Description = "Electronic devices and gadgets", IsActive = true },
             [0.1f, 0.2f, 0.3f, 0.4f, 0.5f]),
            (new Category { Name = "Audio", Description = "Audio equipment and accessories", IsActive = true },
             [0.2f, 0.3f, 0.4f, 0.5f, 0.6f]),
            (new Category { Name = "Gaming", Description = "Gaming peripherals and accessories", IsActive = true },
             [0.3f, 0.4f, 0.5f, 0.6f, 0.7f]),
            (new Category { Name = "Discontinued", Description = "Discontinued products", IsActive = false },
             [0.4f, 0.5f, 0.6f, 0.7f, 0.8f])
        };

        var categoryIds = new Dictionary<string, Guid>();
        foreach (var (category, vector) in categories)
        {
            var id = await categoryHandle.Data.Insert(
                category
                //vectors: new NamedVectors { { "category_vector", vector } }
            );
            categoryIds[category.Name!] = id;
            Console.WriteLine($"Inserted category: {category.Name} (ID: {id})");
        }

        return categoryIds;
    }

    private static async Task<List<Guid>> InsertProducts(
        CollectionClient<Product> productHandle,
        List<ProductDataWithVectors> products,
        Dictionary<string, Guid> categoryIds)
    {
        Console.WriteLine("\n--- Batch Inserting Products ---");
        Console.WriteLine($"Products to store: {products.Count}");

        var batchResults = await productHandle.Data.InsertMany(add =>
        {
            products.ForEach(p =>
            {
                add(
                    p.Data,
                    vectors: new NamedVectors { { "product_vector", p.Vector } }
                );
            });
        });

        var insertedIds = batchResults
            .Where(r => r.Error == null && r.ID.HasValue)
            .Select(r => r.ID!.Value)
            .ToList();
        Console.WriteLine(batchResults.ToList().ElementAt(0));
        Console.WriteLine($"Batch insert completed. Inserted {insertedIds.Count} products successfully.");
        return insertedIds;
    }

    private static async Task QueryAffordableProducts(CollectionClient<Product> productHandle)
    {
        Console.WriteLine("\n1. Finding available products under $200:");
        var affordableProducts = await productHandle.Query.List(
            filter: Filter.Property("isAvailable").Equal(true) & Filter.Property("price").LessThan(200),
            limit: 10
        );
        Console.WriteLine($"  Found {affordableProducts.ToList().Count} affordable products:");
        foreach (var obj in affordableProducts.Objects)
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  - {product?.Name}: ${product?.Price:F2}");
        }
    }

    private static async Task FetchProductById(CollectionClient<Product> productHandle, Guid productId)
    {
        Console.WriteLine("\n2. Fetching specific product by ID:");
        var specificProduct = await productHandle.Query.FetchObjectByID(
            productId,
            metadata: MetadataOptions.Full | MetadataOptions.Vector
        );

        if (specificProduct != null)
        {
            var prod = specificProduct.As<Product>();
            Console.WriteLine($"  Product: {prod?.Name}");
            Console.WriteLine($"  Created: {specificProduct.Metadata.CreationTime}");
            Console.WriteLine($"  Vector name: {specificProduct.Vectors.Keys.FirstOrDefault()}");
        }
    }

    private static async Task SearchProductsByBM25(CollectionClient<Product> productHandle)
    {
        Console.WriteLine("\n3. BM25 search for 'gaming':");
        var bm25Results = await productHandle.Query.BM25(
            "gaming laptop",
            fields: ["name", "description"]
            //limit: 3
        );

        foreach (var obj in bm25Results.Objects)
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  - {product?.Name} (Category: {product?.Category})");
        }
    }

    private static async Task SearchSimilarProducts(CollectionClient<Product> productHandle)
    {
        Console.WriteLine("\n4. Finding similar products to vector:");
        var nearVectorResults = await productHandle.Query.NearVector(
            vector: [0.5f, 0.6f, 0.7f, 0.8f, 0.9f],
            distance: 0.5f,
            limit: 3,
            metadata: MetadataOptions.Distance
        );

        foreach (var obj in nearVectorResults.Objects)
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  - {product?.Name} (Distance: {obj.Metadata.Distance:F4})");
        }
    }

    private static async Task IterateAllProducts(CollectionClient<Product> productHandle)
    {
        Console.WriteLine("\n5. Iterating through all products:");
        var count = 0;
        await foreach (var obj in productHandle.Iterator(cacheSize: 2))
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  [{++count}] {product?.Name} - ${product?.Price:F2}");
        }
    }

    private static async Task ExportCollectionConfig(WeaviateClient weaviate)
    {
        Console.WriteLine("\n6. Exporting Product collection configuration:");
        var exportedConfig = await weaviate.Collections.Export("Product");
        if (exportedConfig != null)
        {
            Console.WriteLine($"  Collection: {exportedConfig.Name}");
            Console.WriteLine($"  Properties: {exportedConfig.Properties.Count}");
            Console.WriteLine($"  Vector Config: {exportedConfig.VectorConfig.First().Key}");
            Console.WriteLine($"  BM25 B parameter: {exportedConfig.InvertedIndexConfig?.Bm25?.B}");
        }
    }

    private static async Task CalculateStatistics(CollectionClient<Product> productHandle)
    {
        Console.WriteLine("\n--- Final Statistics ---");
        var allProducts = await productHandle.Query.List(limit: 100);
        var remainingProducts = allProducts.Objects.ToList();
        Console.WriteLine($"Total products remaining: {remainingProducts.Count}");

        if (remainingProducts.Any())
        {
            var totalValue = remainingProducts.Sum(p => p.As<Product>()?.Price ?? 0);
            var avgPrice = remainingProducts.Average(p => p.As<Product>()?.Price ?? 0);
            var maxPrice = remainingProducts.Max(p => p.As<Product>()?.Price ?? 0);
            var minPrice = remainingProducts.Min(p => p.As<Product>()?.Price ?? 0);

            Console.WriteLine($"Total inventory value: ${totalValue:F2}");
            Console.WriteLine($"Average price: ${avgPrice:F2}");
            Console.WriteLine($"Price range: ${minPrice:F2} - ${maxPrice:F2}");
        }
    }

    private static async Task<List<ProductDataWithVectors>> GetProductsAsync(string filename)
    {
        try
        {
            using FileStream fs = new FileStream(
                filename,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true
            );

            var data = await JsonSerializer.DeserializeAsync<List<ProductDataWithVectors>>(fs) ?? [];
            return data;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading file: {ex.Message}.");
            return new List<ProductDataWithVectors>();
        }
    }
}
