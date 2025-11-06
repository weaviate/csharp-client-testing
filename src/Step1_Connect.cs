using Weaviate.Client;
using System.Text.Json;
using System;
using System.Threading.Tasks;

namespace WeaviateProject;

public static class Step1_Connect
{
    public static async Task<WeaviateClient?> Run()
    {
        // Get environment variables
        var restEndpoint = Environment.GetEnvironmentVariable("WEAVIATE_URL");
        var apiKey = Environment.GetEnvironmentVariable("WEAVIATE_API_KEY");

        var client = Connect.Cloud(restEndpoint: restEndpoint, apiKey: apiKey);

        try
        {
            var isReady = await client.IsReady();
            Console.WriteLine("Is Weaviate ready: " + isReady);
            return client;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to Weaviate: {ex.Message}");
            return null;
        }
    }
}