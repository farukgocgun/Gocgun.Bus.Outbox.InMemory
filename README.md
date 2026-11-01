# Gocgun.Bus.Outbox.InMemory

> **Beta.** This package is in beta: the public API may change between releases.
>
> It references `Gocgun.Bus` through a sibling `ProjectReference`; the base package repository must sit next to this one to build. Publish order: `Gocgun.Bus` first.

Buffer that holds `Gocgun.Bus` messages until a unit of work succeeds.

License: MIT (`LICENSE`).

`IInMemoryOutbox.PublishAsync` and `SendAsync` append to the current DI scope. Nothing reaches `IMessageBus` until `FlushAsync`. `IInMemoryOutboxUnit.ExecuteAsync` flushes after the delegate returns and discards the buffer when the delegate throws.

Limits:

- Not durable. The buffer is process memory. A stop between commit and flush loses the messages.
- No atomicity with a database transaction. A commit can succeed and the later publish can fail. The buffer only stops the other failure: a message published for work that then rolled back.
- A second `FlushAsync` after a completed flush does not publish those messages again.
- `FlushAsync` after the scope is disposed throws `ObjectDisposedException`.
- It publishes through the unkeyed `IMessageBus`, which is the bus named `default`. A host with only named buses must register a `default` bus to use the outbox.

```csharp
services.AddGocgunInMemoryOutbox();
```

The host calls `FlushAsync` after its own commit, or uses `ExecuteAsync` as the unit boundary. This package has no table and no Entity Framework types.

## Publishing to nuget.org

Run `scripts/publish.sh` (or `scripts/publish.ps1` on Windows). It dispatches the manual `release.yml` workflow of each dependency first, in dependency order (Gocgun.Bus, Gocgun.Bus.Outbox.InMemory), then of this package, waiting for each run and stopping on the first failure. Requires the GitHub CLI signed in with `gh auth login`; the `nuget` environment and the nuget.org Trusted Publishing policy must exist in each repository.
