using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using ChuckConway.Cloud.Storage;
using ChuckConway.Images;
using Momntz.Data.Schema;
using Momntz.Infrastructure.Configuration;
using Momntz.Messaging;
using NHibernate;

namespace Momntz.Service.Plugins.Media.Types.Images
{
    public class ImageProcessor : IMedia
    {
        private readonly IStorage _storage;
        private readonly ISettings _settings;
        private readonly ISessionFactory _sessionFactory;

        static readonly List<ImageType> _formats = new List<ImageType>
        {
            new ImageType {Format = ImageFormat.Bmp, Extensions = new[]{"bmp"}},
            new ImageType {Format = ImageFormat.Gif, Extensions = new[]{"gif"}},
            new ImageType {Format = ImageFormat.Jpeg, Extensions = new[]{"jpg", "jpeg"}},
            new ImageType {Format = ImageFormat.Png, Extensions = new[]{"png"}},
            new ImageType {Format = ImageFormat.Tiff, Extensions = new[]{"tiff"}},
            new ImageType {Format = ImageFormat.Tiff, Extensions = new[]{"tif"}},
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessor" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="sessionFactory">The session factory.</param>
        public ImageProcessor(IStorage storage, ISettings settings, ISessionFactory sessionFactory)
        {
            _storage = storage;
            _settings = settings;
            _sessionFactory = sessionFactory;
        }
        
        /// <summary>
        /// Gets the format.
        /// </summary>
        /// <param name="imageFormat">The image format.</param>
        /// <returns>ImageFormat.</returns>
        private static ImageFormat GetFormat(string imageFormat)
        {
            var item = _formats.SingleOrDefault(f => f.Extensions.Any(s => string.Equals(s, imageFormat.ToLower().Trim('.'), StringComparison.InvariantCulture)));

            if (item != null)
            {
                return item.Format;
            }

            return null;
        }

        /// <summary>
        /// Saves the specified momento id.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="parameters">The parameters.</param>
        /// <param name="session">The session.</param>
        private static void Save(string name, InParameters parameters, ISession session)
        {
            using (var trans = session.BeginTransaction())
            {
                session.Save(
                    new MomentoMedia
                        {
                            Filename = parameters.MediaMessage.Filename,
                            Size = parameters.MediaMessage.Size.ToString(CultureInfo.InvariantCulture),
                            Momento = parameters.Momento,
                            Extension = parameters.MediaMessage.Extension,
                            Url = "img/" + name,
                            Username = parameters.MediaMessage.Username,
                            MediaType = parameters.MomentoMediaType
                        });

                trans.Commit();
            }
        }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Consume(Messaging.Models.Media message)
        {
            if (message != null)
            {
                var bytes = _storage.GetFile(QueueConstants.MediaQueue, message.Id.ToString());
                var format = GetFormat(message.Extension);

                using (ISession session = _sessionFactory.OpenSession())
                {
                    var momento = Create(message, session);

                    using (var tran = session.BeginTransaction())
                    {
                        var user = new MomentoUser {Momento = momento, Username = message.Username};
                        session.Save(user);

                        tran.Commit();
                    }

                    var imageConfigurations = GetImageConfigurations(bytes, format, message, momento);

                    ResizeAndSaveImages(imageConfigurations, session);
                }
            }
        }

        /// <summary>
        /// Gets the image configurations.
        /// </summary>
        /// <param name="bytes">The bytes.</param>
        /// <param name="format">The format.</param>
        /// <param name="momento">The momento.</param>
        /// <returns>List{InParameters}.</returns>
        private IEnumerable<InParameters> GetImageConfigurations(byte[] bytes, ImageFormat format, Messaging.Models.Media mediaMessage, Momento momento)
        {
            return new List<InParameters>
            {
                new InParameters { Bytes = bytes, MomentoMediaType = MomentoMediaType.SmallImage, Format = format, MediaMessage = mediaMessage, MaxHeight = _settings.ImageSmallHeight, MaxWidth = _settings.ImageSmallWidth, Momento = momento},
                new InParameters { Bytes = bytes, MomentoMediaType = MomentoMediaType.MediumImage, Format = format, MediaMessage = mediaMessage, MaxHeight = _settings.ImageMediumHeight, MaxWidth = _settings.ImageMediumWidth, Momento = momento},
                new InParameters { Bytes = bytes, MomentoMediaType = MomentoMediaType.LargeImage, Format = format, MediaMessage = mediaMessage, MaxHeight = _settings.ImageLargeHeight, MaxWidth = _settings.ImageLargeWidth, Momento = momento},
                new InParameters { Bytes = bytes, MomentoMediaType = MomentoMediaType.OriginalImage, Format = format, MediaMessage = mediaMessage, MaxHeight = int.MaxValue, MaxWidth = int.MaxValue, Momento = momento},
                    
            };
        }

        /// <summary>
        /// Resizes the and save images.
        /// </summary>
        /// <param name="configurations">The configurations.</param>
        /// <param name="session">The session.</param>
        private void ResizeAndSaveImages(IEnumerable<InParameters> configurations, ISession session)
        {
            foreach (var configuration in configurations)
            {
                SaveImage(configuration, session);
            }
        }

        /// <summary>
        /// Creates the momento.
        /// </summary>
        /// <param name="mediaMessage">The MediaMessage.</param>
        /// <param name="session">The session.</param>
        /// <returns>Momento.</returns>
        public virtual Momento Create(Messaging.Models.Media mediaMessage, ISession session)
        {
            var momento = PopulateMomentoObject(mediaMessage);

            using (var tran = session.BeginTransaction())
            {
                session.Save(momento);
                tran.Commit();
            }

            return momento;
        }

        /// <summary>
        /// Populates the momento object.
        /// </summary>
        /// <param name="mediaMessage">The media message.</param>
        /// <returns>Momento.</returns>
        private static Momento PopulateMomentoObject(Messaging.Models.Media mediaMessage)
        {
            var momento = new Momento
                {
                    InternalId = mediaMessage.Id,
                    User = new User {Username = mediaMessage.Username},
                    UploadedBy = mediaMessage.Username,
                    Visibility = "Public"
                };
            return momento;
        }

        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <param name="session">The session.</param>
        private void SaveImage(InParameters parameters, ISession session)
        {
            var name = SaveToStorage(parameters);

            if (parameters.MomentoMediaType == MomentoMediaType.OriginalImage)
            {
                var imageMetadata = new ExifData(session);
                imageMetadata.ExtractExifSave(new MemoryStream(parameters.Bytes), parameters.Momento);
            }

            Save(name, parameters, session);
        }

        /// <summary>
        /// Saves to storage.
        /// </summary>
        /// <param name="parameters">The parameters.</param>
        /// <returns>System.String.</returns>
        private string SaveToStorage(InParameters parameters)
        {
            byte[] bytes = null;

            if (parameters.MaxWidth < int.MaxValue && parameters.MaxHeight < int.MaxValue)
            {
                bytes = parameters.Bytes.ResizeToMax(new Size(parameters.MaxWidth, parameters.MaxHeight), parameters.Format);
            }

            string type = parameters.MediaMessage.Extension.TrimStart('.');
            string name = string.Format("{0}_{1}.{2}", Path.GetFileNameWithoutExtension(parameters.MediaMessage.Filename), DateTime.Now.Ticks, parameters.MediaMessage.Extension);

            AddToStorage("img", "image", name, type, bytes ?? parameters.Bytes);
            return name;
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

        private class InParameters
        {
            /// <summary>
            /// Gets or sets the momento.
            /// </summary>
            /// <value>The momento.</value>
            public Momento Momento { get; set; }

            /// <summary>
            /// Gets or sets the MediaMessage.
            /// </summary>
            /// <value>The MediaMessage.</value>
            public Messaging.Models.Media MediaMessage { get; set; }

            /// <summary>
            /// Gets or sets the height of the max.
            /// </summary>
            /// <value>The height of the max.</value>
            public int MaxHeight { get; set; }

            /// <summary>
            /// Gets or sets the width of the max.
            /// </summary>
            /// <value>The width of the max.</value>
            public int MaxWidth { get; set; }

            /// <summary>
            /// Gets or sets the format.
            /// </summary>
            /// <value>The format.</value>
            public ImageFormat Format { get; set; }

            /// <summary>
            /// Gets or sets the type of the momento media.
            /// </summary>
            /// <value>The type of the momento media.</value>
            public MomentoMediaType MomentoMediaType { get; set; }

            /// <summary>
            /// Gets or sets the bytes.
            /// </summary>
            /// <value>The bytes.</value>
            public byte[] Bytes { get; set; }
        }

    }
}
