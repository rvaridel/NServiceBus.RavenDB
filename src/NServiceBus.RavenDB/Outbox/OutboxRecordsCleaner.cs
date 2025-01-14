﻿namespace NServiceBus.Persistence.RavenDB
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.RavenDB.Outbox;
    using Raven.Client.Documents;
    using Raven.Client.Documents.Operations;
    using Raven.Client.Documents.Queries;

    class OutboxRecordsCleaner
    {
        public OutboxRecordsCleaner(IDocumentStore documentStore)
        {
            this.documentStore = documentStore;
        }

        public async Task RemoveEntriesOlderThan(DateTime dateTime, CancellationToken cancellationToken = default(CancellationToken))
        {
            var options = new QueryOperationOptions { AllowStale = true };
            var deleteOp = new DeleteByQueryOperation<OutboxRecord, OutboxRecordsIndex>(record => record.Dispatched && record.DispatchedAt <= dateTime, options);

            var operation = await documentStore.Operations.SendAsync(deleteOp, token: cancellationToken).ConfigureAwait(false);

            // This is going to execute multiple "status check" requests to Raven, but this does
            // not currently support CancellationToken.
            await operation.WaitForCompletionAsync().ConfigureAwait(false);
        }

        IDocumentStore documentStore;
    }
}