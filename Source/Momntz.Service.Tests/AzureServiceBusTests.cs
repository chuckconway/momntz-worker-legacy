using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using NUnit.Framework;

namespace Momntz.Service.Tests
{
    [TestFixture]
    public class AzureServiceBusTests
    {
        [Test]
        public void Test()
        {
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");
          
            var client = QueueClient.CreateFromConnectionString(connectionString, "media", ReceiveMode.PeekLock);

            var demo = new DemoMessage {FirstName = "Chuck", LastName = "Conway"};
            var serialized = Newtonsoft.Json.JsonConvert.SerializeObject(demo);
            
            client.Send(new BrokeredMessage(serialized));


            BrokeredMessage message;
            while ((message = client.Receive()) != null)
            {
                message.

            }
        }
    }

    public class DemoMessage
    {
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>The first name.</value>
        public string FirstName { get; set; }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>The last name.</value>
        public string LastName { get; set; }
    }
}
