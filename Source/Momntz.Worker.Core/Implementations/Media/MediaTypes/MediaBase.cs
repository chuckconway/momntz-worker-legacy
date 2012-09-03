using System;
using System.IO;
using Chucksoft.Storage;


namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public abstract class MediaBase
    {
        private readonly IStorage _storage;

        protected MediaBase(IStorage storage)
        {
            _storage = storage;
        }

        protected void AddToStorage(string storageContainer, string contenType, string name, string extension, byte[] bytes)
        {
            _storage.AddFile(storageContainer, name, string.Format("{0}/{1}", contenType, extension), bytes);
        }
    }
}
