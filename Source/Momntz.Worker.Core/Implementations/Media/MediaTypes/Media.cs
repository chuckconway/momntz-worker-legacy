using System;

namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public class MediaItem 
    {
        public Guid Id { get; set; }

        public string Filename { get; set; }

        public string Extension { get; set; }

        public int Size { get; set; }

        public string Username { get; set; }

        public string ContentType { get; set; }

        public string MediaType { get; set; }

        public byte[] Bytes { get; set; }

        /// <summary>
        /// Perform a deep Copy of the object.
        /// </summary>
        /// <typeparam name="T">The type of object being copied.</typeparam>
        /// <param name="source">The object instance to copy.</param>
        /// <returns>The copied object.</returns>
        public static MediaItem Clone(MediaItem source)
        {
            return new MediaItem
                                 {
                                     Id = source.Id,
                                     Filename = source.Filename,
                                     Extension = source.Extension,
                                     Size = source.Size,
                                     Username = source.Username,
                                     ContentType = source.ContentType,
                                     MediaType = source.MediaType,
                                     Bytes = source.Bytes
                                 };
        }
    }
}
