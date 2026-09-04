# AGENTS.md

Guidance for AI coding agents (and humans) working in this repository.

## Project summary

**Enhanced Next Up** is a Jellyfin server plugin. It replaces Jellyfin's built-in
"Next Up" episode selection with one based on the *most recently watched*
episode of a series, rather than the highest-numbered watched episode. See
[README.md](README.md) for the full design spec and the acceptance scenarios
the algorithm must satisfy.

The plugin is architecturally modeled on
[ch-bauer/jellyfin-plugin-nextup-cleanup](https://github.com/ch-bauer/jellyfin-plugin-nextup-cleanup)
(server-side MVC action filter that rewrites the `/Shows/NextUp` response).

## Layout

- `src/Jellyfin.Plugin.EnhancedNextUp/` — plugin source.
  - `Resolvers/EpisodeSequenceResolver.cs` — the core, pure algorithm (no
    Jellyfin server dependency). Prefer making algorithm changes here; it is
    the only class covered by fast unit tests.
  - `Resolvers/EpisodeInfo.cs` — plain data record passed into the resolver.
  - `Services/` — bridges Jellyfin's `Series`/`User`/`Episode`/`BaseItem`
    types into `EpisodeInfo` (`EpisodeHistoryService`) and reads/writes
    per-series settings (`SeriesSettingsProvider`).
  - `Filtering/` — `NextUpResolver` (orchestrates history + algorithm) and
    `NextUpActionFilter` (MVC `IAsyncActionFilter` that intercepts the
    `/Shows/NextUp` endpoint and swaps in the resolved episode).
  - `Configuration/` — `PluginConfiguration.cs` + `configPage.html` (plugin
    settings page served by Jellyfin's dashboard).
  - `Plugin.cs`, `PluginServiceRegistrator.cs` — Jellyfin plugin entry points
    (`BasePlugin`, `IPluginServiceRegistrator`).
- `tests/Jellyfin.Plugin.EnhancedNextUp.Tests/` — xUnit tests. Currently only
  `EpisodeSequenceResolverTests.cs`, covering all 6 README acceptance
  scenarios plus edge cases. This project has **no** dependency on a running
  Jellyfin server — it tests the pure algorithm directly.
- `Jellyfin.Plugin.EnhancedNextUp.slnx` — solution file (build/test at this
  level, not per-project).

## Build & test

```bash
dotnet build Jellyfin.Plugin.EnhancedNextUp.slnx
dotnet test Jellyfin.Plugin.EnhancedNextUp.slnx
```

Both commands restore `Jellyfin.Controller`/`Jellyfin.Model` 10.11.3 from
NuGet on first run (this can take ~15-20s per project). The test project
targets `net9.0` with `<RollForward>LatestMajor</RollForward>` so it also
runs correctly when only a newer shared .NET runtime is installed locally.

Run **all** tests before committing changes to `EpisodeSequenceResolver.cs`
or the services/filters that depend on it. Any behavioral change to the
selection algorithm should come with an updated or new test case, and should
be reconciled with the acceptance scenarios in [README.md](README.md).

## Conventions

- Nullable reference types are enabled (`<Nullable>enable</Nullable>`) — don't
  suppress warnings with `!` unless the null-impossibility is truly guaranteed
  by the surrounding code; prefer a real null-check.
- Keep `EpisodeSequenceResolver` free of any `Jellyfin.*` server-type
  dependency. All Jellyfin-specific glue belongs in `Services/` or
  `Filtering/`, which then map to/from the plain `EpisodeInfo` record.
- `TreatWarningsAsErrors` is `false`, but new code should still build with 0
  warnings — treat warnings as bugs to fix, not to suppress.
- `GenerateDocumentationFile` is enabled on the src project; public
  types/members should have XML doc comments.

## Jellyfin API gotchas (10.11.3)

These are non-obvious, version-specific facts discovered while building this
plugin — useful context if the target Jellyfin.Controller/Model version ever
changes and code needs to be re-verified:

- `User` (and other core entities) live in
  `Jellyfin.Database.Implementations.Entities`, **not**
  `Jellyfin.Data.Entities`.
- `Series.GetEpisodes(User, DtoOptions, bool shouldIncludeMissingEpisodes)`
  takes 3 required parameters.
- `IUserItemData.LastPlayedDate` is `DateTime?`, not `DateTimeOffset?`.
- `QueryResult<T>.Items` is `IReadOnlyList<T>` — to replace an entry you must
  build a new list and reassign the whole `Items` property.

## What NOT to do

- Don't add a dependency on a running Jellyfin server to the test project;
  keep tests fast and self-contained.
- Don't hardcode a different plugin GUID in `configPage.html` than the one
  returned by `Plugin.Id` in `Plugin.cs` — they must match exactly.
