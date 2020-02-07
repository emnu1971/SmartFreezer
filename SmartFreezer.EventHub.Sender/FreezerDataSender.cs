using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Newtonsoft.Json;
using System.Text;
using System.Linq;
using SmartFreezer.Domain.Models;

namespace SmartFreezer.EventHub.Sender
{
    public class FreezerDataSender : IFreezerDataSender
    {

        #region Storage

        private EventHubClient _eventHubClient;

        #endregion Storage

        #region C'tor

        public FreezerDataSender(string eventHubConnectionString)
        {
            _eventHubClient = EventHubClient.CreateFromConnectionString(eventHubConnectionString);
        }

        #endregion C'tor
        
        #region IFreezerDataService Implementation

        public async Task SendEventDataAsync(FreezerEventData freezerEventData)
        {
            EventData eventData = CreateEventData(freezerEventData);
            await _eventHubClient.SendAsync(eventData);
        }

        public async Task SendEventDataAsync(IEnumerable<FreezerEventData> freezerEventDatas)
        {
            var eventDatas = freezerEventDatas.Select(freezerMachineData => CreateEventData(freezerMachineData));

            var eventDataBatch = _eventHubClient.CreateBatch();

            foreach (var eventData in eventDatas)
            {
                if (!eventDataBatch.TryAdd(eventData))
                {
                    await _eventHubClient.SendAsync(eventDataBatch.ToEnumerable());
                    eventDataBatch = _eventHubClient.CreateBatch();
                    eventDataBatch.TryAdd(eventData);
                }
            }

            if (eventDataBatch.Count > 0)
            {
                await _eventHubClient.SendAsync(eventDataBatch.ToEnumerable());
            }
        }

        #endregion IFreezerDataService Implementation

        #region Private Interface

        private static EventData CreateEventData(FreezerEventData freezerEventData)
        {
            var dataAsJson = JsonConvert.SerializeObject(freezerEventData);
            var eventData = new EventData(Encoding.UTF8.GetBytes(dataAsJson));
            return eventData;
        }

        #endregion Private Interface
    }
}
