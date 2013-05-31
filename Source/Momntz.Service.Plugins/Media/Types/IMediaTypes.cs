namespace Momntz.Service.Plugins.Media.Types
{
    public interface IMedia
    {
        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Consume(MediaMessage message);
    }
}
