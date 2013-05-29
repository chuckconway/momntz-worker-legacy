namespace Momntz.Service
{
    public interface ISaga<in T> : ISaga where T :class , new() 
    {
        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Consume(T message);
    }
}
