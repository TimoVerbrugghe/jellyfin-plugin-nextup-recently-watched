namespace Jellyfin.Plugin.NextUpRecentlyWatched.Services;

/// <summary>
/// Reads per-series enable/disable state from the plugin configuration.
/// </summary>
public interface ISeriesSettingsProvider
{
    /// <summary>
    /// Determines whether Next Up (Recently Watched) should compute the next episode for the
    /// given series, or whether Jellyfin's built-in logic should be used instead.
    /// </summary>
    /// <param name="seriesId">The series id.</param>
    /// <returns><see langword="true"/> if the plugin's logic should apply.</returns>
    bool IsEnabledForSeries(Guid seriesId);
}
