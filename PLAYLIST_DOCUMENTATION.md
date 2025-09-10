# Playlist Download Functionality

## Overview

YoutubeDownloader fully supports downloading videos from YouTube playlists. This document explains how the playlist functionality works and how to use it.

## Supported Playlist URL Formats

The application supports various YouTube playlist URL formats:

- `https://www.youtube.com/playlist?list=PLAYLIST_ID`
- `https://youtube.com/playlist?list=PLAYLIST_ID`
- `https://youtu.be/playlist?list=PLAYLIST_ID`
- `https://www.youtube.com/watch?v=VIDEO_ID&list=PLAYLIST_ID`

Additional URL parameters (like `&si=...`, `&index=...`) are automatically ignored.

### Example Playlist URL
```
https://youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq&si=kAM4YY8JsZV6DpWp
```

## How to Download from a Playlist

### Method 1: Using the GUI Application
1. Launch YoutubeDownloader
2. Paste the playlist URL into the query field
3. Click the download button or press Enter
4. Wait for the application to retrieve playlist information
5. Select videos you want to download in the popup dialog
6. Choose output directory and quality settings
7. Click "Confirm" to start downloading

### Method 2: Multiple Playlists
You can process multiple playlists at once by entering multiple URLs (one per line) in the query field.

## Playlist Processing Workflow

When you enter a playlist URL, the application:

1. **Validates the URL** - Checks if it's a valid YouTube playlist URL
2. **Extracts Playlist ID** - Parses the playlist identifier from the URL
3. **Retrieves Metadata** - Gets playlist title, description, and author information
4. **Fetches Video List** - Downloads the complete list of videos in the playlist
5. **Shows Selection Dialog** - Displays all videos for user selection
6. **Processes Downloads** - Downloads selected videos with chosen settings

## Features

### Automatic Video Selection
- Playlists automatically pre-select all videos for download
- Search results and aggregated queries do not pre-select videos
- You can manually select/deselect specific videos

### Batch Download Settings
- **Quality**: Choose video quality (1080p, 720p, 480p, etc.)
- **Format**: Select output format (MP4, WebM, MP3, OGG)
- **Directory**: Choose where to save downloaded files
- **File Naming**: Automatic numbering and safe file name generation

### Smart File Naming
Videos from playlists are automatically numbered and named using the pattern:
```
[Playlist Title] - [##]. [Video Title].[extension]
```

Example:
```
My Favorite Songs - 01. Amazing Song Title.mp4
My Favorite Songs - 02. Another Great Song.mp4
```

### Error Handling
- **Private Playlists**: Requires authentication (login to YouTube account)
- **Unavailable Videos**: Skipped automatically with notification
- **Network Issues**: Automatic retry with error reporting
- **Invalid URLs**: Clear error messages with suggestions

## Authentication for Private Playlists

To access private playlists or personal system playlists (Watch Later, Liked Videos, etc.):

1. Click the "Settings" button in the main interface
2. Go to "Authentication" section
3. Log in with your YouTube/Google account
4. The application will save your authentication cookies

### Personal System Playlists
- **WL** - Watch Later playlist
- **LL** - Liked Videos playlist  
- **LM** - My Mix playlist

These require authentication to access.

## Advanced Features

### Parallel Downloads
- Configure how many videos download simultaneously
- Default: 2 parallel downloads
- Adjustable in Settings

### Skip Existing Files
- Option to skip files that already exist
- Useful for resuming interrupted playlist downloads
- Configurable in Settings

### Subtitle and Audio Track Injection
- Automatic subtitle embedding
- Multi-language audio track support
- Configurable in Settings

## Troubleshooting

### Common Issues

**"Playlist not found or not accessible"**
- Check if the playlist URL is correct
- Verify the playlist is public or you're authenticated
- Try refreshing the playlist page in a browser first

**"No videos found"**
- The playlist might be empty
- All videos might be private or unavailable
- Check your internet connection

**Download fails for some videos**
- Some videos may be region-locked
- Videos might have been deleted or made private
- The application will skip these and continue with others

### Performance Tips

- For large playlists (100+ videos), consider downloading in smaller batches
- Use MP3 format for audio-only content to save bandwidth
- Choose appropriate quality settings for your storage space

## Technical Implementation

The playlist functionality is implemented using:

- **YoutubeExplode** library for YouTube API interaction
- **QueryResolver** for URL parsing and playlist resolution
- **PlaylistResolver** for enhanced playlist handling
- **DownloadMultipleSetupViewModel** for user interface
- **VideoDownloader** for actual file downloading

## Code Examples

### Validating a Playlist URL
```csharp
var isValid = PlaylistUtils.ValidatePlaylistUrl(url);
if (isValid.IsValid)
{
    Console.WriteLine($"Valid playlist ID: {isValid.PlaylistId}");
}
```

### Getting Playlist Information
```csharp
var resolver = new PlaylistResolver();
var playlist = await resolver.ResolvePlaylistAsync(url);
Console.WriteLine($"Playlist: {playlist.Title} ({playlist.Videos.Count} videos)");
```

## Limitations

- YouTube API rate limits may affect large playlist processing
- Some videos may not be available in all regions
- Private videos in public playlists cannot be accessed without proper permissions
- Live streams have special handling and may have different download behavior

## Support

If you encounter issues with playlist downloads:

1. Check that you're using the latest version of YoutubeDownloader
2. Verify the playlist URL works in a web browser
3. Try downloading a single video first to test your setup
4. Check the application logs for detailed error information
5. Report issues on the project's GitHub repository