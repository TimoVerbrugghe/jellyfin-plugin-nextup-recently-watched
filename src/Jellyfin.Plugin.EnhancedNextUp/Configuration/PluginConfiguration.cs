using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.EnhancedNextUp.Configuration;

/// <summary>
/// Persisted, user-configurable settings for the Enhanced Next Up plugin.
/// </summary>
public sealed class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets a value indicating whether the plugin's episode selection logic is
    /// active at all. Allows disabling the whole plugin without uninstalling it.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether specials (season 0) participate in the
    /// "most recently watched" calculation and can be returned as the next episode.
    /// Defaults to <see langword="false"/> per the README's Specials Handling section.
    /// </summary>
    public bool IncludeSpecials { get; set; }

    /// <summary>
    /// Gets or sets the set of series ids for which the plugin's logic should NOT be
    /// applied, letting Jellyfin's built-in Next Up selection take over instead. Stored
    /// as a list (rather than a dictionary) so it serializes cleanly via Jellyfin's XML
    /// configuration serializer; see README's "Series Metadata Storage" Option A.
    /// </summary>
    public List<Guid> DisabledSeriesIds { get; set; } = new();
}
