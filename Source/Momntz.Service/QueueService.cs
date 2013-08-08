using System.Collections.Generic;
using ChuckConway.Cloud.Queue;
using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure;
using Momntz.Infrastructure.Configuration;
using Momntz.Infrastructure.Instrumentation.Logging;
using Momntz.Messaging;
using Momntz.Service.Core;
using Momntz.Service.Plugins.Logging;

namespace Momntz.Service
{
    public class QueueService
    {
        private readonly List<Plugin> _plugins;
        private readonly IInjection _injection;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueService"/> class.
        /// </summary>
        public QueueService()
        {
            _injection = DependencyInjection();
            _plugins = new List<Plugin>
                {
                    //new Plugin { Queue = QueueConstants.MediaQueue, Saga = new MediaSaga(_injection.Get<IStorage>(), _injection.Get<ISettings>(), _injection.Get<ISessionFactory>())},
                    new Plugin { Queue = QueueConstants.LoggingQueue, Saga = new LoggerSaga(_injection.Get<IStorage>())},
                };
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public void Process()
        {
            CheckQueues();
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        /// <summary>
        /// Checks the queues.
        /// </summary>
        private void CheckQueues()
        {
            var queue = _injection.Get<IQueue>();
            queue.OnException += queue_OnException;

            foreach (var plugin in _plugins)
            {
                Plugin plugin2 = plugin;
                //Task.Factory.StartNew(() => queue.ProcessAllMessages<string>(plugin2.Queue, s => plugin2.Saga.Consume(s)));
                queue.ProcessAllMessages(plugin2.Queue, s => plugin2.Saga.Consume(s));
            }
        }

        /// <summary>
        /// Queue_s the on exception.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="s">The s.</param>
        public void queue_OnException(System.Exception e, string s)
        {
            var log =_injection.Get<ILog>();
            log.Exception(e, s);
        }

        /// <summary>
        /// Dependencies the injection.
        /// </summary>
        private static IInjection DependencyInjection()
        {
            IInjection injection = new StructureMapInjection();
            injection.AddManifest(new WorkerRegistry());
            injection.AddManifest(new MomntzRegistry());
            injection.Get<ISettings>();

            return injection;
        }


        /// <summary>
        /// Class Plugin
        /// </summary>
        private class Plugin
        {
            /// <summary>
            /// Gets or sets the queue.
            /// </summary>
            /// <value>The queue.</value>
            public string Queue { get; set; }

            /// <summary>
            /// Gets or sets the saga.
            /// </summary>
            /// <value>The saga.</value>
            public ISaga Saga { get; set; }
        }
    }
}
