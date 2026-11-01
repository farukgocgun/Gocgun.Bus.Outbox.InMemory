// <copyright file="NoteEvent.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests.Messages;

using Gocgun.Bus;
using ProtoBuf;

/// <summary>
/// Test integration event.
/// </summary>
[ProtoContract(Name = "gocgun.bus.outbox.inmemory.tests.v1.NoteEvent")]
public sealed class NoteEvent : IIntegrationEvent
{
    /// <summary>
    /// Gets or sets the text.
    /// </summary>
    [ProtoMember(1)]
    public string Text { get; set; } = string.Empty;

    /// <inheritdoc />
    public override string ToString()
    {
        return this.Text;
    }
}
