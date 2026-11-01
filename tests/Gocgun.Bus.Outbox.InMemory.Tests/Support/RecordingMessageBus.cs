// <copyright file="RecordingMessageBus.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests.Support;

using Gocgun.Bus;

/// <summary>
/// Records publish and send calls without a broker.
/// </summary>
internal sealed class RecordingMessageBus : IMessageBus
{
    private readonly List<string> published = [];
    private readonly List<string> sent = [];
    private int calls;

    /// <summary>
    /// Gets published event texts in order.
    /// </summary>
    public IReadOnlyList<string> Published => this.published;

    /// <summary>
    /// Gets sent command texts in order.
    /// </summary>
    public IReadOnlyList<string> Sent => this.sent;

    /// <summary>
    /// Gets or sets a task every publish awaits before recording, to hold a flush open.
    /// </summary>
    public Task? Hold { get; set; }

    /// <summary>
    /// Gets or sets the 1-based call number that throws, or null.
    /// </summary>
    public int? FailAtCall { get; set; }

    /// <inheritdoc />
    public async ValueTask PublishAsync<TEvent>(
        TEvent message,
        PublishOptions? options = null,
        CancellationToken cancellationToken = default)
        where TEvent : class, IIntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(message);
        cancellationToken.ThrowIfCancellationRequested();
        if (this.Hold is not null)
        {
            await this.Hold.ConfigureAwait(false);
        }

        if (++this.calls == this.FailAtCall)
        {
            throw new InvalidOperationException("bus down");
        }

        this.published.Add(message.ToString() ?? string.Empty);
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
        this.sent.Add(message.ToString() ?? string.Empty);
        return ValueTask.CompletedTask;
    }
}
