using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using YoutubeDownloader.Core.Downloading;
using YoutubeDownloader.Framework;
using YoutubeDownloader.Services;
using YoutubeDownloader.Utils;
using YoutubeDownloader.Utils.Extensions;
using YoutubeDownloader.ViewModels.Components;
using YoutubeExplode.Videos;

namespace YoutubeDownloader.ViewModels.Dialogs;

public partial class DownloadSingleSetupViewModel(
    ViewModelManager viewModelManager,
    DialogManager dialogManager,
    SettingsService settingsService,
    SnackbarManager snackbarManager
) : DialogViewModelBase<DownloadViewModel>
{
    [ObservableProperty]
    public partial IVideo? Video { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<VideoDownloadOption>? AvailableDownloadOptions { get; set; }

    [ObservableProperty]
    public partial VideoDownloadOption? SelectedDownloadOption { get; set; }

    [ObservableProperty]
    public partial string? SectionStartTime { get; set; }

    [ObservableProperty]
    public partial string? SectionEndTime { get; set; }

    [RelayCommand]
    private void Initialize()
    {
        SelectedDownloadOption = AvailableDownloadOptions?.FirstOrDefault(o =>
            o.Container == settingsService.LastContainer
        );
    }

    [RelayCommand]
    private async Task CopyTitleAsync()
    {
        if (Application.Current?.ApplicationLifetime?.TryGetTopLevel()?.Clipboard is { } clipboard)
            await clipboard.SetTextAsync(Video?.Title);
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Video is null || SelectedDownloadOption is null)
            return;

        TimeSpan? sectionStartTime = null;
        if (!string.IsNullOrWhiteSpace(SectionStartTime))
        {
            if (!TryParseSectionTime(SectionStartTime, out var parsed))
            {
                snackbarManager.Notify(
                    "Invalid start time. Please use hh:mm:ss, mm:ss, or a number of seconds."
                );

                return;
            }

            sectionStartTime = parsed;
        }

        TimeSpan? sectionEndTime = null;
        if (!string.IsNullOrWhiteSpace(SectionEndTime))
        {
            if (!TryParseSectionTime(SectionEndTime, out var parsed))
            {
                snackbarManager.Notify(
                    "Invalid end time. Please use hh:mm:ss, mm:ss, or a number of seconds."
                );

                return;
            }

            sectionEndTime = parsed;
        }

        if (
            sectionStartTime is not null
            && sectionEndTime is not null
            && sectionEndTime <= sectionStartTime
        )
        {
            snackbarManager.Notify("The section end time must be after the start time.");
            return;
        }

        var container = SelectedDownloadOption.Container;

        var filePath = await dialogManager.PromptSaveFilePathAsync(
            [
                new FilePickerFileType($"{container.Name} file")
                {
                    Patterns = [$"*.{container.Name}"],
                },
            ],
            FileNameTemplate.Apply(settingsService.FileNameTemplate, Video, container)
        );

        if (string.IsNullOrWhiteSpace(filePath))
            return;

        // Download does not start immediately, so lock in the file path to avoid conflicts
        DirectoryEx.CreateDirectoryForFile(filePath);
        await File.WriteAllBytesAsync(filePath, []);

        settingsService.LastContainer = container;

        Close(
            viewModelManager.CreateDownloadViewModel(
                Video,
                SelectedDownloadOption,
                filePath,
                sectionStartTime,
                sectionEndTime
            )
        );
    }

    // Accepts "hh:mm:ss", "mm:ss", or a plain number of seconds
    private static bool TryParseSectionTime(string? text, out TimeSpan result)
    {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        var trimmedText = text.Trim();

        // Plain number of seconds
        if (
            double.TryParse(
                trimmedText,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out var totalSeconds
            )
        )
        {
            if (totalSeconds < 0)
                return false;

            result = TimeSpan.FromSeconds(totalSeconds);
            return true;
        }

        // Colon-separated components, most significant first: "hh:mm:ss" or "mm:ss"
        var components = trimmedText.Split(':');
        if (components.Length is not (2 or 3))
            return false;

        var numbers = new double[components.Length];
        for (var i = 0; i < components.Length; i++)
        {
            if (
                !double.TryParse(
                    components[i],
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out numbers[i]
                )
                || numbers[i] < 0
            )
                return false;
        }

        result =
            components.Length == 3
                ? TimeSpan.FromHours(numbers[0])
                    + TimeSpan.FromMinutes(numbers[1])
                    + TimeSpan.FromSeconds(numbers[2])
                : TimeSpan.FromMinutes(numbers[0]) + TimeSpan.FromSeconds(numbers[1]);

        return true;
    }
}
