using ChuckConway.Cloud.Storage;
using Momntz.Infrastructure;
using Momntz.Infrastructure.Configuration;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace Momntz.Service.Core
{
    public class WorkerRegistry : Registry
    {
        public WorkerRegistry()
        {
            For<ISessionFactory>().Use(Database.CreateSessionFactory());
            For<IStorage>().Use<AzureStorage>();
            For<IConfigurationService>().Use<MomntzConfiguration>();
            For<ISettings>().Use<Settings>();
        }
    }
}
