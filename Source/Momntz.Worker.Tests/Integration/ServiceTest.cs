using Momntz.Service.Core.IOC;
using NUnit.Framework;

namespace Momntz.Worker.Tests.Integration
{
    [TestFixture]
    public class ServiceTest
    {
        [Test]
        public void Endpoint()
        {
            Server server = new Server();
            server.Start();
        }
    }
}
