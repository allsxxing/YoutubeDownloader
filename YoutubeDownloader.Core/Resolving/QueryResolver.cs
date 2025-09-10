using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Channels;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Resolving;

public class QueryResolver(IReadOnlyList<Cookie>? initialCookies = null)
{
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);
    private readonly bool _isAuthenticated = initialCookies?.Any() == true;

    private async Task<QueryResult?> TryResolvePlaylistAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (PlaylistId.TryParse(query) is not { } playlistId)
            return null;

        // Skip personal system playlists if the user is not authenticated
        var isPersonalSystemPlaylist =
            playlistId == "WL" || playlistId == "LL" || playlistId == "LM";

        if (isPersonalSystemPlaylist && !_isAuthenticated)
            return null;

        try
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);

            return new QueryResult(QueryResultKind.Playlist, $"Playlist: {playlist.Title}", videos);
        }
        catch (Exception ex)
        {
            // Provide more specific error messages for common playlist issues
            var errorMessage = ex.Message;
            if (ex.Message.Contains("not found") || ex.Message.Contains("unavailable"))
            {
                errorMessage =
                    $"Playlist '{playlistId}' was not found or is not accessible. It may be private, deleted, or the URL may be incorrect.";
            }

            throw new InvalidOperationException(errorMessage, ex);
        }
    }

    private async Task<QueryResult?> TryResolveVideoAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (VideoId.TryParse(query) is not { } videoId)
            return null;

        var video = await _youtube.Videos.GetAsync(videoId, cancellationToken);
        return new QueryResult(QueryResultKind.Video, video.Title, [video]);
    }

    private async Task<QueryResult?> TryResolveChannelAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (ChannelId.TryParse(query) is { } channelId)
        {
            var channel = await _youtube.Channels.GetAsync(channelId, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channelId, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelHandle.TryParse(query) is { } channelHandle)
        {
            var channel = await _youtube.Channels.GetByHandleAsync(
                channelHandle,
                cancellationToken
            );

            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (UserName.TryParse(query) is { } userName)
        {
            var channel = await _youtube.Channels.GetByUserAsync(userName, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        if (ChannelSlug.TryParse(query) is { } channelSlug)
        {
            var channel = await _youtube.Channels.GetBySlugAsync(channelSlug, cancellationToken);
            var videos = await _youtube.Channels.GetUploadsAsync(channel.Id, cancellationToken);

            return new QueryResult(QueryResultKind.Channel, $"Channel: {channel.Title}", videos);
        }

        return null;
    }

    private async Task<QueryResult> ResolveSearchAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        var videos = await _youtube
            .Search.GetVideosAsync(query, cancellationToken)
            .CollectAsync(20);

        return new QueryResult(QueryResultKind.Search, $"Search: {query}", videos);
    }

    public async Task<QueryResult> ResolveAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        // If the query starts with a question mark, it's always treated as a search query
        if (query.StartsWith('?'))
            return await ResolveSearchAsync(query[1..], cancellationToken);

        return await TryResolvePlaylistAsync(query, cancellationToken)
            ?? await TryResolveVideoAsync(query, cancellationToken)
            ?? await TryResolveChannelAsync(query, cancellationToken)
            ?? await ResolveSearchAsync(query, cancellationToken);
    }
}
