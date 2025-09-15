using System;
using System.Threading.Tasks;
using Weaviate.Client;

namespace WeaviateProject;

public static class Step8_DeleteCollection
{
    public static async Task Run(WeaviateClient client, string collectionName)
    {
        // Delete the "Product" collection and check if it exists after deletion
        //
        // See Weaviate docs:
        //      Delete collections: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/manage-collections/collection-operations#delete-a-collection
    }
}
