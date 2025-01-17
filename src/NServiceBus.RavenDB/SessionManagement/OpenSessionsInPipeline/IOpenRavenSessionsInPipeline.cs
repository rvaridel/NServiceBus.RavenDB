﻿namespace NServiceBus.Persistence.RavenDB
{
    using System.Collections.Generic;
    using Raven.Client.Documents.Session;

    interface IOpenRavenSessionsInPipeline
    {
        IAsyncDocumentSession OpenSession(IDictionary<string, string> messageHeaders);
    }
}