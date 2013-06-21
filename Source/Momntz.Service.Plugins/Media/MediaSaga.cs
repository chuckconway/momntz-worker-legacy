using System.Collections.Generic;
using System.Linq;

using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure.Configuration;
using Momntz.Messaging;
using Momntz.Service.Plugins.Media.Types;
using Momntz.Service.Plugins.Media.Types.Documents;
using Momntz.Service.Plugins.Media.Types.Images;
using Momntz.Service.Plugins.Media.Types.Videos;
using NHibernate;
using Newtonsoft.Json;

namespace Momntz.Service.Plugins.Media
{
    public class MediaSaga : ISaga
    {
        private readonly List<MediaType> _processors;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSaga"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="settings">The settings.</param>
        public MediaSaga(IStorage storage, ISettings settings, ISessionFactory factory)
        {
            _processors = GetProcessors(storage, settings, factory);
        }

        /// <summary>
        /// Gets the processors.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="factory">The factory.</param>
        /// <returns>List{MomentoMediaType}.</returns>
        private List<MediaType> GetProcessors(IStorage storage, ISettings settings, ISessionFactory factory)
        {
            return new List<MediaType>
                {
                    new MediaType {MediaProcessor = new ImageProcessor(storage, settings, factory) , Extensions = new[]{"bmp", "gif", "jpg", "jpeg", "png", "tiff", "tif"}},
                    new MediaType {MediaProcessor = new VideoProcessor(storage), Extensions = new[]{"mp4"}},
                    new MediaType {MediaProcessor = new DocumentProcessor(), Extensions = new[]{ "doc", "docx", "pdf", "txt"}}
                };
        }

        /// <summary>
        /// Consumes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Consume(string message)
        {
            var mediaMessage = JsonConvert.DeserializeObject<Messaging.Models.Media>(message);
            var processor = _processors.First(m => m.Extensions.Contains(mediaMessage.Extension.ToLower()));
            processor.MediaProcessor.Consume(mediaMessage);
        }

        /// <summary>
        /// Gets the type. In Windows Azure Service Bus this is the same as a Subscription
        /// </summary>
        /// <value>The type.</value>
        public string Type { get { return "media"; } }

        private class MediaType
        {
            /// <summary>
            /// Gets or sets the extensions.
            /// </summary>
            /// <value>The extensions.</value>
            public string[] Extensions { get; set; }

            /// <summary>
            /// Gets or sets the media.
            /// </summary>
            /// <value>The media.</value>
            public IMedia MediaProcessor { get; set; }
        }
    }
}
