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
        // Perform the nearText query specified in QueryConstants.NearTextQuery
        // Limit the returned results to three objects and return their distance scores and vectors as well to print them out
        //
        // See Weaviate docs: 
        //      Vector search: https://client-libraries-beta--docs-weaviate-io.netlify.app/weaviate/search/similarity
    }
}