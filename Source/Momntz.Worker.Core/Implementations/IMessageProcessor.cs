namespace Momntz.Worker.Core.Implementations
{
    public interface IMessageProcessor
    {
        string MessageType { get; }

        void Process(string message);
    }
}
