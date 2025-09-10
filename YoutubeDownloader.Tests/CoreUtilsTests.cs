using YoutubeDownloader.Core.Utils;
using YoutubeDownloader.Core.Utils.Extensions;
using Xunit;

namespace YoutubeDownloader.Tests;

public class PathExTests
{
    [Fact]
    public void EscapeFileName_WithValidFileName_ReturnsUnchanged()
    {
        // Arrange
        var validFileName = "video.mp4";
        
        // Act
        var result = PathEx.EscapeFileName(validFileName);
        
        // Assert
        Assert.Equal("video.mp4", result);
    }
    
    [Fact]
    public void EscapeFileName_WithInvalidChars_ReplacesWithUnderscore()
    {
        // Arrange  
        var invalidFileName = "video\0/test.mp4"; // null char and slash are invalid on Linux
        
        // Act
        var result = PathEx.EscapeFileName(invalidFileName);
        
        // Assert
        Assert.Equal("video__test.mp4", result);
    }
    
    [Fact]
    public void EscapeFileName_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var emptyFileName = "";
        
        // Act
        var result = PathEx.EscapeFileName(emptyFileName);
        
        // Assert
        Assert.Equal("", result);
    }
}

public class UrlTests
{
    [Fact]
    public void TryExtractFileName_WithValidUrl_ReturnsFileName()
    {
        // Arrange
        var url = "https://example.com/path/to/file.mp4";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Equal("file.mp4", result);
    }
    
    [Fact]
    public void TryExtractFileName_WithUrlWithQueryParams_ReturnsFileNameWithoutParams()
    {
        // Arrange
        var url = "https://example.com/path/to/file.mp4?param=value";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Equal("file.mp4", result);
    }
    
    [Fact]
    public void TryExtractFileName_WithUrlEndingInSlash_ReturnsNull()
    {
        // Arrange
        var url = "https://example.com/path/to/";
        
        // Act
        var result = Url.TryExtractFileName(url);
        
        // Assert
        Assert.Null(result);
    }
}

public class StringExtensionsTests
{
    [Fact]
    public void NullIfEmptyOrWhiteSpace_WithValidString_ReturnsString()
    {
        // Arrange
        var validString = "test";
        
        // Act
        var result = validString.NullIfEmptyOrWhiteSpace();
        
        // Assert
        Assert.Equal("test", result);
    }
    
    [Fact]
    public void NullIfEmptyOrWhiteSpace_WithEmptyString_ReturnsNull()
    {
        // Arrange
        var emptyString = "";
        
        // Act
        var result = emptyString.NullIfEmptyOrWhiteSpace();
        
        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public void NullIfEmptyOrWhiteSpace_WithWhiteSpaceString_ReturnsNull()
    {
        // Arrange
        var whiteSpaceString = "   ";
        
        // Act
        var result = whiteSpaceString.NullIfEmptyOrWhiteSpace();
        
        // Assert
        Assert.Null(result);
    }
}
