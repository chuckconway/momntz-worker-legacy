using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure.Configuration;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace Momntz.Service.Core
{
    public class WorkerRegistry : Registry
    {
        public WorkerRegistry()
        {
            var settings = MomntzConfiguration.GetStorageSettings();

            For<ISessionFactory>().Use(new Database().CreateSessionFactory());
            For<ISession>().HybridHttpOrThreadLocalScoped().Use(() => new Database().CreateSessionFactory().OpenSession());
            For<IStorage>().Use<AzureStorage>()
                 .Ctor<string>("cloudUrl")
                 .Is(settings.CloudUrl)
                 .Ctor<string>("cloudAccount")
                 .Is(settings.CloudAccount)
                 .Ctor<string>("cloudKey")
                 .Is(settings.CloudKey);

            For<IConfigurationService>().Use<MomntzConfiguration>();
            For<ISettings>().Use<Settings>();
        }
    }
}
