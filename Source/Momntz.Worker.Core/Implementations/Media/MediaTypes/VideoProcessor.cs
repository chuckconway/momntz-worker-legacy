using Chucksoft.Storage;
using Momntz.Core;
using NHibernate;


namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public class VideoProcessor : IMedia
    {
        private readonly IStorage _storage;

        /// <summary>
        /// Initializes a new instance of the <see cref="VideoProcessor" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="databaseConfiguration">The database configuration.</param>
        public VideoProcessor(IStorage storage, IDatabaseConfiguration databaseConfiguration)
        {
            _storage = storage;

        }

        public string Media
        {
            get { return "Video"; }
        }

        public void Process(Model.QueueData.Media message)
        {
            
        }
    }
}
