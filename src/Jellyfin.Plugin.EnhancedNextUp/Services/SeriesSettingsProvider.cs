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
        if (configuration is null)
        {
            return false;
        }

        return configuration.EnableForAllSeries || configuration.EnabledSeriesIds.Contains(seriesId);
    }
}
