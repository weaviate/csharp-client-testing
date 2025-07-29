using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;
using WeaviateProject.Constants;

namespace WeaviateProject;

public static class Step5_NearTextQuery
{
    public static async Task Run(CollectionClient<Product> collection)
    {
        // Perform the nearText query specified in Constants.NearTextQuery
        //
        // See Weaviate docs: 
        //      Vector search: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/search/similarity#search-with-text
    }
}