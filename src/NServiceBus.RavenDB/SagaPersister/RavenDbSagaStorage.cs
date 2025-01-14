﻿namespace NServiceBus.Persistence.RavenDB
{
    using System;
    using System.Linq;
    using NServiceBus.Features;
    using NServiceBus.Sagas;

    class RavenDbSagaStorage : Feature
    {
        internal RavenDbSagaStorage()
        {
            DependsOn<Sagas>();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent<SagaPersister>(DependencyLifecycle.SingleInstance);

            if (context.Settings.TryGet(out SagaMetadataCollection allSagas))
            {
                var customFinders = allSagas.SelectMany(sagaMetadata => sagaMetadata.Finders)
                    .Where(finder => finder.Properties.ContainsKey("custom-finder-clr-type"))
                    .ToArray();

                if (customFinders.Any())
                {
                    var msg = "RavenDB Persistence does not support custom saga finders using the `IFindSagas<TSagaData>` interface. The following custom finders are invalid:";
                    foreach (var finder in customFinders)
                    {
                        msg += $"{Environment.NewLine}  * {finder.Type.FullName}";
                    }
                    throw new Exception(msg);
                }
            }
        }
    }
}