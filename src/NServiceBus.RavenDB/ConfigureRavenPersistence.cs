namespace NServiceBus.RavenDB
{
    using System;
    using NServiceBus;
    using Persistence;
    using Persistence.SagaPersister;
    using Persistence.SubscriptionStorage;
    using Persistence.TimeoutPersister;
    using Settings;
    using Raven.Client;
    using Raven.Client.Document;
    using RavenLogManager = Raven.Abstractions.Logging.LogManager;

    /// <summary>
    /// Extension methods to configure RavenDB persister.
    /// </summary>
    public static class ConfigureRavenPersistence
    {
        public static Configure PersistenceForTimeouts(this Configure config, IDocumentStore documentStore)
        {
            SetupRavenPersistence(config, documentStore);

            config.Configurer.ConfigureComponent<RavenTimeoutPersistence>(DependencyLifecycle.SingleInstance);

            return config;
        }

        // TODO here would be the place to wire up the ISagaFinder extension point
        public static Configure PersistenceForSagas(this Configure config, IDocumentStore documentStore)
        {
            SetupRavenPersistence(config, documentStore);

            config.Configurer.ConfigureComponent<RavenSagaPersister>(DependencyLifecycle.InstancePerCall);

            return config;
        }

        public static Configure PersistenceForSubscriptions(this Configure config, IDocumentStore documentStore)
        {
            SetupRavenPersistence(config, documentStore);

            config.Configurer.ConfigureComponent<RavenSubscriptionStorage>(DependencyLifecycle.SingleInstance);

            return config;
        }

        public static Configure PersistenceForAll(this Configure config, IDocumentStore documentStore)
        {
            PersistenceForTimeouts(config, documentStore);
            PersistenceForSubscriptions(config, documentStore);
            PersistenceForSagas(config, documentStore);
            return config;
        }

        private static void SetupRavenPersistence(Configure config, IDocumentStore documentStore)
        {
            if (!config.Configurer.HasComponent<IDocumentStore>())
            {
                config.Configurer.ConfigureComponent(() => documentStore, DependencyLifecycle.SingleInstance);
                config.Configurer.ConfigureComponent<RavenSessionFactory>(DependencyLifecycle.SingleInstance);
                config.Configurer.ConfigureComponent<RavenUnitOfWork>(DependencyLifecycle.InstancePerUnitOfWork);
                RavenUserInstaller.RunInstaller = true; // TODO this smells
            }
            else
            {
                // TODO if this is not acceptable, we are going to need a type to docStore mapping set up
                var configuredStore = config.Builder.Build<IDocumentStore>();
                if (configuredStore != documentStore)
                    throw new Exception("You can only point to one RavenDB document store for all persisted types");
            }
        }

        /// <summary>
        /// Specifies the mapping to use for when resolving the database name to use for each message.
        /// </summary>
        /// <param name="config">The configuration object.</param>
        /// <param name="convention">The method referenced by a Func delegate for finding the database name for the specified message.</param>
        /// <returns>The configuration object.</returns>
        public static Configure RavenDBStorageMessageToDatabaseMappingConvention(this Configure config, Func<IMessageContext, string> convention)
        {
            RavenSessionFactory.GetDatabaseName = convention;

            return config;
        }


        /// <summary>
        /// Apply the NServiceBus conventions to a <see cref="DocumentStore"/> .
        /// </summary>
        public static Configure ApplyRavenDBConventions(this Configure config, DocumentStore documentStore)
        {
            if (documentStore.Url == null)
            {
                documentStore.Url = RavenPersistenceConstants.DefaultUrl;
            }
            if (documentStore.DefaultDatabase == null)
            {
                documentStore.DefaultDatabase = Configure.EndpointName;
            }
            documentStore.ResourceManagerId = RavenPersistenceConstants.DefaultResourceManagerId;


            if (SettingsHolder.Get<bool>("Transactions.SuppressDistributedTransactions"))
            {
                documentStore.EnlistInDistributedTransactions = false;
            }
            RavenLogManager.CurrentLogManager = new NoOpLogManager();

            return config;
        }
    }
}