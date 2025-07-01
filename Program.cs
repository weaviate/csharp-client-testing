using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using Weaviate.Client.Models.Vectorizers;

namespace Example;

class Program
{
    private record ProductDataWithVectors(float[] Vector, Product Data);

    static async Task<List<ProductDataWithVectors>> GetProductsAsync(string filename)
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
            return null;
        }
    }

    static float[] GenerateRandomVector(int dimensions, Random random)
    {
        var vector = new float[dimensions];
        for (int i = 0; i < dimensions; i++)
        {
            vector[i] = (float)random.NextDouble();
        }
        return vector;
    }

    static async Task Main()
    {
        Console.WriteLine("=== Weaviate C# Client Demo ===\n");

        // Connect to Weaviate
        //var wcdHost = Environment.GetEnvironmentVariable("WCD_HOST");
        var wcdHost = "";
        var wcdApiKey = Environment.GetEnvironmentVariable("WCD_API_KEY");

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
            weaviate = Weaviate.Client.Connect.Cloud(wcdHost, wcdApiKey);
            Console.WriteLine($"Connected to Weaviate Cloud ({wcdHost})");
        }

        // Check if ready
        // var isReady = await weaviate.IsReady();
        // Console.WriteLine($"Weaviate is ready: {isReady}");

        // Get meta information
        // var meta = await weaviate.GetMeta();
        // Console.WriteLine($"Weaviate version: {meta.Version}");

        // Clean up existing collections
        try
        {
            if (await weaviate.Collections.Exists("Product"))
            {
                await weaviate.Collections.Use<Product>("Product").Delete();
                Console.WriteLine("Deleted existing 'Product' collection");
            }
            if (await weaviate.Collections.Exists("Category"))
            {
                await weaviate.Collections.Use<Category>("Category").Delete();
                Console.WriteLine("Deleted existing 'Category' collection");
            }
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error during cleanup: {e.Message}");
        }
        // Create collections
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
            },
            // ReplicationConfig = new ReplicationConfig
            // {
            //     Factor = 3,
            //     AsyncEnabled = false
            // }
        };

        var categoryClient = await weaviate.Collections.Create<Category>(categoryCollection);
        Console.WriteLine("Created 'Category' collection");

        // Create Product collection with reference to Category
        var productCollection = new Collection()
        {
            Name = "Product",
            Description = "Product catalog with various tech items",
            Properties = Property.FromCollection<Product>(),
            //References = [Property.Reference("category", "Category")],
            VectorConfig = new VectorConfig("product_vector", new Vectorizer.None()),
            InvertedIndexConfig = new InvertedIndexConfig
            {
                IndexTimestamps = true,
                IndexPropertyLength = true,
                CleanupIntervalSeconds = 30,
            },
            ShardingConfig = new ShardingConfig
            {
                VirtualPerPhysical = 128,
                DesiredCount = 1,
                DesiredVirtualCount = 128
            }
        };

        var productClient = await weaviate.Collections.Create<Product>(productCollection);
        Console.WriteLine("Created 'Product' collection with reference to Category");

        // List all collections
        Console.WriteLine("\nListing all collections:");
        await foreach (var collection in weaviate.Collections.List())
        {
            Console.WriteLine($"  - {collection.Name}: {collection.Description}");
        }

        // Insert categories
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
            var id = await categoryClient.Data.Insert(
                category, 
                vectors: new NamedVectors { { "category_vector", vector } }
            );
            categoryIds[category.Name!] = id;
            Console.WriteLine($"Inserted category: {category.Name} (ID: {id})");
        }

        // Read products from JSON file or use sample data
        var products = await GetProductsAsync("products.json");
        Console.WriteLine($"\nProducts to store: {products.Count}");

        // Batch insert products with references
        Console.WriteLine("\n--- Batch Inserting Products with References ---");
        var batchResults = await productClient.Data.InsertMany(add =>
        {
            products.ForEach(p =>
            {
                add(
                    p.Data,
                    vectors: new NamedVectors { { "product_vector", p.Vector } }
                    //references: [new ObjectReference("category", categoryIds[p.Data.Category])]
                );
            });
        });

        Console.WriteLine($"Batch insert completed. Inserted {batchResults.Count(r => r.Error == null)} products successfully.");

        // Demonstrate various query methods
        Console.WriteLine("\n--- Query Operations ---");

        // 1. List with filter
        Console.WriteLine("\n1. Finding available products under $200:");
        var affordableProducts = await productClient.Query.List(
            filter: Filter.Property("isAvailable").Equal(true) & Filter.Property("price").LessThan(200),
            limit: 10
        );
        foreach (var obj in affordableProducts.Objects)
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  - {product?.Name}: ${product?.Price:F2}");
        }

        // 2. Fetch by ID with metadata
        Console.WriteLine("\n2. Fetching specific product by ID:");
        var firstProductId = batchResults.First(r => r.Error == null).ID!.Value;
        var specificProduct = await productClient.Query.FetchObjectByID(
            firstProductId,
            metadata: MetadataOptions.Full | MetadataOptions.Vector
        );
        if (specificProduct != null)
        {
            var prod = specificProduct.As<Product>();
            Console.WriteLine($"  Product: {prod?.Name}");
            Console.WriteLine($"  Created: {specificProduct.Metadata.CreationTime}");
            // Console.WriteLine($"  Vector dimensions: {specificProduct.Vectors["default"].Length}");
        }

        // 3. BM25 search with references
        Console.WriteLine("\n3. BM25 search for 'gaming' with category reference:");
        var bm25Results = await productClient.Query.BM25(
            "gaming laptop",
            fields: ["name", "description"]
            //references: [new QueryReference("category", ["name", "description"])]
            //limit: 3
        );
        foreach (var obj in bm25Results.Objects)
        {
            var product = obj.As<Product>();
            var category = obj.References["category"]?.FirstOrDefault();
            Console.WriteLine($"  - {product?.Name} (Category: {category?.Properties["name"]})");
        }

        // 4. Near Vector search
        Console.WriteLine("\n4. Finding similar products to vector:");
        var nearVectorResults = await productClient.Query.NearVector(
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

        // 5. Complex filter with dates and arrays
        // Console.WriteLine("\n5. Products released in last 6 months with specific tags:");
        // var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
        // var recentProducts = await productClient.Query.List(
        //     filter: Filter.Property("releaseDate").GreaterThan(sixMonthsAgo)
        //         & Filter.Property("tags").ContainsAny(["electronics", "gaming"]),
        //     sort: [Sort.ByProperty("releaseDate")]
        // );
        // foreach (var obj in recentProducts.Objects)
        // {
        //     var product = obj.As<Product>();
        //     Console.WriteLine($"  - {product?.Name} (Released: {product?.ReleaseDate:yyyy-MM-dd})");
        // }

        // 6. GroupBy query
        // Console.WriteLine("\n6. Group products by brand:");
        // var groupedResults = await productClient.Query.List(
        //     groupBy: new GroupByRequest
        //     {
        //         PropertyName = "brand",
        //         NumberOfGroups = 5,
        //         ObjectsPerGroup = 2
        //     }
        // );
        // if (groupedResults.Groups != null)
        // {
        //     foreach (var group in groupedResults.Groups)
        //     {
        //         Console.WriteLine($"  Brand: {group.GroupValue} ({group.Count} products)");
        //         foreach (var obj in group.Objects)
        //         {
        //             var product = obj.As<Product>();
        //             Console.WriteLine($"    - {product?.Name}: ${product?.Price:F2}");
        //         }
        //     }
        // }

        // // 7. Filter by reference properties
        // Console.WriteLine("\n7. Products in active categories:");
        // var activeProducts = await productClient.Query.List(
        //     filter: Filter.Reference("category").Property("isActive").Equal(true),
        //     references: [new QueryReference("category", ["name"])],
        //     limit: 5
        // );
        // foreach (var obj in activeProducts.Objects)
        // {
        //     var product = obj.As<Product>();
        //     var categoryName = obj.References["category"]?.FirstOrDefault()?.Properties["name"];
        //     Console.WriteLine($"  - {product?.Name} (Category: {categoryName})");
        // }

        // 8. Batch reference operations
        // Console.WriteLine("\n8. Adding multiple category references to a product:");
        // var productForMultiRef = batchResults.Last(r => r.Error == null).ID!.Value;
        // var refAddResult = await productClient.Data.ReferenceAddMany(
        //     new DataReference(productForMultiRef, "category", categoryIds["Electronics"]),
        //     new DataReference(productForMultiRef, "category", categoryIds["Gaming"])
        // );
        // Console.WriteLine($"  Added {refAddResult.Count(r => r.Error == null)} references successfully");

        // // 9. Delete operations
        // Console.WriteLine("\n9. Deleting discontinued products:");
        // var deleteResult = await productClient.Data.DeleteMany(
        //     where: Filter.Property("isAvailable").Equal(false),
        //     verbose: true,
        //     dryRun: false
        // );
        // Console.WriteLine($"  Deleted {deleteResult.Successful} products");
        // if (deleteResult.Objects.Any())
        // {
        //     foreach (var delObj in deleteResult.Objects)
        //     {
        //         Console.WriteLine($"    - Deleted ID: {delObj.Uuid} (Success: {delObj.Successful})");
        //     }
        // }

        // 10. Iterator demonstration
        Console.WriteLine("\n10. Iterating through all remaining products:");
        var count = 0;
        await foreach (var obj in productClient.Iterator(cacheSize: 2))
        {
            var product = obj.As<Product>();
            Console.WriteLine($"  [{++count}] {product?.Name} - ${product?.Price:F2}");
        }

        // 11. Export collection configuration
        Console.WriteLine("\n11. Exporting Product collection configuration:");
        var exportedConfig = await weaviate.Collections.Export("Product");
        if (exportedConfig != null)
        {
            Console.WriteLine($"  Collection: {exportedConfig.Name}");
            Console.WriteLine($"  Properties: {exportedConfig.Properties.Count}");
            Console.WriteLine($"  Vector Config: {exportedConfig.VectorConfig.First().Key}");
            Console.WriteLine($"  BM25 B parameter: {exportedConfig.InvertedIndexConfig?.Bm25?.B}");
        }

        // Calculate statistics
        Console.WriteLine("\n--- Final Statistics ---");
        var allProducts = await productClient.Query.List(limit: 100);
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

        Console.WriteLine("\n=== Demo completed successfully! ===");
    }
}
