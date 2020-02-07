using System;

namespace SmartFreezer.Domain.Models
{
    /// <summary>
    /// Freezer Event Data
    /// </summary>
    public class FreezerEventData
    {
        public string City { get; set; }
        public string SerialNumber { get; set; }
        public string SensorType { get; set; }
        public int SensorValue { get; set; }
        public DateTime RecordingTime { get; set; }

        /// <summary>
        /// Should normally not be exposed as public ...
        /// </summary>
        public int MaxContent { get; set; }

        public override string ToString()
        {
            return $"Time: {RecordingTime:HH:mm:ss} | {SensorType}: {SensorValue} | "
                 + $"City: {City} | Serialnumber: {SerialNumber} | MaxContent: {MaxContent} ";
        }

    }
}
