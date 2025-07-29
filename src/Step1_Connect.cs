using Weaviate.Client;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace WeaviateProject;

public static class Step1_Connect
{
    public static async Task<WeaviateClient?> Run()
    {
        // Connect to the local Weaviate instance and return the Client object
        // Connection details {httpHost: "localhost", httpPort: "8080", grpcPort: "50051"}
        // 
        // See Weaviate docs: 
        //      Connect to Weaviate: https://csharp-client--docs-weaviate-io.netlify.app/weaviate/connections/connect-local#no-authentication-enabled
        return null;
    }
}
