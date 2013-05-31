using System.Drawing.Imaging;

namespace Momntz.Service.Plugins.Media.Types.Images
{
    public class ImageType
    {
        /// <summary>
        /// Gets or sets the format.
        /// </summary>
        /// <value>The format.</value>
        public ImageFormat Format { get; set; }

        /// <summary>
        /// Gets or sets the extensions.
        /// </summary>
        /// <value>The extensions.</value>
        public string[] Extensions { get; set; }
    }
}
