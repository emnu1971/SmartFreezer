using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.ServiceBus;
using Newtonsoft.Json;
using SmartFreezer.Domain.Models;

namespace SmartFreezer.AzureFunctions
{
    public static class FreezerMaxContentExceededAlertFunction
    {

        //[FunctionName("FreezerMaxContentExceededAlertFunction")]
        //public static void Run([EventHubTrigger("smartfreezertelemetryeh",
        //    Connection = "TelemetryDataInputEventHubConnectionString",
        //    ConsumerGroup = "max_content_exceeded_alert_function_cg")]
        //string[] myEventHubMessages, TraceWriter log)
        //{

        //    foreach (var eventHubMessage in myEventHubMessages)
        //    {
        //        var data = JsonConvert.DeserializeObject<FreezerEventData>(eventHubMessage);
        //        log.Info($"C# Event Hub trigger function {nameof(FreezerMaxContentExceededAlertFunction)} processed an incoming message: {data}");
        //    }

        //}

        [FunctionName("FreezerMaxContentExceededAlertFunction")]
        public static void Run([EventHubTrigger("smartfreezertelemetryeh",
            Connection = "TelemetryDataInputEventHubConnectionString",
            ConsumerGroup = "max_content_exceeded_alert_function_cg")]
        string[] inputMessages,
            [EventHub("maxcontentreachedalerteh",
            Connection = "TelemetryDataOutputEventHubConnectionString")]
            ICollector<string> outputMessages,
            TraceWriter log)
        {

            foreach (var inputMessage in inputMessages)
            {
                var data = JsonConvert.DeserializeObject<FreezerEventData>(inputMessage);
                log.Info($"C# Event Hub trigger function {nameof(FreezerMaxContentExceededAlertFunction)} processed an incoming message: {data}");

                // check if FreezerContent value does not exceeds the MaxContent value of the freezer
                if (data.SensorType == "FreezerContent" && data.SensorValue > data.MaxContent)
                {
                    log.Info($"C# Event Hub trigger function {nameof(FreezerMaxContentExceededAlertFunction)} processed an outgoing message: {data}");
                    log.Info($"Reason: Current Content Value of {data.SensorValue} for Freezer with Serial: {data.SerialNumber} passed MaxContext of {data.MaxContent}");
                    // if maxcontent exceeds, create alert message to output event hub ...
                    outputMessages.Add(inputMessage);
                }
            }

        }

    }
}
