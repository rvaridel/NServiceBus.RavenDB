﻿namespace NServiceBus.RavenDB.AcceptanceTests
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTests;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTesting.Customization;
    using NServiceBus.ObjectBuilder;

    public class When_providing_a_custom_document_store : NServiceBusAcceptanceTest
    {

        [Test]
        [Ignore("Just documents the incorrect behaviour for now")]
        public void Should_not_resolve_until_start()
        {
            var endpointConfiguration = new EndpointConfiguration("custom-docstore-endpoint");

            var typesToInclude = new List<Type> { typeof(MySaga) };

            //need to include the NServiceBus.RavenDB types since the features need to be discovered
            typesToInclude.AddRange(typeof(RavenDBPersistence).Assembly.GetTypes());

            endpointConfiguration.TypesToIncludeInScan(typesToInclude);
            endpointConfiguration.UseTransport<AcceptanceTestingTransport>();
            endpointConfiguration.EnableOutbox();

            endpointConfiguration.UsePersistence<RavenDBPersistence>()
                    .SetDefaultDocumentStore(_ =>
                    {
                        Assert.Fail("Document store resolved to early");

                        return null;
                    });


            EndpointWithExternallyManagedContainer.Create(endpointConfiguration, new FakeContainerRegistration());
        }

        class MySaga : Saga<MySaga.SagaData>, IAmStartedByMessages<MyMessage>
        {
            public Task Handle(MyMessage message, IMessageHandlerContext context)
            {
                throw new NotImplementedException();
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper)
            {
                mapper.ConfigureMapping<MyMessage>(m => m.SomeId)
                    .ToSaga(s => s.SomeId);
            }

            public class SagaData : ContainSagaData
            {
                public string SomeId { get; set; }
            }
        }

        class MyMessage : IMessage
        {
            public string SomeId { get; set; }
        }

        class FakeContainerRegistration : IConfigureComponents
        {
            public void ConfigureComponent(Type concreteComponent, DependencyLifecycle dependencyLifecycle)
            {

            }

            public void ConfigureComponent<T>(DependencyLifecycle dependencyLifecycle)
            {

            }

            public void ConfigureComponent<T>(Func<T> componentFactory, DependencyLifecycle dependencyLifecycle)
            {

            }

            public void ConfigureComponent<T>(Func<IBuilder, T> componentFactory, DependencyLifecycle dependencyLifecycle)
            {

            }

            public bool HasComponent<T>()
            {
                return false;
            }

            public bool HasComponent(Type componentType)
            {
                return false;
            }

            public void RegisterSingleton(Type lookupType, object instance)
            {

            }

            public void RegisterSingleton<T>(T instance)
            {
            }
        }
    }
}