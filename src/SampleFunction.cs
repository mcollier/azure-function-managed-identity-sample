using Azure.Messaging.EventHubs;
using Azure.Storage.Queues.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Collier.Functions
{
    public static class SampleFunction
    {
        [FunctionName("SubmitOutputQueueMessagesFunction")]
        public static async Task<IActionResult> SubmitOutputQueueMessages(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [Queue("%OutputQueueName%", Connection="MyStorageConnection")]IAsyncCollector<string> messages,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            for (int i = 0; i < 10; i++)
            {
                await messages.AddAsync($"Hello, {i}!");
            }
            return new OkResult();
        }
        
        [FunctionName("ProcessStorageQueueFunction")]
        [return: Queue("%OutputQueueName%", Connection="MyStorageConnection")]
        public static string ProcessStorageQueue(
            [QueueTrigger("%QueueName%", Connection="MyStorageConnection" )] QueueMessage queueItem,
            ILogger log)
        {
            log.LogInformation($"C# function processed: {queueItem.MessageText}");

            string outputMessage = queueItem.MessageText + " output";
            return outputMessage;
        }

        [FunctionName("ProcessEventHubFunction")]
        public static async Task ProcessEventHub([EventHubTrigger("%EventHubName%", Connection = "EventHubConnectionString", ConsumerGroup="%EventHubConsumerGroup%")] EventData[] events, ILogger log)
        {
            // NOTE: Unable to get binding expression to work for ConsumerGroup.  Need to investigate more.  See prior SDK related issue at https://github.com/Azure/azure-functions-eventhubs-extension/issues/4
            
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
