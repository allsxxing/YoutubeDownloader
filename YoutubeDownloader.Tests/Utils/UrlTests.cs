using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Tests.Utils;

public class UrlTests
{
    [Fact]
    public void TryExtractFileName_WithValidUrl_ReturnsFileName()
    {
        // Arrange
        var url = "https://example.com/path/to/file.txt";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Equal("file.txt", result);
    }
    
    [Fact]
    public void TryExtractFileName_WithUrlContainingQueryParameters_ReturnsFileNameWithoutQuery()
    {
        // Arrange
        var url = "https://example.com/downloads/document.pdf?version=1&token=abc123";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Equal("document.pdf", result);
    }
    
    [Fact]
    public void TryExtractFileName_WithUrlEndingInSlash_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com/path/to/directory/";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void TryExtractFileName_WithUrlWithoutPath_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com/";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void TryExtractFileName_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var url = "";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Null(result);
    }
    
    [Theory]
    [InlineData("https://example.com/video.mp4", "video.mp4")]
    [InlineData("https://youtube.com/watch?v=dQw4w9WgXcQ", "watch")]
    [InlineData("https://site.com/folder/subfolder/image.jpg", "image.jpg")]
    [InlineData("https://domain.org/file-with-dashes.zip", "file-with-dashes.zip")]
    public void TryExtractFileName_WithVariousUrls_ReturnsExpectedFileNames(string url, string expected)
    {
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Equal(expected, result);
    }
}