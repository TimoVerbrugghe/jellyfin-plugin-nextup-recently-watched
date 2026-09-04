namespace Jellyfin.Plugin.EnhancedNextUp.Resolvers;

/// <summary>
/// Implements the plugin's core episode selection algorithm described in the project
/// README: instead of "the episode after the highest-numbered watched episode", Enhanced
/// Next Up returns "the first unwatched episode after the most recently watched one".
/// </summary>
/// <remarks>
/// This type has no dependency on Jellyfin server types so the algorithm can be unit
/// tested in isolation (see <c>EpisodeSequenceResolverTests</c>).
/// </remarks>
public static class EpisodeSequenceResolver
{
    /// <summary>
    /// Determines the Enhanced Next Up episode for a series.
    /// </summary>
    /// <param name="episodes">
    /// Every standard-content episode belonging to the series (across all seasons),
    /// in any order. Specials should still be included here — use
    /// <paramref name="includeSpecials"/> to control whether they participate.
    /// </param>
    /// <param name="includeSpecials">
    /// Whether specials (season 0) should count towards "most recently watched" and be
    /// eligible as a "next episode" result. Defaults to <see langword="false"/> per the
    /// plugin's default configuration, matching the README's Specials Handling section.
    /// </param>
    /// <returns>
    /// The next episode to present, or <see langword="null"/> when either nothing has
    /// been watched yet (nothing to base a recommendation on, so Jellyfin's own
    /// selection should be used) or every remaining episode is already watched
    /// (a completed series, per the README's Completed Series Behavior section).
    /// </returns>
    public static EpisodeInfo? GetNextEpisode(IReadOnlyCollection<EpisodeInfo> episodes, bool includeSpecials = false)
    {
        ArgumentNullException.ThrowIfNull(episodes);

        var candidates = episodes
            .Where(e => includeSpecials || !e.IsSpecial)
            .OrderBy(e => e.SequenceKey.Season)
            .ThenBy(e => e.SequenceKey.Episode)
            .ToList();

        if (candidates.Count == 0)
        {
            return null;
        }

        var mostRecentIndex = FindMostRecentlyWatchedIndex(candidates);
        if (mostRecentIndex is null)
        {
            // Nothing watched yet: there is no "recent activity" to anchor on, so the
            // plugin defers to Jellyfin's default Next Up behavior.
            return null;
        }

        // Walk forward from the most recently watched episode, skipping any further
        // episodes that are already watched (README: "Skip already watched episodes
        // until the first unwatched episode is found"). Reaching the end of the list
        // without finding one means the series is fully watched from this point on.
        for (var i = mostRecentIndex.Value + 1; i < candidates.Count; i++)
        {
            if (!candidates[i].IsWatched)
            {
                return candidates[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Finds the index, within an already-sorted candidate list, of the episode the user
    /// watched most recently (by <see cref="EpisodeInfo.LastPlayedDate"/>). Ties are
    /// broken by preferring the episode later in the series' sequence, since that is the
    /// more specific signal of "where the user currently is".
    /// </summary>
    private static int? FindMostRecentlyWatchedIndex(IReadOnlyList<EpisodeInfo> candidates)
    {
        int? bestIndex = null;
        DateTime bestDate = DateTime.MinValue;

        for (var i = 0; i < candidates.Count; i++)
        {
            var episode = candidates[i];
            if (!episode.IsWatched || episode.LastPlayedDate is not DateTime playedDate)
            {
                continue;
            }

            if (bestIndex is null
                || playedDate > bestDate
                || (playedDate == bestDate && candidates[i].SequenceKey.CompareTo(candidates[bestIndex.Value].SequenceKey) > 0))
            {
                bestIndex = i;
                bestDate = playedDate;
            }
        }

        return bestIndex;
    }
}
