using Momntz.Data.Commands.Queue;

namespace Momntz.Worker.Core
{
    public class Queue
    {
        public int Id { get; set; }

        public string Implementation { get; set; }

        public string Payload { get; set; }

        public MessageStatus MessageStatus { get; set; }
    }
}
