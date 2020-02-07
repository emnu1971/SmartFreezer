using Microsoft.Azure.EventHubs;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SmartFreezer.EventConsumer.Evhc.AllPartitions
{
    class Program
    {
        // TODO: put connectionstring in configuration file

        // telemetry data input connectionstring
        //private const string _eventHubConnectionString = "Endpoint=sb://smartfreezertelemetryeh-ns.servicebus.windows.net/;SharedAccessKeyName=SendAndListenPolicy;SharedAccessKey=RRISPOJLWPf52nKcXUUQNwVsoCCHMMb4yPxzYeAWWLo=;EntityPath=smartfreezertelemetryeh";

        // telemetry data output connectionstring (alert max content exceeds)
        private const string _eventHubConnectionString = "Endpoint=sb://smartfreezertelemetryeh-ns.servicebus.windows.net/;SharedAccessKeyName=SendAndListenPolicy;SharedAccessKey=teG+iaIt1BFmjG8XkMw9xrVxOBtgAYsgihJx3aQ/LkU=;EntityPath=maxcontentreachedalerteh";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine($"Connecting to the Event Hub with connection : {_eventHubConnectionString}");
            

            // create an event hub client
            var eventHubClient = EventHubClient.CreateFromConnectionString(_eventHubConnectionString);

            // get partition info from the Event Hub
            var runtimeInformation = await eventHubClient.GetRuntimeInformationAsync();

            // create a partitionreceiver for each partition of the event hub
            var partitionReceivers = runtimeInformation.PartitionIds.Select(partitionId =>
                eventHubClient.CreateReceiver("$Default",
                partitionId, DateTime.UtcNow)).ToList();

            Console.WriteLine("Waiting for incoming events ...");

            // read events from all partitions
            foreach (var partitionReceiver in partitionReceivers)
            {
                // create partition receive handler for each partition receiver
                partitionReceiver.SetReceiveHandler(new SmartFreezerPartitionReceiveHandler(partitionReceiver.PartitionId));

                Console.WriteLine($"Reading from PartitionId: {partitionReceiver.PartitionId}");
            }

            Console.WriteLine("Press any key to shutdown");
            Console.ReadLine();
            await eventHubClient.CloseAsync();
        }
    }
}
