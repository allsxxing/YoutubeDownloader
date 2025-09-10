using YoutubeDownloader.Core.Utils;

namespace YoutubeDownloader.Tests.Utils;

public class PathExTests
{
    [Fact]
    public void EscapeFileName_WithValidFileName_ReturnsUnchanged()
    {
        // Arrange
        var fileName = "NormalFileName.txt";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        Assert.Equal("NormalFileName.txt", result);
    }
    
    [Fact]
    public void EscapeFileName_WithForwardSlash_ReplacesWithUnderscore()
    {
        // Arrange (forward slash is invalid on Linux)
        var fileName = "Folder/SubFolder/File.txt";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        Assert.Equal("Folder_SubFolder_File.txt", result);
    }
    
    [Fact]
    public void EscapeFileName_WithNullCharacter_ReplacesWithUnderscore()
    {
        // Arrange (null character is invalid on all platforms)
        var fileName = "File\0Name.txt";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        Assert.Equal("File_Name.txt", result);
    }
    
    [Fact]
    public void EscapeFileName_WithWindowsInvalidChars_RemainsUnchangedOnLinux()
    {
        // Arrange (these are valid on Linux but invalid on Windows)
        var fileName = "File<>:\"|?*Name.txt";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        // On Linux, these characters are valid, so they should remain unchanged
        Assert.Equal("File<>:\"|?*Name.txt", result);
    }
    
    [Fact]
    public void EscapeFileName_WithEmptyString_ReturnsEmpty()
    {
        // Arrange
        var fileName = "";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        Assert.Equal("", result);
    }
    
    [Theory]
    [InlineData("Video: Title (1080p).mp4", "Video: Title (1080p).mp4")]  // Valid on Linux
    [InlineData("Song | Artist - Album.mp3", "Song | Artist - Album.mp3")]  // Valid on Linux
    [InlineData("Document*.docx", "Document*.docx")]  // Valid on Linux
    [InlineData("COM1.txt", "COM1.txt")]  // Valid filename
    [InlineData("My\"File\".pdf", "My\"File\".pdf")]  // Valid on Linux
    [InlineData("file/with/slashes.txt", "file_with_slashes.txt")]  // Forward slashes invalid on all platforms
    public void EscapeFileName_WithVariousInputs_ReturnsExpectedResults(string input, string expected)
    {
        // Act
        var result = PathEx.EscapeFileName(input);
        
        // Assert
        Assert.Equal(expected, result);
    }
    
    [Fact]
    public void EscapeFileName_PreservesLength()
    {
        // Arrange
        var fileName = "Test/File|Name?.txt";
        
        // Act
        var result = PathEx.EscapeFileName(fileName);
        
        // Assert
        Assert.Equal(fileName.Length, result.Length);
    }
}