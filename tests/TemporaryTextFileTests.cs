namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using System.IO;
  using Xunit;


  public class TemporaryTextFileTests
  {
    private const string BODY_TEXT = "Test text to be written to our temp file\nWith a new line or two\n";
    private const string ADDITIONAL_TEXT = "Additional\ntext";
    private const string ALT_ADDITIONAL_TEXT = "Additional\r\ntext\r\n";


    [Fact]
    public void FileCreationDeletion()
    {
      string tempFilePath;
      using(var tempFile = new TemporaryTextFile(BODY_TEXT)) {
        tempFilePath = tempFile.FilePath;
        Assert.True(File.Exists(tempFilePath));
        Assert.Equal(BODY_TEXT, tempFile.Body);
      }
      Assert.False(File.Exists(tempFilePath));
    }

    [Fact]
    public void DefaultExtension()
    {
      using(var tempFile = new TemporaryTextFile(BODY_TEXT)) {
        Assert.Equal(".tmp", tempFile.FileInfo.Extension);
      }
    }

    [Fact]
    public void CustomExtension()
    {
      const string extension = "foo";
      using(var tempFile = new TemporaryTextFile(BODY_TEXT, extension)) {
        Assert.Equal($".{extension}", tempFile.FileInfo.Extension);
      }
    }

    [Fact]
    public void CustomExtensionWithDot()
    {
      const string extension = ".foo";
      using(var tempFile = new TemporaryTextFile(BODY_TEXT, extension)) {
        Assert.Equal(extension, tempFile.FileInfo.Extension);
      }
    }

    [Fact]
    public void InvalidCustomExtension()
    {
      Assert.Throws<ArgumentException>(() => new TemporaryTextFile(BODY_TEXT, ".foo.bar"));
    }

    [Fact]
    public void AppendText()
    {
      using(var tempFile = new TemporaryTextFile(BODY_TEXT)) {
        Assert.Equal(BODY_TEXT, tempFile.Body);
        tempFile.AppendText(ADDITIONAL_TEXT);
        Assert.Equal(BODY_TEXT + ADDITIONAL_TEXT, tempFile.Body);
      }
    }

    [Fact]
    public void CarriageReturnLineFeed()
    {
      using(var tempFile = new TemporaryTextFile(BODY_TEXT)) {
        Assert.Equal(BODY_TEXT, tempFile.Body);
        tempFile.AppendText(ALT_ADDITIONAL_TEXT);
        Assert.Equal(BODY_TEXT + ALT_ADDITIONAL_TEXT, tempFile.Body);
      }
    }

    [Fact]
    public void SpecificPath()
    {
      DirectoryInfo directoryInfo = null;
      try {
        var tempPath = Path.GetTempPath();
        directoryInfo = Directory.CreateDirectory(Path.Combine(tempPath, Guid.NewGuid().ToString()));
        var path = directoryInfo.FullName;

        using(var tempFile = new TemporaryTextFile(BODY_TEXT, "tmp", path)) {
          Assert.NotNull(tempFile.FileInfo.Directory);
          Assert.Equal(path, tempFile.FileInfo.Directory.FullName);
        }
      }
      finally {
        directoryInfo?.Delete();
      }
    }
  }
}
