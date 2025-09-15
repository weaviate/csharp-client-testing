using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace WeaviateProject.Constants
{
    public static class CollectionConstants
    {
        public const string CollectionName = "Products";
        public const string VectorName = "productVector";
        public const string CollectionDescription = "A collection of product objects for demo purposes.";
    }

    public static class QueryConstants
    {
        public const string NearTextQuery = "modern tech gadgets";
    }

    public static class ProductLoader
    {
        /// <summary>
        /// Loads and deserializes a list of products from a specified JSON file.
        /// </summary>
        public static async Task<List<Product>?> LoadProductsFromFile(string filePath)
        {
            try
            {
                await using var fileStream = File.OpenRead(filePath);
                return await JsonSerializer.DeserializeAsync<List<Product>>(fileStream, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"ERROR: The file '{filePath}' was not found.");
                return null;
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"ERROR: The file '{filePath}' contains invalid JSON. Details: {ex.Message}");
                return null;
            }
        }
    }
}