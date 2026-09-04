using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;
using Jellyfin.Plugin.NextUpRecentlyWatched.Configuration;

namespace Jellyfin.Plugin.NextUpRecentlyWatched;

/// <summary>
/// The Next Up (Recently Watched) plugin entry point. Registers the plugin with Jellyfin and
/// exposes its configuration page. The actual "Next Up" interception logic lives in
/// <see cref="PluginServiceRegistrator"/> and the <c>Filtering</c>/<c>Resolvers</c>
/// namespaces.
/// </summary>
public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Next Up (Recently Watched)";

    /// <inheritdoc />
    public override string Description =>
        "Selects the Next Up episode based on the most recently watched episode rather than the highest-numbered watched episode.";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("f3d1a9c2-8b7e-4c1d-9a2f-6e5d4c3b2a10");

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = Name,
            EmbeddedResourcePath = string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "{0}.Configuration.configPage.html",
                GetType().Namespace)
        };
    }
}
