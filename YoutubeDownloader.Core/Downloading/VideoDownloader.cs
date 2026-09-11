using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Gress;
using YoutubeDownloader.Core.Utils;
using YoutubeExplode;
using YoutubeExplode.Converter;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.ClosedCaptions;

namespace YoutubeDownloader.Core.Downloading;

public class VideoDownloader(IReadOnlyList<Cookie>? initialCookies = null)
{
    private readonly YoutubeClient _youtube = new(Http.Client, initialCookies ?? []);

    public async Task<IReadOnlyList<VideoDownloadOption>> GetDownloadOptionsAsync(
        VideoId videoId,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var manifest = await _youtube.Videos.Streams.GetManifestAsync(videoId, cancellationToken);
        return VideoDownloadOption.ResolveAll(manifest, includeLanguageSpecificAudioStreams);
    }

    public async Task<VideoDownloadOption> GetBestDownloadOptionAsync(
        VideoId videoId,
        VideoDownloadPreference preference,
        bool includeLanguageSpecificAudioStreams = true,
        CancellationToken cancellationToken = default
    )
    {
        var options = await GetDownloadOptionsAsync(
            videoId,
            includeLanguageSpecificAudioStreams,
            cancellationToken
        );

        return preference.TryGetBestOption(options)
            ?? throw new InvalidOperationException("No suitable download option found.");
    }

    public async Task DownloadVideoAsync(
        string filePath,
        IVideo video,
        VideoDownloadOption downloadOption,
        bool includeSubtitles = true,
        TimeSpan? sectionStartTime = null,
        TimeSpan? sectionEndTime = null,
        IProgress<Percentage>? progress = null,
        CancellationToken cancellationToken = default
    )
    {
        // Include subtitles in the output container
        var trackInfos = new List<ClosedCaptionTrackInfo>();
        if (includeSubtitles && !downloadOption.Container.IsAudioOnly)
        {
            var manifest = await _youtube.Videos.ClosedCaptions.GetManifestAsync(
                video.Id,
                cancellationToken
            );

            trackInfos.AddRange(manifest.Tracks);
        }

        var dirPath = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dirPath))
            Directory.CreateDirectory(dirPath);

        // If a specific section of the video was requested, download the full
        // video into a temporary file first, and then cut out the requested
        // section into the final file. This mirrors the behavior of
        // `yt-dlp --download-sections`.
        var isSectionRequested = sectionStartTime is not null || sectionEndTime is not null;

        var downloadFilePath = isSectionRequested
            ? Path.Combine(
                string.IsNullOrWhiteSpace(dirPath) ? Directory.GetCurrentDirectory() : dirPath,
                $"{Path.GetFileNameWithoutExtension(filePath)}.full{Path.GetExtension(filePath)}"
            )
            : filePath;

        // Downloading is scaled down to 90% of the total progress so that the
        // remaining 10% can be attributed to cutting out the requested section
        IProgress<Percentage>? downloadProgress = progress;
        if (isSectionRequested && progress is not null)
            downloadProgress = new ScaledProgress(progress, 0.9);

        try
        {
            var ffmpegFilePath = FFmpeg.TryGetCliFilePath() ?? "ffmpeg";

            await _youtube.Videos.DownloadAsync(
                downloadOption.StreamInfos,
                trackInfos,
                new ConversionRequestBuilder(downloadFilePath)
                    .SetFFmpegPath(ffmpegFilePath)
                    .SetContainer(downloadOption.Container)
                    .SetPreset(ConversionPreset.Medium)
                    .Build(),
                downloadProgress?.ToDoubleBased(),
                cancellationToken
            );

            if (isSectionRequested)
            {
                await CutSectionAsync(
                    ffmpegFilePath,
                    downloadFilePath,
                    filePath,
                    sectionStartTime,
                    sectionEndTime,
                    cancellationToken
                );

                progress?.Report(Percentage.FromFraction(1));
            }
        }
        finally
        {
            if (isSectionRequested)
            {
                try
                {
                    File.Delete(downloadFilePath);
                }
                catch
                {
                    // Ignore failures to clean up the temporary file
                }
            }
        }
    }

    private static async Task CutSectionAsync(
        string ffmpegFilePath,
        string sourceFilePath,
        string destinationFilePath,
        TimeSpan? startTime,
        TimeSpan? endTime,
        CancellationToken cancellationToken = default
    )
    {
        var arguments = new List<string> { "-hide_banner", "-y" };

        // Seeking before the input is faster, at the cost of only being
        // accurate up to the nearest keyframe (same trade-off yt-dlp makes
        // by default when downloading a section of a video)
        if (startTime is { } start)
        {
            arguments.Add("-ss");
            arguments.Add(start.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        }

        arguments.Add("-i");
        arguments.Add(sourceFilePath);

        if (endTime is { } end)
        {
            var duration = end - (startTime ?? TimeSpan.Zero);
            if (duration <= TimeSpan.Zero)
            {
                throw new InvalidOperationException(
                    "The end of the requested section must be after its start."
                );
            }

            arguments.Add("-t");
            arguments.Add(duration.TotalSeconds.ToString("0.###", CultureInfo.InvariantCulture));
        }

        arguments.Add("-map");
        arguments.Add("0");
        arguments.Add("-c");
        arguments.Add("copy");
        arguments.Add(destinationFilePath);

        await FFmpeg.ExecuteAsync(ffmpegFilePath, arguments, cancellationToken);
    }

    private sealed class ScaledProgress(IProgress<Percentage> target, double factor)
        : IProgress<Percentage>
    {
        public void Report(Percentage value) =>
            target.Report(Percentage.FromFraction(value.Fraction * factor));
    }
}
