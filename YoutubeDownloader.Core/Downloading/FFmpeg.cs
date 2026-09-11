using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace YoutubeDownloader.Core.Downloading;

public static class FFmpeg
{
    private static string CliFileName { get; } =
        OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    public static string? TryGetCliFilePath()
    {
        static IEnumerable<string> GetProbeDirectoryPaths()
        {
            yield return AppContext.BaseDirectory;
            yield return Directory.GetCurrentDirectory();

            // Process PATH
            if (
                Environment.GetEnvironmentVariable("PATH")?.Split(Path.PathSeparator) is
                { } processPaths
            )
            {
                foreach (var path in processPaths)
                    yield return path;
            }

            // Registry-based PATH variables
            if (OperatingSystem.IsWindows())
            {
                // User PATH
                if (
                    Environment
                        .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User)
                        ?.Split(Path.PathSeparator) is
                    { } userPaths
                )
                {
                    foreach (var path in userPaths)
                        yield return path;
                }

                // System PATH
                if (
                    Environment
                        .GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine)
                        ?.Split(Path.PathSeparator) is
                    { } systemPaths
                )
                {
                    foreach (var path in systemPaths)
                        yield return path;
                }
            }
        }

        return GetProbeDirectoryPaths()
            .Distinct(StringComparer.Ordinal)
            .Select(dirPath => Path.Combine(dirPath, CliFileName))
            .FirstOrDefault(File.Exists);
    }

    public static bool IsBundled() =>
        File.Exists(Path.Combine(AppContext.BaseDirectory, CliFileName));

    public static bool IsAvailable() => !string.IsNullOrWhiteSpace(TryGetCliFilePath());

    public static async Task ExecuteAsync(
        string ffmpegPath,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default
    )
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo(ffmpegPath)
            {
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
        };

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        process.Start();

        var stderrTask = process.StandardError.ReadToEndAsync(cancellationToken);
        var stdoutTask = process.StandardOutput.ReadToEndAsync(cancellationToken);

        using var _ = cancellationToken.Register(() =>
        {
            try
            {
                if (!process.HasExited)
                    process.Kill(true);
            }
            catch
            {
                // The process may have already exited by the time we try to kill it
            }
        });

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            var stderr = await stderrTask;
            throw new InvalidOperationException(
                $"FFmpeg exited with a non-zero exit code ({process.ExitCode}).{Environment.NewLine}{stderr}"
            );
        }

        // Ensure the tasks are observed even on the success path
        await Task.WhenAll(stdoutTask, stderrTask);
    }
}
