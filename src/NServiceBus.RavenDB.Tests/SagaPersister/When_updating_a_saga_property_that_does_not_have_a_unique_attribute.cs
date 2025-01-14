using System;
using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Persistence.RavenDB;
using NServiceBus.RavenDB.Tests;
using NUnit.Framework;
using Raven.Client.Documents.Session;

[TestFixture]
public class When_updating_a_saga_property_that_does_not_have_a_unique_attribute : RavenDBPersistenceTestBase
{
    [Test]
    public async Task It_should_persist_successfully()
    {
        IAsyncDocumentSession session;
        var options = this.CreateContextWithAsyncSessionPresent(out session);
        var persister = new SagaPersister();
        var uniqueString = Guid.NewGuid().ToString();

        var saga1 = new SagaData
        {
            Id = Guid.NewGuid(),
            UniqueString = uniqueString,
            NonUniqueString = "notUnique"
        };

        var synchronizedSession = new RavenDBSynchronizedStorageSession(session);

        await persister.Save(saga1, this.CreateMetadata<SomeSaga>(saga1), synchronizedSession, options);
        await session.SaveChangesAsync().ConfigureAwait(false);

        var saga = await persister.Get<SagaData>(saga1.Id, synchronizedSession, options);
        saga.NonUniqueString = "notUnique2";
        await persister.Update(saga, synchronizedSession, options);
        await session.SaveChangesAsync().ConfigureAwait(false);
    }

    class SomeSaga : Saga<SagaData>, IAmStartedByMessages<StartSaga>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper)
        {
            mapper.ConfigureMapping<StartSaga>(m => m.UniqueString).ToSaga(s => s.UniqueString);
        }

        public Task Handle(StartSaga message, IMessageHandlerContext context)
        {
            return Task.CompletedTask;
        }
    }

    class SagaData : IContainSagaData
    {
// ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string UniqueString { get; set; }

// ReSharper disable once UnusedAutoPropertyAccessor.Local
        public string NonUniqueString { get; set; }
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
    }
}