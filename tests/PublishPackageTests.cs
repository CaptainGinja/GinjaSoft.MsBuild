namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using System.IO;
  using Xunit;
  using Xunit.Abstractions;


  public class PublishPackageTests
  {
    private readonly ITestOutputHelper _output;


    public PublishPackageTests(ITestOutputHelper output)
    {
      _output = output;
    }


    [Fact]
    public void Test1()
    {
      const string filename = "GinjaSoft.Foo.1.2.3.nupkg";
      using(var directory = new TemporaryDirectory()) {
        var directoryPath = directory.DirectoryPath;
        var filepath = Path.Combine(directoryPath, filename);
     
        var file = new FileInfo(filepath);
        using(var fileStream = file.CreateText()) { }

        var tools = new TestTools(Singletons.Tools, _output.WriteLine);
        var publish = new PublishPackage(directoryPath, filepath, _output.WriteLine, tools);
        // TODO
        //publish.Go();
      }
    }
  }
}
