namespace Momntz.Worker.Core.Implementations.Media.MediaTypes
{
    public interface IMedia
    {
        /// <summary>
        /// Gets the media.
        /// </summary>
        /// <value>The media.</value>
        string Media { get; }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Process(Momntz.Model.QueueData.Media message);
    }
}
