namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System.IO;
  using System.Threading;
  using Xunit;
  using Xunit.Abstractions;


  public class TemporaryDirectoryTests
  {
    private readonly ITestOutputHelper _output;

    public TemporaryDirectoryTests(ITestOutputHelper output)
    {
      _output = output;
    }


    [Fact]
    public void DirectoryCreationDeletion()
    {
      string tempDirectoryPath;
      using(var tempDirectory = new TemporaryDirectory()) {
        tempDirectoryPath = tempDirectory.DirectoryPath;
        Assert.True(Directory.Exists(tempDirectoryPath));
      }
      Thread.Sleep(10);
      Assert.False(Directory.Exists(tempDirectoryPath));
    }

    [Fact]
    public void CanUseDirectory()
    {
      string tempDirectoryPath;
      using(var tempDirectory = new TemporaryDirectory()) {
        tempDirectoryPath = tempDirectory.DirectoryPath;
        var filePath = Path.Combine(tempDirectory.DirectoryPath, "foo.txt");
        var file = new FileInfo(filePath);
        using(var writer = file.CreateText()) { }
        Assert.True(File.Exists(filePath)); 
      }
      Thread.Sleep(10);
      Assert.False(Directory.Exists(tempDirectoryPath));
    }
  }
}
