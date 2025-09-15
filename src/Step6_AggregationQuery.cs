using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step6_AggregationQuery
{
    public static async Task Run(CollectionClient<Product> collection)
    {
        // Perform the following aggregate query: 
        //      Group all products by availability name and for each group find the maximum and minimum price.
        //
        // See Weaviate docs:
        //      Integer properties: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/search/aggregate#aggregate-int-properties
        //      Group by: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/search/aggregate#aggregate-groupedby-properties
    }
}
