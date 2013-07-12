using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Momntz.Data.Schema;

using NHibernate;

namespace Momntz.Service.Plugins.Media.Types.Images
{
    public class ExifData
    {
        private readonly ISession _session;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExifData"/> class.
        /// </summary>
        /// <param name="session">The session.</param>
        public ExifData(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Extracts the exif save.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="momento">The momento.</param>
        /// <returns>DateTime.</returns>
        public  DateTime? ExtractExifSave(Stream stream, Momento momento)
        {
            DateTime? date = null;

            using (stream)
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                PropertyItem[] items = image.PropertyItems;

                if (items.Length > 0)
                {
                    var exifs = new List<Exif>();

                    foreach (PropertyItem propertyItem in items)
                    {
                        //The below codes output funky strings that won't go into the database.
                        string[] ignore = { "501b", "5091", "5090", "927c" };
                        string id = propertyItem.Id.ToString("x");
                        string value = Encoding.UTF8.GetString(propertyItem.Value);

                        if (!ignore.Contains(id))
                        {
                            var exif = new Exif
                                {
                                    Momento = momento,
                                    Key = propertyItem.Id.ToString("x"),
                                    Type = propertyItem.Type,
                                    Value = string.IsNullOrWhiteSpace(value) ? string.Empty : value
                                };

                            exifs.Add(exif);
                        }

                        date = GetDate(id, value, date);
                    }

                        SaveExifs(exifs);

                }
            }

            return date;
        }

        /// <summary>
        /// Saves the exifs.
        /// </summary>
        /// <param name="exifs">The exifs.</param>
        private void SaveExifs(IEnumerable<Exif> exifs)
        {
            using (var trans = _session.BeginTransaction())
            {
                foreach (var exif in exifs)
                {
                    _session.Save(exif);
                }

                trans.Commit();
            }
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private static DateTime? GetDate(string id, string value, DateTime? date)
        {
            if (id == "9003" && !string.IsNullOrEmpty(value))
            {
                try
                {
                    DateTime? taken = DateTaken(value);
                    date = (taken.HasValue ? taken.GetValueOrDefault() : DateTime.MinValue);
                }
                catch // can not fixed f'ed up dates
                {

                }
            }
            return date;
        }

        /// <summary>
        /// Returns the EXIF Image Data of the Date Taken.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>Date Taken or Null if Unavailable</returns>
        public static DateTime? DateTaken(string value)
        {
            string dateTakenTag = value;
            string[] parts = dateTakenTag.Split(':', ' ');
            int year = int.Parse(parts[0]);
            int month = int.Parse(parts[1]);
            int day = int.Parse(parts[2]);
            int hour = int.Parse(parts[3]);
            int minute = int.Parse(parts[4]);
            int second = int.Parse(parts[5]);

            return new DateTime(year, month, day, hour, minute, second);
        }

    }
}
