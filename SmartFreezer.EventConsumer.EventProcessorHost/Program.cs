using Microsoft.Azure.EventHubs.Processor;
using System;
using System.Threading.Tasks;

namespace SmartFreezer.EventConsumer.Evph
{ 
    class Program
    {

        const string eventHubPath = "smartfreezertelemetryeh";
        const string consumerGroupName = "$Default";
        const string eventHubConnectionString = "Endpoint=sb://smartfreezertelemetryeh-ns.servicebus.windows.net/;SharedAccessKeyName=SendAndListenPolicy;SharedAccessKey=RRISPOJLWPf52nKcXUUQNwVsoCCHMMb4yPxzYeAWWLo=;EntityPath=smartfreezertelemetryeh";
        const string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=freezereventdatacapture;AccountKey=8wCEcpEOOg61Yv8ndeXMUt0Wt1EDe6/W/4CEPlvbgU1HATEhQIqZSkRKJDH9LRFZI39GBbSFyA6LetXgn+yaSg==;EndpointSuffix=core.windows.net";
        const string leaseContainerName = "smartfreezerprocesscheckpoint";

        static async Task Main(string[] args)
        {
            Console.WriteLine($"Register the {nameof(SmartFreezerEventProcessor)}");

            var eventProcessorHost = new EventProcessorHost(
              eventHubPath,
              consumerGroupName,
              eventHubConnectionString,
              storageConnectionString,
              leaseContainerName);

            await eventProcessorHost.RegisterEventProcessorAsync<SmartFreezerEventProcessor>();

            Console.WriteLine("Waiting for incoming events...");
            Console.WriteLine("Press any key to shutdown");
            Console.ReadLine();

            await eventProcessorHost.UnregisterEventProcessorAsync();
        }
    }
}
