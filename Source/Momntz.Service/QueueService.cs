using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.WindowsAzure;
using Momntz.Core;
using Momntz.Data.Commands.Queue;
using Momntz.Infrastructure;
using Momntz.Model.Configuration;
using Momntz.Model.QueueData;
using Momntz.Worker.Core;

using Microsoft.ServiceBus;

namespace Momntz.Service
{
    public class QueueService
    {
        private List<ISaga> _processors;
        private ISettings _settings;
        private IDatabaseConfiguration _databaseConfiguration;

        /// <summary>
        /// Processes this instance.
        /// </summary>
        public void Process()
        {
            IInjection injection = new StructureMapInjection();
            injection.AddManifest(new WorkerRegistry());
            injection.AddManifest(new MomntzRegistry());
            _databaseConfiguration = injection.Get<IDatabaseConfiguration>();
            _settings = injection.Get<ISettings>();
            //HibernatingRhinos.Profiler.Appender.NHibernate.NHibernateProfiler.Initialize();
            var list = RetrieveQueuedItems();
            ProcessQueuedItems(list, injection);
        }


        /// <summary>
        /// Retrieves the queued items.
        /// </summary>
        /// <returns>IEnumerable{Queue}.</returns>
        private IEnumerable<Queue> RetrieveQueuedItems()
        {
            IList<Queue> queues;

            // Create the topic if it does not exist already
            string connectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            var namespaceManager = NamespaceManager.CreateFromConnectionString(connectionString);

            if (!namespaceManager.TopicExists("TestTopic"))
            {
                namespaceManager.CreateTopic("TestTopic");
            }

            return queues;
        }

        /// <summary>
        /// Updates the queue.
        /// </summary>
        /// <param name="item">The item.</param>
        public void UpdateQueue(Queue item)
        {
            //using (var session = _databaseConfiguration.CreateSessionFactory(_settings.QueueDatabase).OpenSession())
            //using (var tran = session.BeginTransaction())
            //{
            //    session.Save(item);

            //    tran.Commit();
            //}
        }

        /// <summary>
        /// Processes the queued items.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="injection">The injection.</param>
        private void ProcessQueuedItems(IEnumerable<Queue> items, IInjection injection)
        {
            var messages = _processors ?? GetMessageProcessors(injection);

            foreach (var queue in items)
            {
                foreach (var message in messages.Where(message => message.MessageType == queue.Implementation))
                {
                    try
                    {
                        queue.MessageStatus = MessageStatus.Processing;
                        UpdateQueue(queue);

                        message.Process(queue.Payload);

                        queue.MessageStatus = MessageStatus.Completed;
                        UpdateQueue(queue);
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace);

                        queue.MessageStatus = MessageStatus.Error;
                        queue.Error = error;
                        UpdateQueue(queue);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// Gets the message processors.
        /// </summary>
        /// <param name="injection">The injection.</param>
        /// <returns>List{IMessageProcessor}.</returns>
        private List<IMessageProcessor> GetMessageProcessors(IInjection injection)
        {
            var types = GetType().Assembly.GetTypes();

            var messages = (from type in types 
                            where typeof (IMessageProcessor).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                            select injection.GetInstances<IMessageProcessor>().Single(x => x.GetType()== type)).ToList();

            _processors = messages;
            return messages;
        }
    }
}
