using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using Hypersonic;


namespace Momntz.Worker.Core.Implementations.Media.MediaTypes.Image
{
    public class ExifData
    {
        private readonly IDatabase _database;

        public ExifData(IDatabase database)
        {
            _database = database;
        }

        /// <summary>
        /// Extracts the exif save.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="mediaId">The media id.</param>
        public  DateTime ExtractExifSave(Stream stream, int momentoId)
        {
            DateTime date = DateTime.MinValue;

            using (stream)
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(stream);
                PropertyItem[] items = image.PropertyItems;

                if (items.Length > 0)
                {
                    DataTable table = GetTable();

                    foreach (PropertyItem propertyItem in items)
                    {
                        //The below codes output funky strings that won't go into the database.
                        string[] ignore = { "501b", "5091", "5090", "927c" };
                        string id = propertyItem.Id.ToString("x");
                        string value = Encoding.UTF8.GetString(propertyItem.Value);

                        if (!ignore.Contains(id))
                        {
                            DataRow dataRow = table.NewRow();
                            dataRow["MomentoId"] = momentoId;
                            dataRow["Key"] = propertyItem.Id.ToString("x");
                            dataRow["Type"] = propertyItem.Type;
                            dataRow["Value"] = string.IsNullOrWhiteSpace(value) ? string.Empty : value;
                            table.Rows.Add(dataRow);
                        }

                        date = GetDate(id, value, date);
                    }

                    try
                    {
                        _database.NonQuery("[dbo].[Exif_InsertExif]", new { ExifCollection = table });
                    }
                    catch //This needs to be logged... TODO:Log this! Maybe we can tap into ELMAH
                    {

                    }
                }
            }

            return date;
        }

        /// <summary>
        /// Gets the date.
        /// </summary>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        private static DateTime GetDate(string id, string value, DateTime date)
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
        /// Gets the table.
        /// </summary>
        /// <returns></returns>
        private static DataTable GetTable()
        {
            DataTable table = new DataTable();
            table.Columns.Add("MomentoId", typeof(int));
            table.Columns.Add("Key", typeof(string));
            table.Columns.Add("Type", typeof(int));
            table.Columns.Add("Value", typeof(string));
            return table;
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
