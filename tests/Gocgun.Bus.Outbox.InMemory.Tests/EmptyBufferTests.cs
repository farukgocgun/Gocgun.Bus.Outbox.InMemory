// <copyright file="EmptyBufferTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// An empty buffer flushes without calling the bus.
/// </summary>
public sealed class EmptyBufferTests
{
    /// <summary>
    /// Flush with no messages completes and leaves the bus idle.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Flush_on_empty_buffer_publishes_nothing()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();

        await outbox.FlushAsync(TestContext.Current.CancellationToken);

        Assert.Empty(bus.Published);
        Assert.Empty(bus.Sent);
    }
}
