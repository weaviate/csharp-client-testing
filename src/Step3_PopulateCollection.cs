using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step3_PopulateCollection
{
    public static async Task<List<Guid>> Run(CollectionClient<Product> collection)
    {
        Console.WriteLine("\n--- Step 3: Populating Collection from JSON file ---");

        List<Product> productsToInsert = await LoadProductsFromFile("./data/products.json");
        if (productsToInsert == null || productsToInsert.Count == 0)
        {
            Console.WriteLine("No products to insert. Exiting step.");
            return new List<Guid>();
        }
        var insertedIds = new List<Guid>();

        // Loop through the list and insert one product at a time.
        foreach (var product in productsToInsert)
        {
            var id = await collection.Data.Insert(product);
            insertedIds.Add(id);
        }

        Console.WriteLine($"Successfully inserted {insertedIds.Count} products.");
        return insertedIds;
    }

    /// <summary>
    /// Loads and deserializes a list of products from a specified JSON file.
    /// </summary>
    private static async Task<List<Product>?> LoadProductsFromFile(string filePath)
    {
        try
        {
            await using var fileStream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<List<Product>>(fileStream, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine($"ERROR: The file '{filePath}' was not found.");
            return null;
        }
        catch (JsonException ex)
        {
            Console.WriteLine($"ERROR: The file '{filePath}' contains invalid JSON. Details: {ex.Message}");
            return null;
        }
    }
}