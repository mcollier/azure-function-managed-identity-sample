using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Azure.Storage.Queues.Models;

namespace Collier.Functions
{
    public static class SampleFunction
    {
        [FunctionName("QueueTrigger")]
        public static void RunQueueTrigger(
            [QueueTrigger("%QueueName%", Connection="MyStorageConnection" )] QueueMessage queueItem,
            ILogger log)
        {
            log.LogInformation($"C# function processed: {queueItem.MessageText}");
        }

        [FunctionName("EventHubTriggerCSharp1")]
        public static async Task Run([EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString")] EventData[] events, ILogger log)
        {
            // Reference https://github.com/Azure/azure-sdk-for-net/tree/Microsoft.Azure.WebJobs.Extensions.EventHubs_5.0.0-beta.1/sdk/eventhub/Microsoft.Azure.WebJobs.Extensions.EventHubs
            
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {eventData.EventBody}.  Enqueued time: {eventData.EnqueuedTime}. Partition: {eventData.PartitionKey}.");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
