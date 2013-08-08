using System.Net;
using System.Text;
using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure.Instrumentation.Logging.Models;
using Momntz.Messaging;
using Newtonsoft.Json;
using RestSharp;

namespace Momntz.Service.Plugins.Logging
{
    public class LoggerSaga : ISaga
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoggerSaga" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        public LoggerSaga(IStorage storage)
        {
            _storage = storage;
        }

        public string Type { get { return "logging"; } }
        public void Consume(string message)
        {
            var q = JsonConvert.DeserializeObject<QueueLogMessage>(message);

            //Get message from storage account.
            var rawBytes = _storage.GetFile(QueueConstants.LoggingQueue, q.Id.ToString());
            var text = Encoding.Default.GetString(rawBytes);

            //make a post to loggly with content.
            var client = new RestClient();
            RestRequest request = new RestRequest(q.Endpoint);
            request.AddParameter("application/json", text, ParameterType.RequestBody);
            request.Method = Method.POST;
            var response = client.Execute(request);
            var code = response.StatusCode;

            //remove message from storage account.
            _storage.DeleteFile(QueueConstants.LoggingQueue, q.Id.ToString());
        }
    }
}
