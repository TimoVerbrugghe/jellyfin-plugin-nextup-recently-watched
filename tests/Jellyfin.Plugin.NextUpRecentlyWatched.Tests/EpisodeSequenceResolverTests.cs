using Jellyfin.Plugin.NextUpRecentlyWatched.Resolvers;
using Xunit;

namespace Jellyfin.Plugin.NextUpRecentlyWatched.Tests;

/// <summary>
/// Unit tests for <see cref="EpisodeSequenceResolver"/>, mapped directly to the
/// Acceptance Tests described in the project README.
/// </summary>
public sealed class EpisodeSequenceResolverTests
{
    private static readonly DateTime BaseDate = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static EpisodeInfo Ep(
        int season,
        int episode,
        bool watched,
        DateTime? lastPlayed = null,
        int? endingEpisode = null,
        bool isSpecial = false)
    {
        return new EpisodeInfo(
            Guid.NewGuid(),
            season,
            episode,
            endingEpisode ?? episode,
            isSpecial,
            watched,
            lastPlayed);
    }

    /// <summary>
    /// Acceptance Test 1: basic advancement. The most recently (and only) watched
    /// episode is S1E3, so the next episode should be S1E4.
    /// </summary>
    [Fact]
    public void GetNextEpisode_BasicAdvancement_ReturnsFollowingEpisode()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(1, 2, watched: true, BaseDate.AddMinutes(1)),
            Ep(1, 3, watched: true, BaseDate.AddMinutes(2)),
            Ep(1, 4, watched: false),
            Ep(1, 5, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.NotNull(next);
        Assert.Equal((1, 4), next!.SequenceKey);
    }

    /// <summary>
    /// Acceptance Test 2: rewatch scenario. The user finished the series long ago (S1E5
    /// watched furthest back) but most recently rewatched S1E2, so the next episode
    /// should be S1E3 — not "series complete" and not S1E6.
    /// </summary>
    [Fact]
    public void GetNextEpisode_RewatchOfEarlierEpisode_ReturnsEpisodeAfterRewatched()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(1, 2, watched: true, BaseDate.AddDays(30)), // most recently watched
            Ep(1, 3, watched: false),
            Ep(1, 4, watched: true, BaseDate.AddDays(1)),
            Ep(1, 5, watched: true, BaseDate.AddDays(2))
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.NotNull(next);
        Assert.Equal((1, 3), next!.SequenceKey);
    }

    /// <summary>
    /// Acceptance Test 3: season-jump scenario. The most recently watched episode is the
    /// last episode of season 1, so the next episode should roll over into season 2.
    /// </summary>
    [Fact]
    public void GetNextEpisode_EndOfSeason_RollsOverToNextSeason()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(1, 2, watched: true, BaseDate.AddMinutes(1)),
            Ep(2, 1, watched: false),
            Ep(2, 2, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.NotNull(next);
        Assert.Equal((2, 1), next!.SequenceKey);
    }

    /// <summary>
    /// Acceptance Test 4: specials must not affect the calculation by default. A
    /// recently watched special should be ignored, and specials should never be
    /// returned as the next episode, unless explicitly included.
    /// </summary>
    [Fact]
    public void GetNextEpisode_SpecialsExcludedByDefault_AreIgnored()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(0, 1, watched: true, BaseDate.AddDays(10), isSpecial: true), // most recent overall, but a special
            Ep(1, 2, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes, includeSpecials: false);

        Assert.NotNull(next);
        Assert.Equal((1, 2), next!.SequenceKey);
    }

    /// <summary>
    /// Specials can optionally participate when explicitly enabled.
    /// </summary>
    [Fact]
    public void GetNextEpisode_SpecialsIncluded_CanBeReturnedAsNext()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(1, 2, watched: false, isSpecial: true), // e.g. a mid-season special
            Ep(1, 3, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes, includeSpecials: true);

        Assert.NotNull(next);
        Assert.True(next!.IsSpecial);
    }

    /// <summary>
    /// Acceptance Test 5: multi-episode files. A file spanning S1E01-E02 should be
    /// treated as a single sequence step; the episode following it is S1E03, using
    /// Jellyfin's own <c>IndexNumberEnd</c>-derived ending episode number rather than
    /// assuming one file equals one episode number.
    /// </summary>
    [Fact]
    public void GetNextEpisode_MultiEpisodeFile_SequencesByStartingEpisodeNumber()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate, endingEpisode: 2), // covers E01-E02 in one file
            Ep(1, 3, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.NotNull(next);
        Assert.Equal((1, 3), next!.SequenceKey);
    }

    /// <summary>
    /// Acceptance Test 6: completed series. Every episode is watched, so there is no
    /// "next" episode and the plugin should produce no entry (deferring entirely to
    /// Jellyfin, which will also have nothing to show).
    /// </summary>
    [Fact]
    public void GetNextEpisode_CompletedSeries_ReturnsNull()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: true, BaseDate),
            Ep(1, 2, watched: true, BaseDate.AddMinutes(1)),
            Ep(1, 3, watched: true, BaseDate.AddMinutes(2))
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.Null(next);
    }

    /// <summary>
    /// When nothing has been watched yet there is no signal to anchor on, so the plugin
    /// should defer to Jellyfin's default behavior rather than guessing.
    /// </summary>
    [Fact]
    public void GetNextEpisode_NothingWatched_ReturnsNull()
    {
        var episodes = new[]
        {
            Ep(1, 1, watched: false),
            Ep(1, 2, watched: false)
        };

        var next = EpisodeSequenceResolver.GetNextEpisode(episodes);

        Assert.Null(next);
    }

    /// <summary>
    /// An empty episode collection (e.g. a series with no standard episodes) must not
    /// throw and should simply produce no result.
    /// </summary>
    [Fact]
    public void GetNextEpisode_NoEpisodes_ReturnsNull()
    {
        var next = EpisodeSequenceResolver.GetNextEpisode(Array.Empty<EpisodeInfo>());

        Assert.Null(next);
    }
}
