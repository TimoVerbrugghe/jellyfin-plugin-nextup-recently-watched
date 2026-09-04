using Jellyfin.Database.Implementations.Entities;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.EnhancedNextUp.Filtering;

/// <summary>
/// Resolves the Enhanced Next Up episode for a series/user combination.
/// </summary>
public interface INextUpResolver
{
    /// <summary>
    /// Gets the episode that should be presented as "Next Up" for <paramref name="series"/>
    /// and <paramref name="user"/>, or <see langword="null"/> if the plugin should defer
    /// to Jellyfin's own selection (plugin/series disabled, nothing watched yet, or the
    /// series is fully watched).
    /// </summary>
    /// <param name="series">The series to compute a Next Up episode for.</param>
    /// <param name="user">The user whose watch state should be used.</param>
    /// <returns>The next episode, or <see langword="null"/>.</returns>
    Episode? GetNextEpisode(Series series, User user);
}
