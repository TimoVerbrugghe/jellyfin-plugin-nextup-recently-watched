using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.EnhancedNextUp.Resolvers;
using Jellyfin.Plugin.EnhancedNextUp.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;

namespace Jellyfin.Plugin.EnhancedNextUp.Filtering;

/// <summary>
/// Orchestrates the Enhanced Next Up algorithm for a given series/user combination:
/// checks whether the plugin is enabled for that series, loads the episode data, and
/// delegates the actual selection to <see cref="EpisodeSequenceResolver"/>.
/// </summary>
public sealed class NextUpResolver : INextUpResolver
{
    private readonly ISeriesSettingsProvider _seriesSettingsProvider;
    private readonly IEpisodeHistoryService _episodeHistoryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NextUpResolver"/> class.
    /// </summary>
    /// <param name="seriesSettingsProvider">Instance of the <see cref="ISeriesSettingsProvider"/> interface.</param>
    /// <param name="episodeHistoryService">Instance of the <see cref="IEpisodeHistoryService"/> interface.</param>
    public NextUpResolver(ISeriesSettingsProvider seriesSettingsProvider, IEpisodeHistoryService episodeHistoryService)
    {
        _seriesSettingsProvider = seriesSettingsProvider;
        _episodeHistoryService = episodeHistoryService;
    }

    /// <inheritdoc />
    public Episode? GetNextEpisode(Series series, User user)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(user);

        if (!_seriesSettingsProvider.IsEnabledForSeries(series.Id))
        {
            return null;
        }

        var includeSpecials = Plugin.Instance?.Configuration.IncludeSpecials ?? false;
        var episodeInfos = _episodeHistoryService.GetEpisodes(series, user);
        var nextEpisodeInfo = EpisodeSequenceResolver.GetNextEpisode(episodeInfos, includeSpecials);
        if (nextEpisodeInfo is null)
        {
            return null;
        }

        return series
            .GetEpisodes(user, new MediaBrowser.Controller.Dto.DtoOptions(false), shouldIncludeMissingEpisodes: false)
            .OfType<Episode>()
            .FirstOrDefault(e => e.Id == nextEpisodeInfo.Id);
    }
}
