using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step4_FetchById
{
    public static async Task Run(CollectionClient<Product> collection, Guid productId)
    {
        // Get the object with the ID "productId"
        // Also print out the metadata, properties, and vector of the object
        //
        // See Weaviate docs: 
        //      Get object by ID: https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/search/basics
    }
}
