using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Chucksoft.Core.Drawing;
using Chucksoft.Storage;
using System.Linq;
using Hypersonic;
using Momntz.Model;
using Momntz.Model.Configuration;

namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public class ImageProcessor : MediaBase, IMedia
    {
        private readonly ISettings _settings;
        private readonly ISession _session;

        static readonly List<Format> _formats =  new List<Format>
                {
                    new Format {ImageFormat = ImageFormat.Bmp, Extensions=new[]{"bmp"}},
                    new Format {ImageFormat = ImageFormat.Gif, Extensions=new[]{"gif"}},
                    new Format {ImageFormat = ImageFormat.Jpeg, Extensions=new[]{"jpg", "jpeg"}},
                    new Format {ImageFormat = ImageFormat.Png, Extensions=new[]{"png"}},
                    new Format {ImageFormat = ImageFormat.Tiff, Extensions=new[]{"tiff"}},
                };

        public ImageProcessor(IStorage storage, ISettings settings, ISession session) :base(storage)
        {
            _settings = settings;
            _session = session;
        }

        public string Media
        {
            get { return "Image"; }
        }

        private static ImageFormat GetFormat(string imageFormat)
        {
            var item = _formats.SingleOrDefault(f => f.Extensions.Any(s => string.Equals(s, imageFormat.ToLower().Trim('.'), StringComparison.InvariantCulture)));

            if(item != null)
            {
                return item.ImageFormat;
            }

            return null;
        }

        /// <summary>
        /// Saves the specified momento id.
        /// </summary>
        /// <param name="momentoId">The momento id.</param>
        /// <param name="name">The name.</param>
        /// <param name="mediaType">Type of the media.</param>
        /// <param name="image">The image.</param>
        private void Save(int momentoId, string name, MediaType mediaType, MediaItem image)
        {

            _session.Save(
                new
                    {
                        image.Filename,
                        image.Size,
                        MomentoId = momentoId,
                        image.Extension,
                        Url = "img/" + name,
                        image.Username,
                        MediaType = mediaType
                    }, "MomentoMedia");
        }


        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void Process(MediaItem message)
        {
            ImageFormat format = GetFormat(message.Extension);

            _session.Save(new { InternalId = message.Id, message.Username, UploadedBy = message.Username, Visibility = "Public" }, "Momento");

            var single = _session.Query<Momento>().Where(m => m.InternalId == message.Id).Single();

            _session.Save(new {MomentoId = single.Id, Username = message.Username}, "MomentoUser");

            SaveImage(single.Id, MediaType.SmallImage, _settings.ImageSmallWidth, _settings.ImageSmallHeight, format, message);
            SaveImage(single.Id, MediaType.MediumImage, _settings.ImageMediumWidth, _settings.ImageMediumHeight, format, message);
            SaveImage(single.Id, MediaType.LargeImage, _settings.ImageLargeWidth, _settings.ImageLargeHeight, format, message);
            SaveImage(single.Id, MediaType.OriginalImage, int.MaxValue, int.MaxValue, format, message);

            _session.Database.ConnectionString = _settings.QueueDatabase;
            _session.Database.CommandType = CommandType.Text;
            
            _session.Database.NonQuery(string.Format("Delete From Media Where Id = '{0}'", message.Id));

            //Reset to momntz database
            _session.Database.ConnectionString = null;
        }

        private void SaveImage(int momentoId, MediaType mediaType, int maxWidth, int maxHeight, ImageFormat format, MediaItem message)
        {
            byte[] bytes = null;

            if (maxWidth < int.MaxValue && maxHeight < int.MaxValue)
            {
                bytes = message.Bytes.ResizeToMax(new Size(maxWidth, maxHeight), format);

            }

            string type = message.Extension.TrimStart('.');
            string name = string.Format("{0}_{1}{2}", Path.GetFileNameWithoutExtension(message.Filename), DateTime.Now.Ticks, message.Extension);

            AddToStorage("img", "image", name, type, bytes ?? message.Bytes);
            Save(momentoId, name, mediaType, message);
        }

        private class Format
        {
            public ImageFormat ImageFormat { get; set; }

            public string[] Extensions { get; set; }
        }
    }
}
