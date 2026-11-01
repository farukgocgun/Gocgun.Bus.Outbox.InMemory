// <copyright file="InMemoryOutbox.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory;

using Gocgun.Bus;

/// <summary>
/// Scope-owned buffer. Flush publishes once. Discard drops the buffer.
/// </summary>
internal sealed class InMemoryOutbox : IInMemoryOutbox, IInMemoryOutboxUnit, IDisposable, IAsyncDisposable
{
    private readonly IMessageBus bus;
    private readonly List<PendingDelivery> pending = [];
    private readonly SemaphoreSlim gate = new(1, 1);
    private int disposed;
    private bool flushed;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryOutbox"/> class.
    /// </summary>
    /// <param name="bus">Transport used by <see cref="FlushAsync"/>.</param>
    public InMemoryOutbox(IMessageBus bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        this.bus = bus;
    }

    /// <summary>
    /// Gets a value indicating whether the scope buffer is disposed.
    /// </summary>
    private bool IsDisposed => Volatile.Read(ref this.disposed) != 0;

    /// <inheritdoc />
    public ValueTask PublishAsync<TEvent>(
        TEvent message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        return this.EnqueueAsync(
            (bus, token) => bus.PublishAsync(message, options, token),
            cancellationToken);
    }

    /// <inheritdoc />
    public ValueTask SendAsync<TCommand>(
        TCommand message,
        SendOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCommand : class, IIntegrationCommand
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        return this.EnqueueAsync(
            (bus, token) => bus.SendAsync(message, options, token),
            cancellationToken);
    }

    /// <inheritdoc />
    public async ValueTask FlushAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this.IsDisposed, this);
        await this.gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(this.IsDisposed, this);
            if (this.flushed)
            {
                return;
            }

            while (this.pending.Count > 0)
            {
                PendingDelivery next = this.pending[0];
                await next.Deliver(this.bus, cancellationToken).ConfigureAwait(false);
                this.pending.RemoveAt(0);
            }

            this.flushed = true;
        }
        finally
        {
            this.gate.Release();
        }
    }

    /// <inheritdoc />
    public void Discard()
    {
        ObjectDisposedException.ThrowIf(this.IsDisposed, this);
        this.gate.Wait();
        try
        {
            ObjectDisposedException.ThrowIf(this.IsDisposed, this);
            this.pending.Clear();
        }
        finally
        {
            this.gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DiscardAsync(CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(this.IsDisposed, this);
        await this.gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(this.IsDisposed, this);
            this.pending.Clear();
        }
        finally
        {
            this.gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> work, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(work);
        ObjectDisposedException.ThrowIf(this.IsDisposed, this);
        try
        {
            await work(cancellationToken).ConfigureAwait(false);
            await this.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            if (!this.IsDisposed)
            {
                // Messages already handed to the bus before the failure are published and are not rolled back.
                await this.DiscardAsync(CancellationToken.None).ConfigureAwait(false);
            }

            throw;
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) != 0)
        {
            return;
        }

        // Never block on a running flush. The semaphore has no wait handle, so leaving it undisposed releases nothing.
        if (this.gate.Wait(0))
        {
            this.pending.Clear();
            this.gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref this.disposed, 1) != 0)
        {
            return;
        }

        // Let a running flush finish instead of failing it with ObjectDisposedException.
        await this.gate.WaitAsync().ConfigureAwait(false);
        this.pending.Clear();
        this.gate.Release();
    }

    private async ValueTask EnqueueAsync(
        Func<IMessageBus, CancellationToken, ValueTask> deliver,
        CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(this.IsDisposed, this);
        await this.gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ObjectDisposedException.ThrowIf(this.IsDisposed, this);
            if (this.flushed)
            {
                throw new InvalidOperationException("The in-memory outbox was already flushed.");
            }

            this.pending.Add(new PendingDelivery(deliver));
        }
        finally
        {
            this.gate.Release();
        }
    }
}
