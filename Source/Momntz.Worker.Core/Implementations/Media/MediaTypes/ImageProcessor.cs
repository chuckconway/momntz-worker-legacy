using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;

using System.Linq;
using ChuckConway.Images;
using Momntz.Core;
using Momntz.Infrastructure;
using Momntz.Model;
using Momntz.Model.Configuration;
using NHibernate;

namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public class ImageProcessor : MediaBase, IMedia
    {
        private readonly ISettings _settings;
        private readonly IDatabaseConfiguration _databaseConfiguration;

        static readonly List<Format> _formats = new List<Format>
                {
                    new Format {ImageFormat = ImageFormat.Bmp, Extensions = new[]{"bmp"}},
                    new Format {ImageFormat = ImageFormat.Gif, Extensions = new[]{"gif"}},
                    new Format {ImageFormat = ImageFormat.Jpeg, Extensions = new[]{"jpg", "jpeg"}},
                    new Format {ImageFormat = ImageFormat.Png, Extensions = new[]{"png"}},
                    new Format {ImageFormat = ImageFormat.Tiff, Extensions = new[]{"tiff"}},
                };

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageProcessor" /> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="databaseConfiguration">The database configuration.</param>
        public ImageProcessor(IStorage storage, ISettings settings, IDatabaseConfiguration databaseConfiguration)
            : base(storage)
        {
            _settings = settings;
            _databaseConfiguration = databaseConfiguration;
        }

        /// <summary>
        /// Gets the media.
        /// </summary>
        /// <value>The media.</value>
        public string Media
        {
            get { return "Image"; }
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
                return item.ImageFormat;
            }

            return null;
        }

        /// <summary>
        /// Saves the specified momento id.
        /// </summary>
        /// <param name="momento">The momento.</param>
        /// <param name="name">The name.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="image">The image.</param>
        /// <param name="session">The session.</param>
        private static void Save(Momento momento, string name, MediaType mediaType, Model.QueueData.Media image, ISession session)
        {
            using (var trans = session.BeginTransaction())
            {
                session.Save(
                    new MomentoMedia
                        {
                            Filename = image.Filename,
                            Size = image.Size.ToString(CultureInfo.InvariantCulture),
                            Momento = momento,
                            Extension = image.Extension,
                            Url = "img/" + name,
                            Username = image.Username,
                            MediaType = mediaType
                        });

                trans.Commit();
            }
        }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Process(Model.QueueData.Media message)
        {
            using (ISession session = _databaseConfiguration.CreateSessionFactory().OpenSession())
            {
                var format = GetFormat(message.Extension);
                var momento = CreateMomento(message, session);

                using (var tran = session.BeginTransaction())
                {
                    var user = new MomentoUser { Momento = momento, Username = message.Username };
                    session.Save(user);

                    tran.Commit();
                }

                ResizeAndSaveImages(message, momento, format, session);
            }

            DeleteMediaFromQueueDatabase(message);
        }

        /// <summary>
        /// Resizes the and save images.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="momento">The momento.</param>
        /// <param name="format">The format.</param>
        /// <param name="session">The session.</param>
        private void ResizeAndSaveImages(Model.QueueData.Media message, Momento momento, ImageFormat format, ISession session)
        {
            SaveImage(momento, MediaType.SmallImage, _settings.ImageSmallWidth, _settings.ImageSmallHeight, format, message, session);
            SaveImage(momento, MediaType.MediumImage, _settings.ImageMediumWidth, _settings.ImageMediumHeight, format, message, session);
            SaveImage(momento, MediaType.LargeImage, _settings.ImageLargeWidth, _settings.ImageLargeHeight, format, message, session);
            SaveImage(momento, MediaType.OriginalImage, int.MaxValue, int.MaxValue, format, message, session);
        }

        /// <summary>
        /// Deletes the media from queue database.
        /// </summary>
        /// <param name="message">The message.</param>
        public virtual void DeleteMediaFromQueueDatabase(Model.QueueData.Media message)
        {
            using (var session = _databaseConfiguration.CreateSessionFactory(_settings.QueueDatabase).OpenSession())
            using (var trans = session.BeginTransaction())
            {
                session.Delete(message);
                trans.Commit();
            }
        }

        /// <summary>
        /// Creates the momento.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        /// <returns>Momento.</returns>
        public virtual Momento CreateMomento(Model.QueueData.Media message, ISession session)
        {
            var momento = new Momento
                {
                    InternalId = message.Id,
                    Username = message.Username,
                    UploadedBy = message.Username,
                    Visibility = "Public",
                    
                };

            using (var tran = session.BeginTransaction())
            {
                session.Save(momento);
                tran.Commit();
            }

            return momento;
        }

        /// <summary>
        /// Saves the image.
        /// </summary>
        /// <param name="momento">The momento.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="maxWidth">Width of the max.</param>
        /// <param name="maxHeight">Height of the max.</param>
        /// <param name="format">The format.</param>
        /// <param name="message">The message.</param>
        /// <param name="session">The session.</param>
        private void SaveImage(Momento momento, MediaType mediaType, int maxWidth, int maxHeight, ImageFormat format, Model.QueueData.Media message, ISession session)
        {
            var name = SaveToStorage(maxWidth, maxHeight, format, message);

            if (mediaType == MediaType.OriginalImage)
            {
                var imageMetadata = new ExifData(session);
                imageMetadata.ExtractExifSave(new MemoryStream(message.Bytes), momento);
            }

            Save(momento, name, mediaType, message, session);
        }

        /// <summary>
        /// Saves to storage.
        /// </summary>
        /// <param name="maxWidth">Width of the max.</param>
        /// <param name="maxHeight">Height of the max.</param>
        /// <param name="format">The format.</param>
        /// <param name="message">The message.</param>
        /// <returns>System.String.</returns>
        private string SaveToStorage(int maxWidth, int maxHeight, ImageFormat format, Model.QueueData.Media message)
        {
            byte[] bytes = null;

            if (maxWidth < int.MaxValue && maxHeight < int.MaxValue)
            {
                bytes = message.Bytes.ResizeToMax(new Size(maxWidth, maxHeight), format);
            }

            string type = message.Extension.TrimStart('.');
            string name = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(message.Filename), DateTime.Now.Ticks, message.Extension);

            AddToStorage("img", "image", name, type, bytes ?? message.Bytes);
            return name;
        }

        private class Format
        {
            /// <summary>
            /// Gets or sets the image format.
            /// </summary>
            /// <value>The image format.</value>
            public ImageFormat ImageFormat { get; set; }

            /// <summary>
            /// Gets or sets the extensions.
            /// </summary>
            /// <value>The extensions.</value>
            public string[] Extensions { get; set; }
        }
    }
}
