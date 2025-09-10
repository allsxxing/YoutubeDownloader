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

/// <summary>
/// Enhanced playlist resolver with additional validation and error handling
/// </summary>
public class PlaylistResolver(IReadOnlyList<Cookie>? initialCookies = null)
{
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);
    private readonly bool _isAuthenticated = initialCookies?.Any() == true;

    /// <summary>
    /// Validates if a URL is a valid YouTube playlist URL
    /// </summary>
    public static bool IsValidPlaylistUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;
            
        return PlaylistId.TryParse(url) != null;
    }

    /// <summary>
    /// Extracts playlist ID from various YouTube playlist URL formats
    /// </summary>
    public static string? ExtractPlaylistId(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;
            
        var playlistId = PlaylistId.TryParse(url);
        return playlistId?.Value;
    }

    /// <summary>
    /// Resolves a playlist with enhanced error handling and validation
    /// </summary>
    public async Task<PlaylistResult?> ResolvePlaylistAsync(
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
        {
            throw new InvalidOperationException(
                "Cannot access personal playlists without authentication. Please log in to your YouTube account."
            );
        }

        try
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            var videos = await _youtube.Playlists.GetVideosAsync(playlistId, cancellationToken);

            return new PlaylistResult(
                playlistId.Value,
                playlist.Title,
                playlist.Author?.ChannelTitle,
                playlist.Description,
                videos.ToList()
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to resolve playlist '{playlistId}': {ex.Message}",
                ex
            );
        }
    }

    /// <summary>
    /// Gets detailed information about a playlist without downloading videos
    /// </summary>
    public async Task<PlaylistInfo?> GetPlaylistInfoAsync(
        string query,
        CancellationToken cancellationToken = default
    )
    {
        if (PlaylistId.TryParse(query) is not { } playlistId)
            return null;

        try
        {
            var playlist = await _youtube.Playlists.GetAsync(playlistId, cancellationToken);
            return new PlaylistInfo(
                playlistId.Value,
                playlist.Title,
                playlist.Author?.ChannelTitle,
                playlist.Description,
                playlist.Thumbnails.GetWithHighestResolution()?.Url
            );
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to get playlist info for '{playlistId}': {ex.Message}",
                ex
            );
        }
    }
}

/// <summary>
/// Result of playlist resolution with videos
/// </summary>
public record PlaylistResult(
    string PlaylistId,
    string Title,
    string? ChannelTitle,
    string? Description,
    IReadOnlyList<IVideo> Videos
);

/// <summary>
/// Basic playlist information without videos
/// </summary>
public record PlaylistInfo(
    string PlaylistId,
    string Title,
    string? ChannelTitle,
    string? Description,
    string? ThumbnailUrl
);