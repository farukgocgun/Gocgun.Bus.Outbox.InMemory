// <copyright file="FlushPublishesTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Messages;
using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Flush publishes buffered messages.
/// </summary>
public sealed class FlushPublishesTests
{
    /// <summary>
    /// A successful flush publishes the event and the command once.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Flush_publishes_buffered_event_and_command()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await outbox.PublishAsync(new NoteEvent { Text = "event" }, cancellationToken: cancellationToken);
        await outbox.SendAsync(new NoteCommand { Text = "command" }, cancellationToken: cancellationToken);
        Assert.Empty(bus.Published);
        Assert.Empty(bus.Sent);

        await outbox.FlushAsync(cancellationToken);

        Assert.Single(bus.Published);
        Assert.Contains("event", bus.Published[0], StringComparison.Ordinal);
        Assert.Single(bus.Sent);
        Assert.Contains("command", bus.Sent[0], StringComparison.Ordinal);
    }
}
