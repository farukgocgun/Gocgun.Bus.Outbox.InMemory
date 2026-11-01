// <copyright file="DoubleFlushTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Messages;
using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A completed flush does not publish again.
/// </summary>
public sealed class DoubleFlushTests
{
    /// <summary>
    /// A second flush after success does not duplicate the message.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Second_flush_does_not_publish_again()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await outbox.PublishAsync(new NoteEvent { Text = "once" }, cancellationToken: cancellationToken);
        await outbox.FlushAsync(cancellationToken);
        await outbox.FlushAsync(cancellationToken);

        Assert.Single(bus.Published);
    }
}
