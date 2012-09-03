using System;
using System.Threading;
using Momntz.Worker.Core;

namespace Momntz.Worker.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                QueueService service = new QueueService();
                service.Process();

                Thread.Sleep(30000);
            }
        }
    }
}
