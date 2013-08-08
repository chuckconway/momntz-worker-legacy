using System.Data;

using NUnit.Framework;

namespace Momntz.Service.Tests
{
    [TestFixture]
    public class AzureServiceBusTests
    {
        [Test]
        public void Test()
        {
            //string chuck = "chuck";

            var service = new QueueService();
            service.Process();

            //TestMethod("chuck", "chuck2", null);

            //var demo = new DemoMessage {FirstName = "Chuck", LastName = "Conway"};

            //IQueue q = new AzureQueue();
            ////q.Send("media", demo);

            //int messageCount = 0;

            //q.ProcessAllMessages<DemoMessage>("media", r => messageCount++);
        }


        public void TestMethod(string chuck)
        {

        }


        public void TestMethod(string chuck, int parmeterTwo)
        {

        }

        public void TestMethod(string chuck, string parmeterTwo, string nullValue = null)
        {

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
