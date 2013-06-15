using ChuckConway.Cloud.Storage;
using Momntz.Core;

namespace Momntz.Service.Plugins.Media.Types.Videos
{
    public class VideoProcessor : IMedia
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProcessor" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="databaseConfiguration">The database configuration.</param>
        public VideoProcessor(IStorage storage)
        {
            _storage = storage;

        }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Consume(Messaging.Models.Media message)
        {
            throw new System.NotImplementedException();
        }
    }
}
