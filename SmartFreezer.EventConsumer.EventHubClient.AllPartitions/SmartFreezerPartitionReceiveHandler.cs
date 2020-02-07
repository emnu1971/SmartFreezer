using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using SmartFreezer.Domain.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SmartFreezer.EventConsumer.Evhc.AllPartitions
{
    public class SmartFreezerPartitionReceiveHandler : IPartitionReceiveHandler
    {
        #region Storage
                
        public int MaxBatchSize => 10;
        public string PartitionId { get; }

        #endregion Storage

        #region C'tor

        public SmartFreezerPartitionReceiveHandler(string partitionId)
        {
            PartitionId = partitionId;
        }

        #endregion C'tor

        #region IPartitionReceiveHandler Implementation

        public Task ProcessErrorAsync(Exception error)
        {
            Console.WriteLine($"Exception: {error.Message}");
            return Task.CompletedTask;
        }

        public Task ProcessEventsAsync(IEnumerable<EventData> eventDatas)
        {
            if (eventDatas != null)
            {
                foreach (var eventData in eventDatas)
                {
                    // get event data as JSON string
                    var dataAsJson = Encoding.UTF8.GetString(eventData.Body.Array);

                    // deserialize json to FreezerEventdata instance
                    var freezerEventData = JsonConvert.DeserializeObject<FreezerEventData>(dataAsJson);

                    Console.WriteLine($"{freezerEventData} | PartitionId: {PartitionId}" +
                      $" | Offset: {eventData.SystemProperties.Offset}");
                }
            }
            return Task.CompletedTask;
        }

        #endregion IPartitionReceiveHandler Implementation
    }
}
