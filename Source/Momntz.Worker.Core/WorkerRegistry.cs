using Momntz.Core;
using Momntz.Infrastructure;
using Momntz.Model.Configuration;
using Momntz.Worker.Core.Implementations;
using NHibernate;
using StructureMap.Configuration.DSL;

namespace Momntz.Worker.Core
{
    public class WorkerRegistry : Registry
    {
        public WorkerRegistry()
        {
            For<IDatabaseConfiguration>().Use(new DatabaseConfiguration());
            For<ISession>().Use(new DatabaseConfiguration().CreateSessionFactory().OpenSession());
            For<IStorage>().Use<AzureStorage>();
            For<IConfigurationService>().Use<MomntzConfiguration>();
            For<ISettings>().Use<Settings>();
           

            Scan(
                s =>
                    {
                        s.AddAllTypesOf<IMessageProcessor>();
                    s.TheCallingAssembly();
                    s.WithDefaultConventions();
                });
        }
    }
}
