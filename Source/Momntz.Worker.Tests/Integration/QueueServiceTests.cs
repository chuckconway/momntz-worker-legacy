using Momntz.Service;
using NUnit.Framework;

namespace Momntz.Worker.Tests.Integration
{
    [TestFixture]
    public class QueueServiceTests
    {
        [Test]
        public void QueueService_Process_NoErrors()
        {
            QueueService service = new QueueService();
            service.Process();
        }
    }
}
