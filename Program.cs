using System;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

class Program
{
    static async Task Main()
    {
        Console.WriteLine("=== Weaviate C# Demo Start ===\n");

        // --- Step 1: Connect to Weaviate ---
        // Establishes a connection to your Weaviate instance.
        var client = await Step1_Connect.Run();
        if (client is null) return;

        // The name of our collection
        const string collectionName = "Products";

        // --- Step 2: Create Collection ---
        // Deletes any existing collection and creates a new one with a defined schema.
        var productCollection = await Step2_CreateCollection.Run(client, collectionName);

        // --- Step 3: Populate Collection ---
        // Inserts a batch of product data into the new collection.
        var insertedIds = await Step3_PopulateCollection.Run(productCollection);

        // --- Step 4: Fetch by ID ---
        // Retrieves a single product object using its unique ID.
        if (insertedIds.Any())
        {
            await Step4_FetchById.Run(productCollection, insertedIds.First());
        }

        // --- Step 5: Near Text Query ---
        // Performs a semantic search to find products related to a text concept.
        await Step5_NearTextQuery.Run(productCollection);

        // --- Step 6: Aggregation Query ---
        // Calculates aggregate statistics (e.g., average price) across the data.
        await Step6_AggregationQuery.Run(productCollection);

        // --- Step 7: Delete Objects ---
        // Removes specific objects from the collection based on a filter.
        await Step7_DeleteObjects.Run(productCollection);

        // --- Step 8: Delete Collection ---
        // Deletes the entire collection to clean up.
        await Step8_DeleteCollection.Run(client, collectionName);

        Console.WriteLine("\n=== Demo completed successfully! ===");
    }
}