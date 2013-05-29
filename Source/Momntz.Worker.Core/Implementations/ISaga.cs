namespace Momntz.Worker.Core.Implementations
{
    public interface ISaga<in T> where T :class , new() 
    {
        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        string Type { get; }

        /// <summary>
        /// Processes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        void Process(T message);
    }
}
