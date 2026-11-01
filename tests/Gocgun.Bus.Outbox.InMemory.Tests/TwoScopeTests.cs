// <copyright file="TwoScopeTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Messages;
using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Two scopes keep separate buffers.
/// </summary>
public sealed class TwoScopeTests
{
    /// <summary>
    /// Flushing one scope publishes only that scope. A thrown unit in the other scope publishes nothing.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Second_scope_does_not_see_the_first_buffer()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope first = provider.CreateAsyncScope();
        await using AsyncServiceScope second = provider.CreateAsyncScope();
        IInMemoryOutbox firstOutbox = first.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        IInMemoryOutbox secondOutbox = second.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        IInMemoryOutboxUnit secondUnit = second.ServiceProvider.GetRequiredService<IInMemoryOutboxUnit>();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await firstOutbox.PublishAsync(new NoteEvent { Text = "first" }, cancellationToken: cancellationToken);
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await secondUnit.ExecuteAsync(
                async token =>
                {
                    await secondOutbox.PublishAsync(new NoteEvent { Text = "second" }, cancellationToken: token).ConfigureAwait(false);
                    throw new InvalidOperationException("second failed");
                },
                cancellationToken).ConfigureAwait(false);
        });

        await firstOutbox.FlushAsync(cancellationToken);

        Assert.Single(bus.Published);
        Assert.Contains("first", bus.Published[0], StringComparison.Ordinal);
    }
}
