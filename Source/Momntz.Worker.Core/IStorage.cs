using System.IO;

namespace Momntz.Worker.Core
{
    public interface IStorage
    {
        /// <summary>
        /// Creates the bucket.
        /// </summary>
        /// <param name="name">The name.</param>
        void CreateBucket(string name);

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="file">The file.</param>
        void AddFile(string bucketName, string keyName, string contentType, byte[] file);

        /// <summary>
        /// Adds the file.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        /// <param name="contentType">Type of the content.</param>
        /// <param name="stream">The stream.</param>
        void AddFile(string bucketName, string keyName, string contentType, Stream stream);


        /// <summary>
        /// Deletes the file.
        /// </summary>
        /// <param name="bucketName">Name of the bucket.</param>
        /// <param name="keyName">Name of the key.</param>
        void DeleteFile(string bucketName, string keyName);

        /// <summary>
        /// Gets the file.
        /// </summary>
        /// <returns></returns>
        byte[] GetFile(string bucketName, string keyName);
    }
}
