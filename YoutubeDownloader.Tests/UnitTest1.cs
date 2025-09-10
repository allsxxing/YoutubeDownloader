namespace YoutubeDownloader.Tests;

public class HelloWorldTests
{
    [Fact]
    public void HelloWorld_ReturnsExpectedMessage()
    {
        // Arrange
        var expected = "Hello, World!";
        
        // Act
        var actual = "Hello, World!";
        
        // Assert
        Assert.Equal(expected, actual);
    }
    
    [Fact]
    public void SampleMathTest_Addition_ReturnsCorrectSum()
    {
        // Arrange
        var a = 2;
        var b = 3;
        var expected = 5;
        
        // Act
        var actual = a + b;
        
        // Assert
        Assert.Equal(expected, actual);
    }
}