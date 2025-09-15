using System;
using System.Linq;
using System.Threading.Tasks;
using Weaviate.Client;

namespace WeaviateProject;

class Program
{
    // Boolean variables to control execution flow
    private static bool step1_connect = true;
    private static bool step2_createCollection = true;
    private static bool step3_populateCollection = true;
    private static bool step4_fetchById = true;
    private static bool step5_nearTextQuery = true;
    private static bool step6_aggregationQuery = true;
    private static bool step7_deleteObjects = true;
    private static bool step8_deleteCollection = true;

    static async Task Main()
    {
        try
        {
            await Execute();
        }
        catch (Exception e)
        {
            Console.WriteLine($"\nAn error occurred during the demo: {e.Message}");
            Console.WriteLine(e.StackTrace);
        }
        Console.WriteLine("\nDemo finished.");
    }

    private static async Task Execute()
    {
        Console.WriteLine("=== Weaviate C# Client - User testing ===\n");

        WeaviateClient? client = null;
        try
        {
            // STEP 1: Connect to Weaviate
            if (!step1_connect) return;
            Console.WriteLine("===============================");
            Console.WriteLine("===== Step 1: Connect to Weaviate =====");
            Console.WriteLine("===============================");
            client = await Step1_Connect.Run();
            if (client is null) return;

            // The name of our collection
            string collectionName = Constants.CollectionConstants.CollectionName;

            // STEP 2: Create Collection
            if (!step2_createCollection) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 2: Create Collection =====");
            Console.WriteLine("===============================");
            var productCollection = await Step2_CreateCollection.Run(client, collectionName);
            Console.WriteLine("Collection created successfully.");

            // STEP 3: Populate Collection
            if (!step3_populateCollection) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 3: Populate Collection =====");
            Console.WriteLine("===============================");
            var insertedIds = await Step3_PopulateCollection.Run(productCollection);

            if (!insertedIds.Any())
            {
                Console.WriteLine("No data was created, skipping query steps.");
                return;
            }
            Console.WriteLine("Objects created successfully.");

            // Wait for import to complete before querying
            await Task.Delay(2000);

            // STEP 4: Fetch by ID
            if (!step4_fetchById) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 4: Fetch Object by ID =====");
            Console.WriteLine("===============================");
            await Step4_FetchById.Run(productCollection, insertedIds.First());

            // STEP 5: Near Text Query
            if (!step5_nearTextQuery) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 5: NearText Vector Search =====");
            Console.WriteLine("===============================");
            await Step5_NearTextQuery.Run(productCollection);

            // STEP 6: Aggregation Query
            if (!step6_aggregationQuery) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 6: Aggregate Query =====");
            Console.WriteLine("===============================");
            await Step6_AggregationQuery.Run(productCollection);

            // STEP 7: Delete Objects
            if (!step7_deleteObjects) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 7: Delete Objects =====");
            Console.WriteLine("===============================");
            await Step7_DeleteObjects.Run(productCollection, insertedIds.First());

            // STEP 8: Delete Collection
            if (!step8_deleteCollection) return;
            Console.WriteLine("\n===============================");
            Console.WriteLine("===== Step 8: Delete Collection =====");
            Console.WriteLine("===============================");
            await Step8_DeleteCollection.Run(client, collectionName);

            Console.WriteLine("\n=== Demo completed successfully! ===");
        }
        finally
        {
            if (client != null)
            {
                try
                {
                    client.Dispose();
                    Console.WriteLine("\nWeaviate client disposed.");
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error disposing Weaviate client: {e.Message}");
                }
            }
        }
    }
}
