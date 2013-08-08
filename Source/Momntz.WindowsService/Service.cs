using System.ServiceProcess;
using System.Threading;
using Momntz.Service;

namespace Momntz.WindowsService
{
    public partial class Service : ServiceBase
    {
        SingleThreadQueueService service = new SingleThreadQueueService(60000);

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        public Service()
        {
            InitializeComponent();

            ServiceName = "Momntz Service";
            CanHandlePowerEvent = true;
            CanHandleSessionChangeEvent = true;
            CanPauseAndContinue = true;
            CanShutdown = true;
            CanStop = true;
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the service by the Service Control Manager (SCM) or when the operating system starts (for a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            service.Start();
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service stops running.
        /// </summary>
        protected override void OnStop()
        {
            service.Stop();
        }

        public class SingleThreadQueueService
        {
            private readonly int _elaspeTime;
            private volatile bool _stop;

            /// <summary>
            /// Initializes a new instance of the <see cref="SingleThreadQueueService"/> class.
            /// </summary>
            /// <param name="elaspeTime">The elaspe time.</param>
            public SingleThreadQueueService(int elaspeTime)
            {
                _elaspeTime = elaspeTime;
            }

            /// <summary>
            /// Starts this instance.
            /// </summary>
            public void Start()
            {
                var thread = new Thread(() =>
                {
                    while (!_stop)
                    {
                        new QueueService().Process();
                        Thread.Sleep(_elaspeTime);
                    }
                });

                thread.Start();
            }

            /// <summary>
            /// Stops this instance.
            /// </summary>
            public void Stop()
            {
                _stop = true;
            }
        }
    }
}
