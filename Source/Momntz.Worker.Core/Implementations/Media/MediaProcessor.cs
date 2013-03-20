using System.Linq;
using System.Collections.Generic;
using Chucksoft.Storage;

using Momntz.Core;
using Momntz.Model.Configuration;
using Momntz.Model.Core;
using Momntz.Worker.Core.Implementations.Media.MediaTypes;
using Newtonsoft.Json;

namespace Momntz.Worker.Core.Implementations.Media
{
    public class MediaProcessor : IMessageProcessor
    {
        private readonly IDatabaseConfiguration _databaseConfiguration;
        private readonly IStorage _storage;
        private readonly ISettings _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaProcessor"/> class.
        /// </summary>
        /// <param name="databaseConfiguration">The database configuration.</param>
        /// <param name="storage">The storage.</param>
        /// <param name="settings">The settings.</param>
        public MediaProcessor(IDatabaseConfiguration databaseConfiguration, IStorage storage, ISettings settings)
        {
            _databaseConfiguration = databaseConfiguration;
            _storage = storage;
            _settings = settings;
        }

        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <value>The type of the message.</value>
        public string MessageType
        {
            get { return typeof (MediaMessage).FullName; }
        }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Process(string message)
        {
            var msg = JsonConvert.DeserializeObject<MediaMessage>(message);

            var list = GetMediaTypes();
            var single = list.Single(m => m.Media == msg.MediaType);

            var media = ReteiveMedia(msg);

            single.Process(media);
        }

        /// <summary>
        /// Reteives the media.
        /// </summary>
        /// <param name="msg">The MSG.</param>
        /// <returns>Model.QueueData.Media.</returns>
        private Model.QueueData.Media ReteiveMedia(MediaMessage msg)
        {
            Model.QueueData.Media media;

            using (var session = _databaseConfiguration.CreateSessionFactory(_settings.QueueDatabase).OpenSession())
            using (var tran = session.BeginTransaction())
            {
                media = session.QueryOver<Model.QueueData.Media>()
                               .Where(x => x.Id == msg.Id)
                               .SingleOrDefault();

                tran.Commit();
            }

            return media;
        }

        private IEnumerable<IMedia> GetMediaTypes()
        {
            return new List<IMedia>
                       {
                           new DocumentProcessor(_storage, _databaseConfiguration),
                           new ImageProcessor(_storage, _settings, _databaseConfiguration),
                           new VideoProcessor(_storage, _databaseConfiguration)
                       };
        }
    }
}
