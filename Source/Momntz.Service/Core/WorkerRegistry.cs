using ChuckConway.Cloud.Queue;
using ChuckConway.Cloud.Storage;
using Momntz.Core.Contants;
using Momntz.Infrastructure.Configuration;
using Momntz.Infrastructure.Instrumentation.Logging;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace Momntz.Service.Core
{
    public class WorkerRegistry : Registry
    {
        public WorkerRegistry()
        {
            var settings = MomntzConfiguration.GetSettings();

            settings.LoggerType = LoggingConstants.Cloud;
            settings.ServiceLoggingEndpoint = "https://logs.loggly.com/inputs/d7aa4078-bbe6-400e-958e-4fb08497f2de";

            For<ISessionFactory>().Use(new Database().CreateSessionFactory());
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
    }
}
