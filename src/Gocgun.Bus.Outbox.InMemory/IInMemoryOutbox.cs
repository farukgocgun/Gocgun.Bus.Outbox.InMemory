// <copyright file="IInMemoryOutbox.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory;

using Gocgun.Bus;

/// <summary>
/// Buffers integration messages for the current scope until <see cref="FlushAsync"/> runs.
/// </summary>
public interface IInMemoryOutbox
{
    /// <summary>
    /// Appends an integration event. The broker is not called.
    /// </summary>
    /// <typeparam name="TEvent">protobuf-net event contract.</typeparam>
    /// <param name="message">Payload stored until flush.</param>
    /// <param name="options">Optional ids and headers passed through on flush.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task. Nothing is published.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    ValueTask PublishAsync<TEvent>(
        TEvent message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent;

    /// <summary>
    /// Appends an integration command. The broker is not called.
    /// </summary>
    /// <typeparam name="TCommand">protobuf-net command contract.</typeparam>
    /// <param name="message">Payload stored until flush.</param>
    /// <param name="options">Optional ids and headers passed through on flush.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A completed task. Nothing is published.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="message"/> is null.</exception>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    ValueTask SendAsync<TCommand>(
        TCommand message,
        SendOptions? options = null,
        CancellationToken cancellationToken = default)
        where TCommand : class, IIntegrationCommand;

    /// <summary>
    /// Publishes every buffered message through <see cref="IMessageBus"/> in append order.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the bus has accepted each buffered message.</returns>
    /// <remarks>
    /// Delivery is not atomic. When the bus throws, messages accepted before the failure stay published and the failing message
    /// and those after it stay buffered, so a second flush resumes with the remainder. Discard drops them.
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    ValueTask FlushAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Drops every buffered message. Nothing is published. Blocks while a flush is running; prefer <see cref="DiscardAsync"/>.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    void Discard();

    /// <summary>
    /// Drops every buffered message without blocking a thread. Nothing is published.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that completes when the buffer is empty.</returns>
    /// <exception cref="ObjectDisposedException">Thrown when the scope buffer is disposed.</exception>
    ValueTask DiscardAsync(CancellationToken cancellationToken = default);
}
