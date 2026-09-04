using Jellyfin.Plugin.NextUpRecentlyWatched.Filtering;
using Jellyfin.Plugin.NextUpRecentlyWatched.Services;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.NextUpRecentlyWatched;

/// <summary>
/// Registers the Next Up (Recently Watched) plugin's services with Jellyfin's dependency
/// injection container, and wires <see cref="NextUpActionFilter"/> into the MVC
/// pipeline so it can intercept every request, regardless of which controller serves it.
/// </summary>
public sealed class PluginServiceRegistrator : IPluginServiceRegistrator
{
    /// <inheritdoc />
    public void RegisterServices(IServiceCollection serviceCollection, IServerApplicationHost applicationHost)
    {
        serviceCollection.AddSingleton<ISeriesSettingsProvider, SeriesSettingsProvider>();
        serviceCollection.AddSingleton<IEpisodeHistoryService, EpisodeHistoryService>();
        serviceCollection.AddSingleton<INextUpResolver, NextUpResolver>();
        serviceCollection.AddSingleton<NextUpActionFilter>();

        serviceCollection.Configure<MvcOptions>(options => options.Filters.AddService<NextUpActionFilter>());
    }
}
