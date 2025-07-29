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
        // Populate the "Product" collection with data from a JSON file
        //
        // Return the created object IDs in the "insertedIds" list
        //
        // See Weaviate docs: 
        //      Create a new object: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/manage-objects/create#create-an-object

        var insertedIds = new List<Guid>();

        // This is where the object creation code goes
        // insertedIds.Add(...) // Add the objects to the appropriate list once created

        return insertedIds;
    }

    // Loads and deserializes a list of products from a specified JSON file.
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