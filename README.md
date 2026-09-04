<div align="center">
  <img src="images/icon.png" alt="Next Up (Recently Watched) for Jellyfin" width="128" />
  <h1>Next Up (Recently Watched) for Jellyfin</h1>
</div>

> [!CAUTION]
> **This plugin was written with AI assistance.** It has been tested against a live
> Jellyfin 10.11 server, but it has not been battle-tested across every library layout,
> client, or Jellyfin version.
>
> It is offered as is, with **no guarantee of support, bug fixes, or troubleshooting**.
> Please back up your Jellyfin configuration before installing, and report issues via
> GitHub Issues.

## What it does

By default, Jellyfin picks the **Next Up** episode for a series based on the
highest-numbered episode you've watched — not the one you watched most recently. If you
rewatch an earlier episode, Jellyfin still points Next Up at the far end of the series
instead of the episode right after the one you just watched.

**Next Up (Recently Watched)** changes this: it selects the Next Up episode based on the
**most recently watched** episode instead. So if you watched S01E01, S01E02, and S01E10 (in
that order), then went back and rewatched S01E02, Jellyfin's default would suggest S01E11 —
this plugin suggests S01E03 instead.

This filtering happens server-side (as an MVC action filter on the `/Shows/NextUp`
response), so it works the same way for every client — web, mobile, TV apps, etc. — without
any watch data being modified, marked, or deleted. Turning the plugin off, or disabling it
for a series, immediately puts Next Up back to Jellyfin's normal behavior for that series.

### Key behaviors

- Uses the most recently watched episode (`LastPlayedDate`), not the highest-numbered one.
- Skips already-watched episodes to find the next unwatched one, and correctly handles
  multi-episode files (e.g. `S01E01-E02`).
- If every standard episode in a series has been watched, no Next Up entry is generated —
  Jellyfin's own fallback behavior applies.
- Specials are ignored by default when computing the next episode (configurable).
- Supports rewatch and "season jump" scenarios by always trusting your most recent
  activity over historical completion state.

## Configuration

The plugin is **disabled for all TV shows by default**. On the plugin's settings page
(Dashboard → Plugins → Next Up (Recently Watched)) you can:

- Toggle **Enable for all TV shows** to turn it on everywhere at once, or
- Leave that off and pick individual shows from the per-series list instead.
- Toggle whether specials should be included when determining the next episode.

## Installation

1. In Jellyfin, go to **Dashboard → Plugins → Repositories**.
2. Add a new repository with this manifest URL:

   ```text
   https://github.com/TimoVerbrugghe/jellyfin-plugin-nextup-recently-watched/releases/latest/download/manifest.json
   ```

3. Go to **Dashboard → Plugins → Catalog**, find **Next Up (Recently Watched)**, and
   install it.
4. Restart Jellyfin.
5. Go to **Dashboard → Plugins → Next Up (Recently Watched)** to configure which shows it
   applies to.

Alternatively, download a release zip directly from the
[Releases](https://github.com/TimoVerbrugghe/jellyfin-plugin-nextup-recently-watched/releases)
page and extract it into your Jellyfin `plugins/` directory.

## Compatibility

Targets Jellyfin `10.11.x`. Future releases will aim to track the latest supported Jellyfin
version where API compatibility permits.

## Development

See [.github/copilot-instructions.md](.github/copilot-instructions.md) for the algorithm
spec, project layout, and build/test instructions if you'd like to contribute.
