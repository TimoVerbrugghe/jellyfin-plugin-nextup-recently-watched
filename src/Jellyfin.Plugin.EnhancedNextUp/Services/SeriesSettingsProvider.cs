namespace Jellyfin.Plugin.EnhancedNextUp.Services;

/// <summary>
/// Default <see cref="ISeriesSettingsProvider"/> backed by the plugin's persisted
/// <see cref="Configuration.PluginConfiguration"/>.
/// </summary>
public sealed class SeriesSettingsProvider : ISeriesSettingsProvider
{
    /// <inheritdoc />
    public bool IsEnabledForSeries(Guid seriesId)
    {
        var configuration = Plugin.Instance?.Configuration;
        if (configuration is null || !configuration.Enabled)
        {
            return false;
        }

        return !configuration.DisabledSeriesIds.Contains(seriesId);
    }
}
