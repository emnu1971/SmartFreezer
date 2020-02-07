using Microsoft.Azure.EventHubs;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SmartFreezer.EventConsumer.Evhc.SinglePartition
{
    class Program
    {
        // TODO: put connectionstring in configuration file
        private const string _eventHubConnectionString = "Endpoint=sb://smartfreezertelemetryeh-ns.servicebus.windows.net/;SharedAccessKeyName=SendAndListenPolicy;SharedAccessKey=RRISPOJLWPf52nKcXUUQNwVsoCCHMMb4yPxzYeAWWLo=;EntityPath=smartfreezertelemetryeh";
        private const int _partitionId = 1;
        static async Task Main(string[] args)
        {
            Console.WriteLine("Connecting to the Event Hub ...");

            // create an event hub client
            var eventHubClient = EventHubClient.CreateFromConnectionString(_eventHubConnectionString);

            // create partitionreceiver (receives from a single partition)
            // CreateReceiver(consumerGroupName,PartitionId,StartTime=Time from where PartitionReceiver will start from receiving events
            var partitionReceiver = eventHubClient.CreateReceiver("$Default", _partitionId.ToString(), DateTime.UtcNow);

            Console.WriteLine($"Reading from PartitionId: {_partitionId}");
            Console.WriteLine("Waiting for incoming events ...");
            while (true)
            {
                // use partitionreceiver to read events from partition with id "0", this returns an IEnumerable of EventData objects
                // specify the max number of messages that you want to receive in a single batch (e.g. 10)
                var eventDatas = await partitionReceiver.ReceiveAsync(10);

                if (eventDatas != null)
                {
                    foreach (var eventData in eventDatas)
                    {
                        // body contains the FreezerEventData object serialized as JSON, the Array property returns the Byte[],
                        // next encode the Array and we get the JSON string !
                        var dataAsJson = Encoding.UTF8.GetString(eventData.Body.Array);

                        // write JSON to Console ...
                        Console.WriteLine($"{dataAsJson} | PartitionId= {partitionReceiver.PartitionId}");
                    }
                }
            }
        }
    }
}
