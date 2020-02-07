using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
using Newtonsoft.Json;
using SmartFreezer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartFreezer.EventConsumer.Evph
{ 
    public class SmartFreezerEventProcessor : IEventProcessor
    {
        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Console.WriteLine($"Shutting down SmartFreezerData processor for partition {context.PartitionId} accessed by consumer group {context.ConsumerGroupName}. " +
              $"Reason: {reason}");
            return Task.CompletedTask;
        }

        public Task OpenAsync(PartitionContext context)
        {
            Console.WriteLine($"Initialized processor for partition {context.PartitionId} accessed by consumer group {context.ConsumerGroupName}");
            return Task.CompletedTask;
        }

        public Task ProcessErrorAsync(PartitionContext context, Exception error)
        {
            Console.WriteLine($"Error for partition {context.PartitionId}: {error.Message} accessed by consumer group {context.ConsumerGroupName}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(PartitionContext context,
          IEnumerable<EventData> smartFreezerEventDatas)
        {
            if (smartFreezerEventDatas != null)
            {
                foreach (var eventData in smartFreezerEventDatas)
                {
                    var dataAsJson = Encoding.UTF8.GetString(eventData.Body.Array);
                    var smartFreezerData =
                      JsonConvert.DeserializeObject<FreezerEventData>(dataAsJson);
                    Console.WriteLine($"{smartFreezerData} | PartitionId: {context.PartitionId}" +
                      $" | Offset: {eventData.SystemProperties.Offset} accessed by consumer group {context.ConsumerGroupName}");
                }
            }
            // This stores the current offset in the Azure Blob Storage
            return context.CheckpointAsync();
        }
    }

}

