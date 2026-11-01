// <copyright file="ExceptionDiscardsTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Messages;
using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// A failed unit drops the buffer.
/// </summary>
public sealed class ExceptionDiscardsTests
{
    /// <summary>
    /// When the unit throws, buffered messages are not published.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Execute_discards_buffer_when_unit_throws()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        IInMemoryOutboxUnit unit = scope.ServiceProvider.GetRequiredService<IInMemoryOutboxUnit>();

        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await unit.ExecuteAsync(
                async token =>
                {
                    await outbox.PublishAsync(new NoteEvent { Text = "lost" }, cancellationToken: token).ConfigureAwait(false);
                    throw new InvalidOperationException("rolled back");
                },
                cancellationToken).ConfigureAwait(false);
        });

        Assert.Empty(bus.Published);
        await outbox.FlushAsync(cancellationToken);
        Assert.Empty(bus.Published);
    }
}
