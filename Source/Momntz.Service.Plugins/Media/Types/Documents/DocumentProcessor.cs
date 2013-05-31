using System;

namespace Momntz.Service.Plugins.Media.Types.Documents
{
    public class DocumentProcessor : IMedia
    {
        /// <summary>
        /// Gets the media.
        /// </summary>
        /// <value>The media.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Consume(MediaMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
