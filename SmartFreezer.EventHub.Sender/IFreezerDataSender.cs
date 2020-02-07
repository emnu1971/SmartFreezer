using SmartFreezer.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartFreezer.EventHub.Sender
{
    public interface IFreezerDataSender
    {
        Task SendEventDataAsync(FreezerEventData freezerEventData);
        Task SendEventDataAsync(IEnumerable<FreezerEventData> freezerEventDatas);
    }
}
