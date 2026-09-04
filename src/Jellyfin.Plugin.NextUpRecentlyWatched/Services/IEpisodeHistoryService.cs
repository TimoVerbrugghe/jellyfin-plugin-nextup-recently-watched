using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.NextUpRecentlyWatched.Resolvers;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.NextUpRecentlyWatched.Services;

/// <summary>
/// Retrieves the episodes of a series, translated into the algorithm's
/// Jellyfin-agnostic <see cref="EpisodeInfo"/> shape, for a specific user.
/// </summary>
public interface IEpisodeHistoryService
{
    /// <summary>
    /// Gets every episode of <paramref name="series"/>, with watched state and last
    /// played date resolved for <paramref name="user"/>.
    /// </summary>
    /// <param name="series">The series to read episodes from.</param>
    /// <param name="user">The user whose watch history/state should be used.</param>
    /// <returns>The series' episodes as <see cref="EpisodeInfo"/> instances.</returns>
    IReadOnlyCollection<EpisodeInfo> GetEpisodes(Series series, User user);
}
