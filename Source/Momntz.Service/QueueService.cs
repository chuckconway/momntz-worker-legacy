using System.Collections.Generic;
using ChuckConway.Cloud.Queue;
using ChuckConway.Cloud.Storage;
using Momntz.Core;
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
        private List<ISaga> _processors;
        private ISettings _settings;

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
            IQueue queue = new AzureQueue();
            foreach (var plugin in plugins)
            {
                Plugin plugin1 = plugin;
                queue.ProcessAllMessages(plugin.Queue, s => plugin1.Saga.Consume(s));
            }


            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
        }

        /// <summary>
        /// Dependencies the injection.
        /// </summary>
        private IInjection DependencyInjection()
        {
            IInjection injection = new StructureMapInjection();
            injection.AddManifest(new WorkerRegistry());
            injection.AddManifest(new MomntzRegistry());
            _settings = injection.Get<ISettings>();

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
