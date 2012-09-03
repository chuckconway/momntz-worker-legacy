namespace Momntz.Worker.Core.Implementations.Email
{
    public class EmailProcessor : IMessageProcessor
    {
        public string MessageType
        {
            get { return typeof(EmailMessage).FullName; ; }
        }

        public void Process(string message)
        {
            
        }
    }
}
