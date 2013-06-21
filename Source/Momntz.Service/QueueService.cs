using System.Collections.Generic;
using System.Threading.Tasks;
using System.Timers;
using ChuckConway.Cloud.Queue;
using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure;
using Momntz.Infrastructure.Configuration;
using Momntz.Messaging;
using Momntz.Service.Core;
using Momntz.Service.Plugins.Media;
using NHibernate;

namespace Momntz.Service
{
    public class QueueService
    {
        private List<Plugin> plugins;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueueService"/> class.
        /// </summary>
        public QueueService()
        {
            var injection = DependencyInjection();

            plugins = new List<Plugin>
                {
                    new Plugin { Queue = QueueConstants.MediaQueue, Saga = new MediaSaga(injection.Get<IStorage>(), injection.Get<ISettings>(), injection.Get<ISessionFactory>())},
                };
        }

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public void Process()
        {
            CheckQueues();

            //Timer timer = new Timer(30000);
            //timer.Elapsed += timer_Elapsed;

            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckQueues();
        }

        private void CheckQueues()
        {
            IQueue queue = new AzureQueue();
            queue.OnException += queue_OnException;

            foreach (var plugin in plugins)
            {
                Plugin plugin2 = plugin;
                //Task.Factory.StartNew(() => queue.ProcessAllMessages<string>(plugin2.Queue, s => plugin2.Saga.Consume(s)));
                queue.ProcessAllMessages<string>(plugin2.Queue, s => plugin2.Saga.Consume(s));
            }
        }

        void queue_OnException(System.Exception e, string s)
        {
            string chuck = string.Empty;
        }

        /// <summary>
        /// Dependencies the injection.
        /// </summary>
        private IInjection DependencyInjection()
        {
            IInjection injection = new StructureMapInjection();
            injection.AddManifest(new WorkerRegistry());
            injection.AddManifest(new MomntzRegistry());
            injection.Get<ISettings>();

            return injection;
        }

        ///// <summary>
        ///// Gets the message processors.
        ///// </summary>
        ///// <param name="injection">The injection.</param>
        ///// <returns>List{IMessageProcessor}.</returns>
        //private List<IMessageProcessor> GetMessageProcessors(IInjection injection)
        //{
        //    var types = GetType().Assembly.GetTypes();

        //    var messages = (from type in types 
        //                    where typeof (IMessageProcessor).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
        //                    select injection.GetInstances<IMessageProcessor>().Single(x => x.GetType()== type)).ToList();

        //    _processors = messages;
        //    return messages;
        //}

        private class Plugin
        {
            public string Queue { get; set; }

            public ISaga Saga { get; set; }
        }
    }
}
