﻿namespace NServiceBus.RavenDB.Tests
{
    using NServiceBus.Persistence.RavenDB;
    using Raven.Client.Documents;
    using Raven.Client.Documents.Session;

    class RavenAsyncSessionFactory : IAsyncSessionProvider
    {
        IAsyncDocumentSession session;
        readonly IDocumentStore store;

        public RavenAsyncSessionFactory(IDocumentStore store)
        {
            session = null;
            this.store = store;
        }

        public IAsyncDocumentSession AsyncSession => session ?? (session = OpenAsyncSession());

        IAsyncDocumentSession OpenAsyncSession()
        {
            var documentSession = store.OpenAsyncSession();
            return documentSession;
        }

        public void ReleaseSession()
        {
            if (session == null)
                return;

            session.Dispose();
            session = null;
        }

        public void SaveChanges()
        {
            session?.SaveChangesAsync()
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }
    }
}
