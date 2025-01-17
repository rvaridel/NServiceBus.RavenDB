﻿namespace NServiceBus.Persistence.RavenDB
{
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Outbox;
    using NServiceBus.Persistence;
    using NServiceBus.Transport;

    class RavenDBSynchronizedStorageAdapter : ISynchronizedStorageAdapter
    {
        static readonly Task<CompletableSynchronizedStorageSession> EmptyResult = Task.FromResult((CompletableSynchronizedStorageSession) null);

        public Task<CompletableSynchronizedStorageSession> TryAdapt(OutboxTransaction transaction, ContextBag context)
        {
            // Since RavenDB doesn't support System.Transactions (or have transactions), there's no way to adapt anything out of the Outbox transaction.
            // Everything about the Raven session is controlled by OpenAsyncSessionBehavior.
            return EmptyResult;
        }

        public Task<CompletableSynchronizedStorageSession> TryAdapt(TransportTransaction transportTransaction, ContextBag context)
        {
            // Since RavenDB doesn't support System.Transactions (or have transactions), there's no way to adapt anything out of the transport transaction.
            // Everything about the Raven session is controlled by OpenAsyncSessionBehavior.
            return EmptyResult;
        }
    }
}