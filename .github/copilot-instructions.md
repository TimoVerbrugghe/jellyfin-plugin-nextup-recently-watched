# Copilot instructions

Guidance for GitHub Copilot (and humans) working in this repository.

## Project summary

**Next Up (Recently Watched)** is a Jellyfin server plugin. It replaces Jellyfin's built-in
"Next Up" episode selection with one based on the *most recently watched*
episode of a series, rather than the highest-numbered watched episode. See
[README.md](../README.md) for the user-facing description and install
instructions.

The plugin is architecturally modeled on
[ch-bauer/jellyfin-plugin-nextup-cleanup](https://github.com/ch-bauer/jellyfin-plugin-nextup-cleanup)
(server-side MVC action filter that rewrites the `/Shows/NextUp` response).

## Episode selection algorithm

1. Find the most recently watched episode for the current user (by
   `LastPlayedDate`, not by highest episode number).
2. Find the next sequential episode in the series (`S01E01` -> `S01E02`,
   `S01E24` -> `S02E01`, etc.), using Jellyfin's own episode
   numbering/ordering so multi-episode files (e.g. `S01E01-E02`) are handled
   correctly.
3. Skip already-watched episodes until the first unwatched episode is found.
4. If every standard episode in the series is watched, the plugin generates
   no Next Up entry (falls back to Jellyfin's default behavior) rather than
   inventing one.

Specials are excluded from the calculation by default (configurable via
`IncludeSpecials`). Season jumps (e.g. most recently watched is `S05E08`
after only `S01E01`/`S01E02` were watched before) intentionally resolve to
`S05E09` — the algorithm always trusts the most recent activity over
historical completion state, including on rewatches.

### Acceptance scenarios (must keep passing in `EpisodeSequenceResolverTests.cs`)

| Watched | Most recent | Expected next |
| --- | --- | --- |
| E1, E2, E10 | E2 | E3 |
| E1, E2, E3 | E1 | E4 |
| S1E1, S1E2, S5E8 | S5E8 | S5E9 |
| Special 1 watched, current S1E5 | S1E5 | S1E6 (specials ignored by default) |
| S1E1-E2 (multi-episode file) watched | S1E1-E2 | S1E3 |
| All episodes watched | — | No plugin-generated entry |

## Layout

- `src/Jellyfin.Plugin.NextUpRecentlyWatched/` — plugin source.
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
- `tests/Jellyfin.Plugin.NextUpRecentlyWatched.Tests/` — xUnit tests. Currently only
  `EpisodeSequenceResolverTests.cs`, covering all 6 README acceptance
  scenarios plus edge cases. This project has **no** dependency on a running
  Jellyfin server — it tests the pure algorithm directly.
- `Jellyfin.Plugin.NextUpRecentlyWatched.slnx` — solution file (build/test at this
  level, not per-project).

## Build & test

```bash
dotnet build Jellyfin.Plugin.NextUpRecentlyWatched.slnx
dotnet test Jellyfin.Plugin.NextUpRecentlyWatched.slnx
```

Both commands restore `Jellyfin.Controller`/`Jellyfin.Model` 10.11.3 from
NuGet on first run (this can take ~15-20s per project). The test project
targets `net9.0` with `<RollForward>LatestMajor</RollForward>` so it also
runs correctly when only a newer shared .NET runtime is installed locally.

Run **all** tests before committing changes to `EpisodeSequenceResolver.cs`
or the services/filters that depend on it. Any behavioral change to the
selection algorithm should come with an updated or new test case, and should
be reconciled with the acceptance scenarios listed above.

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
