// <copyright file="InMemoryOutboxServiceCollectionExtensions.cs" company="Gocgun">
// Copyright (c) 2026 Omer Faruk Gocgun
// </copyright>

namespace Gocgun.Bus.Outbox.InMemory;

using Gocgun.Bus;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Registers the in-memory outbox as a scoped buffer over <see cref="IMessageBus"/>.
/// </summary>
public static class InMemoryOutboxServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="IInMemoryOutbox"/> and <see cref="IInMemoryOutboxUnit"/> for the current scope.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <returns>The same service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services"/> is null.</exception>
    public static IServiceCollection AddGocgunInMemoryOutbox(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddScoped<InMemoryOutbox>();
        services.AddScoped<IInMemoryOutbox>(static provider => provider.GetRequiredService<InMemoryOutbox>());
        services.AddScoped<IInMemoryOutboxUnit>(static provider => provider.GetRequiredService<InMemoryOutbox>());
        return services;
    }
}
