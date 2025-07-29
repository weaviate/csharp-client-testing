using Weaviate.Client;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace WeaviateProject;

public static class Step1_Connect
{
    public static async Task<WeaviateClient?> Run()
    {
        Console.WriteLine("--- Step 1: Connecting to Weaviate ---");

        var client = Connect.Local(restPort: 8080, grpcPort: 50051);

        try
        {
            // var meta = await client.GetMeta();
            // Console.WriteLine($"Connected to Weaviate v{meta.Version} on {meta.Hostname}");
            // Console.WriteLine($"Modules: {JsonSerializer.Serialize(meta.Modules)}");
            return client;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to Weaviate: {ex.Message}");
            return null;
        }
    }
}
