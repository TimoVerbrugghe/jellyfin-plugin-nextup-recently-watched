namespace Jellyfin.Plugin.EnhancedNextUp.Resolvers;

/// <summary>
/// A minimal, Jellyfin-agnostic representation of a single episode used by the
/// episode selection algorithm. Keeping this independent of
/// <c>MediaBrowser.Model.Dto.BaseItemDto</c> lets the algorithm be unit tested
/// without spinning up any part of the Jellyfin server.
/// </summary>
/// <param name="Id">
/// A stable identifier for the episode (maps to Jellyfin's item id in production).
/// </param>
/// <param name="SeasonNumber">
/// The season number the episode belongs to. Specials conventionally use season 0.
/// </param>
/// <param name="EpisodeNumber">
/// The starting episode number within the season. For multi-episode files this is the
/// first episode number (e.g. 1 for a file spanning E01-E02).
/// </param>
/// <param name="EndingEpisodeNumber">
/// The last episode number covered by this item. Equal to <paramref name="EpisodeNumber"/>
/// for single-episode files, and greater for multi-episode files (e.g. 2 for a file
/// spanning E01-E02). Jellyfin exposes this as <c>IndexNumberEnd</c>.
/// </param>
/// <param name="IsSpecial">
/// Whether this episode is a special (season 0), as determined by Jellyfin's own
/// season/parent metadata rather than by inspecting <paramref name="SeasonNumber"/> directly.
/// </param>
/// <param name="IsWatched">Whether the current user has marked this episode as played.</param>
/// <param name="LastPlayedDate">
/// When the current user most recently played this episode, if known. Used to determine
/// which episode was watched most recently across the whole series.
/// </param>
public sealed record EpisodeInfo(
    Guid Id,
    int SeasonNumber,
    int EpisodeNumber,
    int EndingEpisodeNumber,
    bool IsSpecial,
    bool IsWatched,
    DateTime? LastPlayedDate)
{
    /// <summary>
    /// Gets a comparable sort/sequence key for this episode: season first, then the
    /// episode's starting number. Used to order episodes and to find "the next one".
    /// </summary>
    public (int Season, int Episode) SequenceKey => (SeasonNumber, EpisodeNumber);
}
