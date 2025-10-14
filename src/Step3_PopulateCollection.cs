using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Weaviate.Client;

namespace WeaviateProject;
using WeaviateProject.Constants;

public static class Step3_PopulateCollection
{
    public static async Task<List<Guid>> Run(CollectionClient<Product> collection)
    {
        List<Product> productsToInsert = await ProductLoader.LoadProductsFromFile("./data/products.json");
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
}
