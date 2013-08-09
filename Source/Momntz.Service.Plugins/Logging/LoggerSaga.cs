using System;
using System.Text;
using ChuckConway.Cloud.Storage;
using Microsoft.ServiceBus.Messaging;
using Momntz.Infrastructure.Configuration;
using Momntz.Infrastructure.Instrumentation.Logging;
using Momntz.Infrastructure.Instrumentation.Logging.Models;
using Momntz.Messaging;
using RestSharp;

namespace Momntz.Service.Plugins.Logging
{
    public class LoggerSaga : ISaga
    {
        private readonly IStorage _storage;
        private readonly ApplicationSettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerSaga" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        public LoggerSaga(IStorage storage, ApplicationSettings settings)
        {
            _storage = storage;
            _settings = settings;
        }

        public string Type { get { return "logging"; } }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Consume(BrokeredMessage message)
        {
            try
            {
                var msg = message.GetBody<QueueLogMessage>();

                //Get message from storage account.
                var rawBytes = _storage.GetFile(QueueConstants.LoggingQueue, msg.Id.ToString());
                var text = Encoding.Default.GetString(rawBytes);

                //make a post to loggly with content.
                var client = new RestClient();
                var request = new RestRequest(msg.Endpoint);
                request.AddParameter("application/json", text, ParameterType.RequestBody);
                request.Method = Method.POST;
                 client.Execute(request);
 
                //remove message from storage account.
                _storage.DeleteFile(QueueConstants.LoggingQueue, msg.Id.ToString());
            }
            catch (Exception ex)
            {
                var logToFileSystem = new LogToFile(_settings.LoggingFilePath);
                logToFileSystem.Exception(ex);
            }
        }
    }
}
