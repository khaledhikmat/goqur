using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using goqur.shared.models;
using Newtonsoft.Json;

namespace goqur.shared.services
{
    public class NotificationService : INotificationService
    {
        private static QueueClient _queueClient = null;
         private string _connectionString;
        private string _queueName;

        public NotificationService(string connectionString, string queueName)
        {
            _connectionString = connectionString;
            _queueName = queueName;
        }

        public async Task EnqueueAyahsList(List<Ayah> ayahs)
        {
            // WARNING: Enqueue the collection OR one at a time
            // foreach (Ayah ayah in ayahs) 
            // {
            //     Console.WriteLine($"NotificationService - ID: {ayah.Id} - ayah number: {ayah.AyahNumber} - english nouns: {ayah.EnglishNouns.Count} - english concepts: {ayah.EnglishConcepts.Count}");
            // }
            var messageAsJson = JsonConvert.SerializeObject(ayahs);
            await GetQueueClient().SendMessageAsync(messageAsJson);
        }

       private QueueClient GetQueueClient() 
        {
            if (_queueClient == null)
            {
                // Create a queue client with message encoding set to base64
                _queueClient = new QueueClient(_connectionString, _queueName, new QueueClientOptions {
                    MessageEncoding = QueueMessageEncoding.Base64
                });

                // Create the queue if it does not exist
                _queueClient.CreateIfNotExists();
            }

            return _queueClient;
        }
    }
}
