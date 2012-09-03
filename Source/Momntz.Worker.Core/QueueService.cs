using System;
using System.Collections.Generic;
using System.Linq;
using Hypersonic;
using Momntz.Data.Commands.Queue;
using Momntz.Infrastructure;
using Momntz.Model.Configuration;
using Momntz.Worker.Core.Implementations;

namespace Momntz.Worker.Core
{
    public class QueueService
    {
        private IList<IMessageProcessor> _processors;
        private ISettings _settings;
        private ISession _session;

        public void Process()
        {
            IInjection injection = new StructureMapInjection();
            injection.AddManifest(new WorkerRegistry());
            injection.AddManifest(new MomntzRegistry());
            _session = injection.Get<ISession>();
            _settings = injection.Get<ISettings>();

            var list = RetreiveQueuedItems();
            ProcessQueuedItems(list, injection);
        }

        private IEnumerable<Queue> RetreiveQueuedItems()
        {
            _session.Database.ConnectionString = _settings.QueueDatabase;
            var list = _session.Query<Queue>()
                .Where("MessageStatus = 'Queued'")
                .List();

            //Reset to the Momntz Database
            _session.Database.ConnectionString = null;

            return list;
        }

        public void UpdateQueue(Queue item, MessageStatus status)
        {
            _session.Database.ConnectionString = _settings.QueueDatabase;
            _session.SaveAnonymous<Queue>(new { MessageStatus = status }, q => q.Id == item.Id);

            //Reset to original Database
            _session.Database.ConnectionString = null;
        }

        public void AddError(string error, Queue item)
        {
            _session.Database.ConnectionString = _settings.QueueDatabase;
            _session.SaveAnonymous<Queue>(new { MessageStatus = MessageStatus.Error, Error = error }, q => q.Id == item.Id);

            //Reset to original Database
            _session.Database.ConnectionString = null;
        }

        private void ProcessQueuedItems(IEnumerable<Queue> items, IInjection injection)
        {
            var messages = _processors ?? GetMessageProcessors(injection);

            foreach (var queue in items)
            {
                foreach (var message in messages.Where(message => message.MessageType == queue.Implementation))
                {
                    try
                    {
                        UpdateQueue(queue, MessageStatus.Processing);
                        message.Process(queue.Payload);
                        UpdateQueue(queue, MessageStatus.Completed);
                    }
                    catch (Exception ex)
                    {
                        string error = String.Format("Message:{0}, StackTrace:{1}", ex.Message, ex.StackTrace);
                        AddError(error, queue);
                    }
                    break;
                }
            }
        }

        private List<IMessageProcessor> GetMessageProcessors(IInjection injection)
        {
            var types = GetType().Assembly.GetTypes();

            var messages = (from type in types 
                            where typeof (IMessageProcessor).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                            select (IMessageProcessor)injection.Get(type)).ToList();

            _processors = messages;
            return messages;
        }
    }
}
