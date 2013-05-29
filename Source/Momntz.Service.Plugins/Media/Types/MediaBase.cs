
using ChuckConway.Cloud.Storage;

namespace Momntz.Service.Plugins.Media.Types
{
    public abstract class MediaBase
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaBase"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        protected MediaBase(IStorage storage)
        {
            _storage = storage;
        }


        /// <summary>
        /// Adds to storage.
        /// </summary>
        /// <param name="storageContainer">The storage container.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="name">The name.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="bytes">The bytes.</param>
        protected void AddToStorage(string storageContainer, string contentType, string name, string extension, byte[] bytes)
        {
            _storage.AddFile(storageContainer, name, string.Format("{0}/{1}", contentType, extension), bytes);
        }
    }
}
