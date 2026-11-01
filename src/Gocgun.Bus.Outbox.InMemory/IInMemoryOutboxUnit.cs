// <copyright file="IInMemoryOutboxUnit.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory;

/// <summary>
/// Runs one unit of work against the scope buffer.
/// </summary>
public interface IInMemoryOutboxUnit
{
    /// <summary>
    /// Runs <paramref name="work"/>, then flushes. Any exception discards the buffered messages that were not yet handed to the bus and is rethrown; messages already accepted by the bus are not rolled back.
    /// </summary>
    /// <param name="work">Unit of work. It may append messages on the same scope.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes after a successful flush.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="work"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    ValueTask ExecuteAsync(Func<CancellationToken, ValueTask> work, CancellationToken cancellationToken = default);
}
