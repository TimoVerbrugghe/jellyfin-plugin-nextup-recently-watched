using Jellyfin.Plugin.NextUpRecentlyWatched.Filtering;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Jellyfin.Plugin.NextUpRecentlyWatched.Filtering;

/// <summary>
/// Intercepts the <c>/Shows/NextUp</c> endpoint's MVC action execution and, for each
/// returned series that the plugin is enabled for, substitutes Jellyfin's chosen episode
/// with the one computed by <see cref="INextUpResolver"/>.
/// </summary>
/// <remarks>
/// This runs as an MVC action filter (rather than middleware) so it applies uniformly
/// regardless of which controller ultimately serves the request — including requests
/// that pass through other plugins' filters first — and so it can inspect/replace the
/// strongly-typed <see cref="QueryResult{BaseItemDto}"/> before it is serialized,
/// mirroring the approach used by jellyfin-plugin-nextup-cleanup's NextUpActionFilter.
/// </remarks>
internal sealed class NextUpActionFilter : IAsyncActionFilter
{
    private readonly INextUpResolver _nextUpResolver;
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IDtoService _dtoService;

    /// <summary>
    /// Initializes a new instance of the <see cref="NextUpActionFilter"/> class.
    /// </summary>
    /// <param name="nextUpResolver">Instance of the <see cref="INextUpResolver"/> interface.</param>
    /// <param name="libraryManager">Instance of the <see cref="ILibraryManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="dtoService">Instance of the <see cref="IDtoService"/> interface.</param>
    public NextUpActionFilter(
        INextUpResolver nextUpResolver,
        ILibraryManager libraryManager,
        IUserManager userManager,
        IDtoService dtoService)
    {
        _nextUpResolver = nextUpResolver;
        _libraryManager = libraryManager;
        _userManager = userManager;
        _dtoService = dtoService;
    }

    /// <inheritdoc />
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        if (!IsNextUpEndpoint(context))
        {
            await next().ConfigureAwait(false);
            return;
        }

        var userId = GetUserId(context);
        var executed = await next().ConfigureAwait(false);

        if (userId is null)
        {
            return;
        }

        if (executed.Result is not ObjectResult { Value: QueryResult<BaseItemDto> queryResult })
        {
            return;
        }

        var user = _userManager.GetUserById(userId.Value);
        if (user is null)
        {
            return;
        }

        var items = queryResult.Items;
        var replaced = false;
        List<BaseItemDto>? mutable = null;

        for (var i = 0; i < items.Count; i++)
        {
            var currentDto = items[i];
            if (currentDto.SeriesId is not Guid seriesId)
            {
                continue;
            }

            if (_libraryManager.GetItemById(seriesId) is not Series series)
            {
                continue;
            }

            var nextEpisode = _nextUpResolver.GetNextEpisode(series, user);
            if (nextEpisode is null || nextEpisode.Id == currentDto.Id)
            {
                continue;
            }

            mutable ??= new List<BaseItemDto>(items);
            mutable[i] = _dtoService.GetBaseItemDto(nextEpisode, new DtoOptions(true), user);
            replaced = true;
        }

        if (replaced && mutable is not null)
        {
            queryResult.Items = mutable;
        }
    }

    /// <summary>
    /// Determines whether the current request targets the Next Up endpoint
    /// (<c>TvShowsController.GetNextUp</c>, routed at <c>/Shows/NextUp</c>).
    /// Classification is deliberately narrow so the filter has zero effect on
    /// unrelated requests.
    /// </summary>
    /// <remarks>
    /// The MVC <see cref="Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor.ControllerName"/>
    /// is derived from the controller's class name (minus the "Controller" suffix),
    /// which is <c>TvShows</c> for Jellyfin's <c>TvShowsController</c> — not the
    /// <c>[Route("Shows")]</c> attribute's route segment. Matching on the route
    /// segment instead of the class-derived name would never succeed.
    /// </remarks>
    private static bool IsNextUpEndpoint(ActionContext context)
    {
        var actionDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
        return actionDescriptor is not null
            && string.Equals(actionDescriptor.ControllerName, "TvShows", StringComparison.Ordinal)
            && string.Equals(actionDescriptor.ActionName, "GetNextUp", StringComparison.Ordinal);
    }

    private static Guid? GetUserId(ActionExecutingContext context)
    {
        return context.ActionArguments.TryGetValue("userId", out var value) && value is Guid guid
            ? guid
            : null;
    }
}
