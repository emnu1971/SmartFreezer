using Microsoft.Hadoop.Avro;
using Microsoft.Hadoop.Avro.Container;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using SmartFreezer.Domain.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartFreezer.EventArchive.CaptureReceiver
{
    class Program
    {
        // TODO: Enter the connection string of your storage account here
        const string storageConnectionString = "DefaultEndpointsProtocol=https;AccountName=freezereventdatacapture;AccountKey=8wCEcpEOOg61Yv8ndeXMUt0Wt1EDe6/W/4CEPlvbgU1HATEhQIqZSkRKJDH9LRFZI39GBbSFyA6LetXgn+yaSg==;EndpointSuffix=core.windows.net";

        // TODO: Enter the blob container name here
        const string containerName = "freezereventhubdatacapture";

        static async Task Main(string[] args)
        {
            // get the storage account in azure
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // create a blobclient on the storage account
            var blobClient = storageAccount.CreateCloudBlobClient();

            // create a blobcontainer
            var blobContainer = blobClient.GetContainerReference(containerName);

            // list all the items from the blobcontainer
            // the overload useFlatBlobListing has the advantage that you only get the files, not the folder
            var resultSegment = await blobContainer.ListBlobsSegmentedAsync(null, true, BlobListingDetails.All, null, null, null, null);

            // we just want to have CloudBlockBlob
            // CloudBlockBlob is a file which contains the events.
            foreach(var cloudBlockBlob in resultSegment.Results.OfType<CloudBlockBlob>())
            {
                await ProcessCloudBlockBlobAsync(cloudBlockBlob);
            }

            Console.ReadLine();
        }

        private static async Task ProcessCloudBlockBlobAsync(CloudBlockBlob cloudBlockBlob)
        {
            // download the freezer events in avro format
            var avroRecords = await DownloadAvroRecordsAsync(cloudBlockBlob);
            // print the events on the console
            PrintSmartFreezerDataEvents(avroRecords);
        }

        private static async Task<List<AvroRecord>> DownloadAvroRecordsAsync(CloudBlockBlob cloudBlockBlob)
        {
            // download the freezer evenets from the blob storage 
            // and put them in a memory stream (for large files, use filestream instead)
            var memoryStream = new MemoryStream();
            await cloudBlockBlob.DownloadToStreamAsync(memoryStream);
            memoryStream.Seek(0, SeekOrigin.Begin);

            // read memorystream and create list of avro records from stream
            List<AvroRecord> avroRecords;
            using (var reader = AvroContainer.CreateGenericReader(memoryStream))
            {
                using (var sequentialReader = new SequentialReader<object>(reader))
                {
                    avroRecords = sequentialReader.Objects.OfType<AvroRecord>().ToList();
                }
            }

            // return the avro records
            return avroRecords;
        }

        private static void PrintSmartFreezerDataEvents(List<AvroRecord> avroRecords)
        {
            var smartFreezerEventDatas = avroRecords.Select(avroRecord =>
                CreateSmartFreezerData(avroRecord));

            foreach (var freezerEventData in smartFreezerEventDatas)
            {
                Console.WriteLine(freezerEventData);
            }
        }


        private static FreezerEventData CreateSmartFreezerData(AvroRecord avroRecord)
        {
            // body of avrorecord contains eventdata in JSON format, get as byte[]
            var body = avroRecord.GetField<byte[]>("Body");

            // create JSON string from byte[]
            var dataAsJson = Encoding.UTF8.GetString(body);

            // deserialize JSON to FreezerEventData object
            var freezerEventData = JsonConvert.DeserializeObject<FreezerEventData>(dataAsJson);

            return freezerEventData;
        }
    }
}
