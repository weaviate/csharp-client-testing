using System;
using System.Threading.Tasks;
using Weaviate.Client;
using Weaviate.Client.Models;

namespace WeaviateProject;

public static class Step7_DeleteObjects
{
    public static async Task Run(CollectionClient<Product> collection, Guid productId)
    {
        // Delete an object with UUID productId and check if it exists after deletion
        //
        // See Weaviate docs: 
        //      Delete objects: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/manage-objects/delete#delete-object-by-id
    }
}
