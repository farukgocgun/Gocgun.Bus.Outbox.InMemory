// <copyright file="AsyncSafetyTests.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests;

using Gocgun.Bus.Outbox.InMemory.Tests.Messages;
using Gocgun.Bus.Outbox.InMemory.Tests.Support;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Async discard, dispose during a flush, and partial flush behavior.
/// </summary>
public sealed class AsyncSafetyTests
{
    /// <summary>
    /// DiscardAsync drops the buffer without publishing.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task DiscardAsync_drops_buffer()
    {
        RecordingMessageBus bus = new();
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;

        await outbox.PublishAsync(new NoteEvent { Text = "dropped" }, cancellationToken: cancellationToken);
        await outbox.DiscardAsync(cancellationToken);
        await outbox.FlushAsync(cancellationToken);

        Assert.Empty(bus.Published);
    }

    /// <summary>
    /// Disposing the scope while a flush awaits the bus waits for the flush instead of failing it.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task DisposeAsync_waits_for_running_flush()
    {
        TaskCompletionSource hold = new(TaskCreationOptions.RunContinuationsAsynchronously);
        RecordingMessageBus bus = new() { Hold = hold.Task };
        await using ServiceProvider provider = OutboxHost.Create(bus);
        AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await outbox.PublishAsync(new NoteEvent { Text = "in-flight" }, cancellationToken: cancellationToken);

        Task flush = outbox.FlushAsync(cancellationToken).AsTask();
        Task dispose = scope.DisposeAsync().AsTask();
        await Task.Delay(100, cancellationToken);
        Assert.False(dispose.IsCompleted);

        hold.SetResult();
        await flush;
        await dispose;

        Assert.Single(bus.Published);
    }

    /// <summary>
    /// A bus failure mid-flush keeps the undelivered rest buffered, so a second flush resumes without republishing.
    /// </summary>
    /// <returns>A task that completes when the assertion finishes.</returns>
    [Fact]
    public async Task Partial_flush_resumes_with_remainder()
    {
        RecordingMessageBus bus = new() { FailAtCall = 2 };
        await using ServiceProvider provider = OutboxHost.Create(bus);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IInMemoryOutbox outbox = scope.ServiceProvider.GetRequiredService<IInMemoryOutbox>();
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        await outbox.PublishAsync(new NoteEvent { Text = "one" }, cancellationToken: cancellationToken);
        await outbox.PublishAsync(new NoteEvent { Text = "two" }, cancellationToken: cancellationToken);
        await outbox.PublishAsync(new NoteEvent { Text = "three" }, cancellationToken: cancellationToken);

        await Assert.ThrowsAsync<InvalidOperationException>(async () => await outbox.FlushAsync(cancellationToken).ConfigureAwait(false));
        Assert.Single(bus.Published);

        await outbox.FlushAsync(cancellationToken);

        Assert.Equal(3, bus.Published.Count);
        Assert.Contains("two", bus.Published[1], StringComparison.Ordinal);
        Assert.Contains("three", bus.Published[2], StringComparison.Ordinal);
    }
}
