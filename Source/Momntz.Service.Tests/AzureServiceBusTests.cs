using System.Data;

using Momntz.Infrastructure.Logging;
using NUnit.Framework;

namespace Momntz.Service.Tests
{
    [TestFixture]
    public class AzureServiceBusTests
    {
        [Test]
        [LogAspect]
        public void Test()
        {
            string chuck = "chuck";

            //QueueService service = new QueueService();
            //service.Process();

            TestMethod("chuck", "chuck2", null);

            //var demo = new DemoMessage {FirstName = "Chuck", LastName = "Conway"};

            //IQueue q = new AzureQueue();
            ////q.Send("media", demo);

            //int messageCount = 0;

            //q.ProcessAllMessages<DemoMessage>("media", r => messageCount++);
        }

        [LogAspect]
        public void TestMethod(string chuck)
        {

        }

        [LogAspect]
        public void TestMethod(string chuck, int parmeterTwo)
        {

        }

        [LogAspect]
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
