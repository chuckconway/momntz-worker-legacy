﻿using System;
using Momntz.Service.Core.IOC;

namespace Momntz.Worker.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            Server server = new Server();
            server.Start();

            Console.ReadLine();
            
        }
    }
}
