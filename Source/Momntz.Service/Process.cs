using System;
using System.Collections.Concurrent;
using System.ServiceProcess;
using Nancy.Hosting.Self;

namespace Momntz.Service
{
    public partial class Process : ServiceBase
    {
        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        

        public Process()
        {
            InitializeComponent();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {

        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {

        }
    }
}
