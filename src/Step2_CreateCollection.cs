using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;
using WeaviateProject.Constants;

public static class Step2_CreateCollection
{
    public static async Task<CollectionClient<Product>> Run(WeaviateClient client, string collectionName)
    {
        // Create collection with name Constants.CollectionName and description Constants.CollectionDescription
        // The collection should store objects defined in Product.cs 
        // The collection uses the text2vecContextionary vectorizer and has the vector Constants.VectorName
        //
        // See Weaviate docs: 
        //      Create a collection with properties: https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/manage-collections/collection-operations
        //      Define a vectorizer: https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/manage-collections/vector-config
        return null;
    }
}