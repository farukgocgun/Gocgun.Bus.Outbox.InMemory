// <copyright file="OutboxHost.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory.Tests.Support;

using Gocgun.Bus;
using Gocgun.Bus.Outbox.InMemory;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Builds a provider with a recording bus and the in-memory outbox.
/// </summary>
internal static class OutboxHost
{
    /// <summary>
    /// Creates a root provider. The caller disposes it.
    /// </summary>
    /// <param name="bus">Bus shared by every scope.</param>
    /// <returns>The root provider.</returns>
    public static ServiceProvider Create(RecordingMessageBus bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ServiceCollection services = new();
        services.AddSingleton<IMessageBus>(bus);
        services.AddGocgunInMemoryOutbox();
        return services.BuildServiceProvider();
    }
}
