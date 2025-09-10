using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using YoutubeDownloader.Core.Resolving;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.Core.Utils;

/// <summary>
/// Utility class for playlist-specific operations and batch downloading
/// </summary>
public static class PlaylistUtils
{
    /// <summary>
    /// Validates multiple playlist URLs and returns validation results
    /// </summary>
    public static IEnumerable<PlaylistValidationResult> ValidatePlaylistUrls(
        IEnumerable<string> urls
    )
    {
        foreach (var url in urls)
        {
            yield return ValidatePlaylistUrl(url);
        }
    }

    /// <summary>
    /// Validates a single playlist URL
    /// </summary>
    public static PlaylistValidationResult ValidatePlaylistUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return new PlaylistValidationResult(url, false, "URL is empty or null");
        }

        var playlistId = PlaylistResolver.ExtractPlaylistId(url);
        if (playlistId == null)
        {
            return new PlaylistValidationResult(url, false, "Invalid playlist URL format");
        }

        return new PlaylistValidationResult(url, true, null, playlistId);
    }

    /// <summary>
    /// Generates safe file names for playlist videos with numbering
    /// </summary>
    public static string GeneratePlaylistVideoFileName(
        IVideo video,
        int index,
        int totalCount,
        string extension = "mp4",
        string? playlistTitle = null
    )
    {
        var paddedIndex = index.ToString().PadLeft(totalCount.ToString().Length, '0');
        var safeVideoTitle = SanitizeFileName(video.Title);
        var safePlaylistTitle = !string.IsNullOrEmpty(playlistTitle)
            ? SanitizeFileName(playlistTitle)
            : "Playlist";

        return $"{safePlaylistTitle} - {paddedIndex}. {safeVideoTitle}.{extension}";
    }

    /// <summary>
    /// Sanitizes a string to be safe for use as a file name
    /// </summary>
    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "Untitled";

        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = string.Join(
            "_",
            fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries)
        );

        // Trim and limit length
        sanitized = sanitized.Trim().Substring(0, Math.Min(sanitized.Length, 200));

        return string.IsNullOrWhiteSpace(sanitized) ? "Untitled" : sanitized;
    }

    /// <summary>
    /// Groups videos by duration for batch processing
    /// </summary>
    public static IEnumerable<IGrouping<string, IVideo>> GroupVideosByDuration(
        IEnumerable<IVideo> videos
    )
    {
        return videos.GroupBy(v =>
            v.Duration switch
            {
                null => "Live/Unknown",
                var d when d.Value.TotalMinutes < 5 => "Short (< 5 min)",
                var d when d.Value.TotalMinutes < 15 => "Medium (5-15 min)",
                var d when d.Value.TotalMinutes < 60 => "Long (15-60 min)",
                _ => "Very Long (> 1 hour)",
            }
        );
    }

    /// <summary>
    /// Estimates total download size based on video count and average quality
    /// </summary>
    public static string EstimateDownloadSize(int videoCount, string quality = "720p")
    {
        var averageSizeMB = quality switch
        {
            "1080p" => 100,
            "720p" => 50,
            "480p" => 25,
            "360p" => 15,
            _ => 50,
        };

        var totalSizeGB = (videoCount * averageSizeMB) / 1024.0;

        return totalSizeGB < 1 ? $"{videoCount * averageSizeMB:F0} MB" : $"{totalSizeGB:F1} GB";
    }

    /// <summary>
    /// Filters videos based on duration criteria
    /// </summary>
    public static IEnumerable<IVideo> FilterVideosByDuration(
        IEnumerable<IVideo> videos,
        TimeSpan? minDuration = null,
        TimeSpan? maxDuration = null
    )
    {
        return videos.Where(video =>
        {
            if (video.Duration == null)
                return true; // Include live streams and unknown durations

            if (minDuration.HasValue && video.Duration < minDuration.Value)
                return false;

            if (maxDuration.HasValue && video.Duration > maxDuration.Value)
                return false;

            return true;
        });
    }

    /// <summary>
    /// Creates a summary report for a playlist
    /// </summary>
    public static string CreatePlaylistSummary(string playlistTitle, IReadOnlyList<IVideo> videos)
    {
        var totalDuration = videos
            .Where(v => v.Duration.HasValue)
            .Sum(v => v.Duration!.Value.TotalMinutes);

        var durationGroups = GroupVideosByDuration(videos);

        var summary = $"""
            Playlist: {playlistTitle}
            Total Videos: {videos.Count}
            Total Duration: {totalDuration / 60:F1} hours ({totalDuration:F0} minutes)

            Duration Breakdown:
            """;

        foreach (var group in durationGroups)
        {
            summary += $"  {group.Key}: {group.Count()} videos\n";
        }

        return summary;
    }
}

/// <summary>
/// Result of playlist URL validation
/// </summary>
public record PlaylistValidationResult(
    string Url,
    bool IsValid,
    string? ErrorMessage,
    string? PlaylistId = null
);
