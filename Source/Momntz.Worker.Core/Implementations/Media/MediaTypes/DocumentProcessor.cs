using System;

using Momntz.Core;
using Momntz.Infrastructure;


namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public class DocumentProcessor : IMedia
    {
        private readonly IStorage _storage;
        private readonly IDatabaseConfiguration _databaseConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentProcessor"/> class.
        /// </summary>
        /// <param name="storage">The storage.</param>
        /// <param name="databaseConfiguration">The database configuration.</param>
        public DocumentProcessor(IStorage storage, IDatabaseConfiguration databaseConfiguration)
        {
            _storage = storage;
            _databaseConfiguration = databaseConfiguration;
        }

        /// <summary>
        /// Gets the media.
        /// </summary>
        /// <value>The media.</value>
        public string Media
        {
            get { return "Document"; }
        }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        public void Process(Model.QueueData.Media message)
        {
            throw new NotImplementedException();
        }
    }
}
