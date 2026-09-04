using Jellyfin.Database.Implementations.Entities;
using Jellyfin.Plugin.EnhancedNextUp.Resolvers;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;

namespace Jellyfin.Plugin.EnhancedNextUp.Services;

/// <summary>
/// Builds the pure <see cref="EpisodeInfo"/> view of a series' episodes (for a given
/// user) that <see cref="EpisodeSequenceResolver"/> needs, translating from Jellyfin's
/// library/user-data model.
/// </summary>
public sealed class EpisodeHistoryService : IEpisodeHistoryService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserDataManager _userDataManager;

    /// <summary>
    /// Initializes a new instance of the <see cref="EpisodeHistoryService"/> class.
    /// </summary>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userDataManager">Instance of the <see cref="IUserDataManager"/> interface.</param>
    public EpisodeHistoryService(ILibraryManager libraryManager, IUserDataManager userDataManager)
    {
        _libraryManager = libraryManager;
        _userDataManager = userDataManager;
    }

    /// <inheritdoc />
    public IReadOnlyCollection<EpisodeInfo> GetEpisodes(Series series, User user)
    {
        ArgumentNullException.ThrowIfNull(series);
        ArgumentNullException.ThrowIfNull(user);

        var episodes = series.GetEpisodes(user, new DtoOptions(false), shouldIncludeMissingEpisodes: false);

        var result = new List<EpisodeInfo>();
        foreach (var item in episodes)
        {
            if (item is not Episode episode)
            {
                continue;
            }

            var userData = _userDataManager.GetUserData(user, episode);
            var isSpecial = episode.ParentIndexNumber is 0;

            result.Add(new EpisodeInfo(
                episode.Id,
                episode.ParentIndexNumber ?? 0,
                episode.IndexNumber ?? 0,
                episode.IndexNumberEnd ?? episode.IndexNumber ?? 0,
                isSpecial,
                userData?.Played ?? false,
                userData?.LastPlayedDate));
        }

        return result;
    }
}
