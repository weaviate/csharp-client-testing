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
        var insertedIds = new List<Guid>();

        // Populate the "Product" collection with data from a JSON file (productsToInsert)
        //
        // Return the created object IDs in the "insertedIds" list
        //
        // See Weaviate docs: 
        //      Create a new object: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/manage-objects/create#create-an-object

        // This is where the object creation code goes
        // insertedIds.Add(...) // Add the objects to the appropriate list once created

        return null;
    }
}
