using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.NextUpRecentlyWatched.Configuration;

/// <summary>
/// Persisted, user-configurable settings for the Next Up (Recently Watched) plugin.
/// </summary>
public sealed class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin's episode selection logic is
    /// applied to every TV show by default. Defaults to <see langword="false"/>: the
    /// plugin ships opt-in, so administrators must explicitly enable it globally or
    /// pick individual series via <see cref="EnabledSeriesIds"/>.
    /// </summary>
    public bool EnableForAllSeries { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether specials (season 0) participate in the
    /// "most recently watched" calculation and can be returned as the next episode.
    /// Defaults to <see langword="false"/> per the README's Specials Handling section.
    /// </summary>
    public bool IncludeSpecials { get; set; }

    /// <summary>
    /// Gets or sets the set of series ids for which the plugin's logic should be
    /// applied even though <see cref="EnableForAllSeries"/> is off. Ignored when
    /// <see cref="EnableForAllSeries"/> is <see langword="true"/> (all series are
    /// already enabled). Stored as a list (rather than a dictionary) so it serializes
    /// cleanly via Jellyfin's XML configuration serializer; see README's "Series
    /// Metadata Storage" Option A.
    /// </summary>
    public List<Guid> EnabledSeriesIds { get; set; } = new();
}
