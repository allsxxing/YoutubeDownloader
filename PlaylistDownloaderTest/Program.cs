using System;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Playlists;

namespace PlaylistDownloaderTest;

class Program
{
    static async Task Main(string[] args)
    {
        var youtube = new YoutubeClient();
        
        // The playlist URL from the problem statement
        var playlistUrl = "https://youtube.com/playlist?list=PLVTRPfBTtSUkOryPU3E25lgbfEncWmLPq&si=kAM4YY8JsZV6DpWp";
        
        try
        {
            Console.WriteLine("Testing playlist parsing and video retrieval...");
            Console.WriteLine($"Playlist URL: {playlistUrl}");
            Console.WriteLine();
            
            // Parse playlist ID from URL
            if (PlaylistId.TryParse(playlistUrl) is not { } playlistId)
            {
                Console.WriteLine("❌ Failed to parse playlist ID from URL");
                return;
            }
            
            Console.WriteLine($"✅ Successfully parsed playlist ID: {playlistId}");
            
            // Get playlist metadata
            var playlist = await youtube.Playlists.GetAsync(playlistId);
            Console.WriteLine($"✅ Successfully retrieved playlist: {playlist.Title}");
            Console.WriteLine($"   Author: {playlist.Author?.ChannelTitle}");
            Console.WriteLine($"   Description: {playlist.Description?[..Math.Min(100, playlist.Description.Length)]}...");
            Console.WriteLine();
            
            // Get playlist videos
            Console.WriteLine("🔍 Retrieving videos from playlist...");
            var videos = youtube.Playlists.GetVideosAsync(playlistId);
            
            var videoCount = 0;
            await foreach (var video in videos)
            {
                videoCount++;
                Console.WriteLine($"   {videoCount:D2}. {video.Title} ({video.Duration})");
                
                // Limit output to first 10 videos for testing
                if (videoCount >= 10) 
                {
                    Console.WriteLine("   ... (showing first 10 videos only)");
                    break;
                }
            }
            
            Console.WriteLine();
            Console.WriteLine($"✅ Successfully retrieved {videoCount}+ videos from the playlist");
            Console.WriteLine();
            Console.WriteLine("🎉 Playlist functionality is working correctly!");
            Console.WriteLine("   The existing YoutubeDownloader application should be able to download");
            Console.WriteLine("   videos from this playlist by pasting the URL into the application.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error: {ex.Message}");
            Console.WriteLine($"   Full exception: {ex}");
        }
    }
}