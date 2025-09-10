# How to Download Videos from YouTube Playlist

## Specific Example: PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq

This document provides step-by-step instructions for downloading videos from the playlist specified in the problem statement:
`https://www.youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq`

## Prerequisites

1. **YoutubeDownloader Application**: Download and install the latest version from the [releases page](https://github.com/Tyrrrz/YoutubeDownloader/releases)
2. **FFmpeg**: Required for video processing (comes bundled with most YoutubeDownloader releases)

## Step-by-Step Instructions

### Method 1: Using the GUI Application (Recommended)

1. **Launch the Application**
   - Start YoutubeDownloader on your system
   - Wait for the main interface to load

2. **Enter the Playlist URL**
   - In the query field at the top, paste the playlist URL:
     ```
     https://www.youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq
     ```
   - Alternative supported formats:
     ```
     https://youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq
     PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq
     ```

3. **Process the Playlist**
   - Click the download button or press Enter
   - The application will fetch playlist information and show a notification like:
     ```
     Found playlist with X videos
     ```

4. **Select Videos to Download**
   - A dialog will appear showing all videos in the playlist
   - By default, all videos will be pre-selected (unlike search results)
   - You can:
     - Keep all videos selected for full playlist download
     - Uncheck specific videos you don't want to download
     - Use the selection controls to select/deselect all

5. **Configure Download Settings**
   - **Quality**: Choose video quality (1080p, 720p, 480p, etc.)
   - **Format**: Select output format (MP4, WebM, MP3, OGG)
   - **Directory**: Choose where to save the downloaded files
   - **File Naming**: Videos will be automatically numbered and named:
     ```
     [Playlist Title] - 01. [Video Title].mp4
     [Playlist Title] - 02. [Video Title].mp4
     ```

6. **Start Download**
   - Click "Confirm" to begin downloading
   - The application will process videos in parallel (configurable in Settings)
   - Monitor progress in the downloads list

### Method 2: Programmatic Usage

If you want to integrate playlist downloading into your own application, you can use the core library:

```csharp
using YoutubeDownloader.Core.Resolving;
using YoutubeDownloader.Core.Downloading;

// Initialize resolver
var resolver = new QueryResolver();

// Resolve playlist
var result = await resolver.ResolveAsync("https://www.youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq");

if (result.Kind == QueryResultKind.Playlist)
{
    Console.WriteLine($"Found playlist: {result.Title}");
    Console.WriteLine($"Videos: {result.Videos.Count}");
    
    // Download each video
    var downloader = new VideoDownloader();
    foreach (var video in result.Videos)
    {
        var options = await downloader.GetDownloadOptionsAsync(video.Id);
        // Configure and download...
    }
}
```

## Supported Features

### URL Formats
The application supports various YouTube playlist URL formats:
- `https://www.youtube.com/playlist?list=PLAYLIST_ID`
- `https://youtube.com/playlist?list=PLAYLIST_ID`
- `https://youtu.be/playlist?list=PLAYLIST_ID`
- `https://www.youtube.com/watch?v=VIDEO_ID&list=PLAYLIST_ID`
- Just the playlist ID: `PLAYLIST_ID`

### Download Options
- **Video Quality**: Multiple resolution options available
- **Audio Quality**: For audio-only downloads (MP3, OGG)
- **Subtitle Embedding**: Automatic subtitle download and embedding
- **Multi-language Audio**: Support for videos with multiple audio tracks
- **Metadata Injection**: Automatic title, thumbnail, and metadata embedding

### Advanced Features
- **Parallel Downloads**: Configure how many videos download simultaneously
- **Skip Existing Files**: Resume interrupted downloads
- **Authentication**: Login support for private playlists
- **Error Handling**: Automatic retry and skip unavailable videos

## Troubleshooting

### Common Issues

**"Playlist not found or not accessible"**
- Verify the playlist URL is correct
- Check if the playlist is public
- Try accessing the playlist in a web browser first

**"No videos found"**
- The playlist might be empty
- Videos might be private or region-locked
- Check your internet connection

**Some videos fail to download**
- Some videos may be region-locked or removed
- The application will skip these and continue with others
- Check the error messages for specific details

### Performance Tips
- For large playlists (100+ videos), consider downloading in smaller batches
- Use appropriate quality settings for your storage space
- Monitor disk space during large downloads

## Validation

The repository includes a test project that validates playlist functionality:

```bash
cd PlaylistDownloaderTest
dotnet run
```

This test confirms that:
- The specific playlist URL is correctly parsed
- YoutubeExplode library compatibility
- File name sanitization works properly
- All URL format variations are supported

## Additional Resources

- [Full Playlist Documentation](./PLAYLIST_DOCUMENTATION.md)
- [Project README](./Readme.md)
- [GitHub Issues](https://github.com/Tyrrrz/YoutubeDownloader/issues) for bug reports
- [Releases Page](https://github.com/Tyrrrz/YoutubeDownloader/releases) for downloads

## Conclusion

YoutubeDownloader already provides comprehensive support for downloading videos from the specified playlist URL. No additional implementation is required - simply use the existing application with the playlist URL to download all videos from the playlist.