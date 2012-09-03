using System.Data;
using System.Linq;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using Chucksoft.Storage;
using Hypersonic;
using Momntz.Model.Configuration;
using Momntz.Model.Core;
using Momntz.Worker.Core.Implementations.Media.MediaTypes;

namespace Momntz.Worker.Core.Implementations.Media
{
    public class MediaProcessor : IMessageProcessor
    {
        private readonly ISession _session;
        private readonly IStorage _storage;
        private readonly ISettings _settings;

        public MediaProcessor(ISession session, IStorage storage, ISettings settings)
        {
            _session = session;
            _storage = storage;
            _settings = settings;
        }
         
        public string MessageType
        {
            get { return typeof (MediaMessage).FullName; }
        }

        public void Process(string message)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            MediaMessage msg = serializer.Deserialize<MediaMessage>(message);

            var list = GetMediaTypes();
            var single = list.Single(m => m.Media == msg.MediaType);

            _session.Database.ConnectionString = _settings.QueueDatabase;
            _session.Database.CommandType = CommandType.Text;
            
            var item = _session.Query<MediaItem>("Media")
                .Where(i => i.Id == msg.Id)
                .Single();

            _session.Database.ConnectionString = null;

            single.Process(item);
        }

        private IEnumerable<IMedia> GetMediaTypes()
        {
            return new List<IMedia>
                       {
                           new DocumentProcessor(_storage, _session),
                           new ImageProcessor(_storage, _settings, _session),
                           new VideoProcessor(_storage, _session)
                       };
        }
    }
}
