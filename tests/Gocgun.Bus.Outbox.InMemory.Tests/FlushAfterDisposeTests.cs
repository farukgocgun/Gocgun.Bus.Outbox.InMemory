// <copyright file="FlushAfterDisposeTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Flush after the scope is disposed fails without publishing.
/// </summary>
public sealed class FlushAfterDisposeTests
{
    /// <summary>
    /// Flush on a disposed scope throws <see cref="ObjectDisposedException"/>.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Flush_after_scope_dispose_throws()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        await scope.DisposeAsync();

        await Assert.ThrowsAsync<ObjectDisposedException>(async () => await outbox.FlushAsync(TestContext.Current.CancellationToken).ConfigureAwait(false));
        Assert.Empty(bus.Published);
    }
}
