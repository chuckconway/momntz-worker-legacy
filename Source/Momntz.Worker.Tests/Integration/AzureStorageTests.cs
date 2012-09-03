using System.IO;
using Chucksoft.Core.Services;
using Chucksoft.Storage;
using NUnit.Framework;

namespace Momntz.Worker.Tests.Integration
{
    [TestFixture]
    public class AzureStorageTests
    {
        [Test]
        public void Bucket_Create_BucketIsCreatedSuccessfully()
        {
            IStorage storage = new AzureStorage(new ConfigurationService());
            string name = "img"; //Path.GetRandomFileName();
            storage.CreateBucket(name);
        }

        [Test]
        public void File_AddFile_FileIsAddedSuccessfully()
        {
            IStorage storage = new AzureStorage(new ConfigurationService());
            storage.AddFile("img", Path.GetRandomFileName(), "image/jpg", new byte[128]);
        }
        
    }
}
