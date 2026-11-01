// <copyright file="PendingDelivery.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory;

using Gocgun.Bus;

/// <summary>
/// One buffered publish or send, invoked only from <see cref="IInMemoryOutbox.FlushAsync"/>.
/// </summary>
internal sealed class PendingDelivery
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PendingDelivery"/> class.
    /// </summary>
    /// <param name="deliver">Bus call captured at enqueue time.</param>
    public PendingDelivery(Func<IMessageBus, CancellationToken, ValueTask> deliver)
    {
        ArgumentNullException.ThrowIfNull(deliver);
        this.Deliver = deliver;
    }

    /// <summary>
    /// Gets the bus call captured at enqueue time.
    /// </summary>
    public Func<IMessageBus, CancellationToken, ValueTask> Deliver { get; }
}
