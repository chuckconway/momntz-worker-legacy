using ChuckConway.Cloud.Queue;
using ChuckConway.Cloud.Storage;
using Momntz.Core.Contants;
using Momntz.Infrastructure.Configuration;
using Momntz.Infrastructure.Instrumentation.Logging;
using Momntz.Infrastructure.Processors;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace Momntz.Service.Core
{
    public class WorkerRegistry : Registry
    {
        public WorkerRegistry()
        {
            var settings = MomntzConfiguration.GetSettings();
            SetLogging(settings);

            For<ISessionFactory>().Use(new Database().CreateSessionFactory());
            For<IProjectionProcessor>().Use<ProjectionProcessor>();
            For<ICommandProcessor>().Use<CommandProcessor>();
            For<ApplicationSettings>().Use(settings);

            For<ISession>().HybridHttpOrThreadLocalScoped().Use(() => new Database().CreateSessionFactory().OpenSession());
            For<IStorage>().Use<AzureStorage>()
                 .Ctor<string>("cloudUrl")
                 .Is(settings.CloudUrl)
                 .Ctor<string>("cloudAccount")
                 .Is(settings.CloudAccount)
                 .Ctor<string>("cloudKey")
                 .Is(settings.CloudKey);

            For<IQueue>().Use<AzureQueue>()
                .Ctor<string>("connectionString")
                .Is(settings.ServiceBusEndpoint);

            For<ILog>().Use<Log>();

            For<IConfigurationService>().Use<MomntzConfiguration>();
            For<ISettings>().Use<Settings>();
        }

        /// <summary>
        /// Sets the logging.
        /// </summary>
        /// <param name="settings">The settings.</param>
        private static void SetLogging(ApplicationSettings settings)
        {
            settings.LoggerType = LoggingConstants.Cloud;
            settings.RestLoggingEndpoint = settings.ServiceLoggingEndpoint;
        }
    }
}
