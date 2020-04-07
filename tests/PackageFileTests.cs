namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using System.IO;
  using Xunit;
  using Xunit.Abstractions;


  public class PackageFileTests
  {
    private readonly ITestOutputHelper _output;


    public PackageFileTests(ITestOutputHelper output) { _output = output; }


    [Fact]
    public void ReleasePackage()
    {
      Test("GinjaSoft.Foo.1.2.3.nupkg",
           packageFile => {
             Assert.Equal("GinjaSoft.Foo", packageFile.PackageName);
             Assert.Equal("1.2.3", packageFile.Version);
             Assert.False(packageFile.IsPreRelease);
             Assert.False(packageFile.ContainsSymbols);
           });
    }

    [Fact]
    public void PreReleasePackage()
    {
      Test("GinjaSoft.Foo.1.2.3-foo.nupkg",
           packageFile => {
             Assert.Equal("GinjaSoft.Foo", packageFile.PackageName);
             Assert.Equal("1.2.3-foo", packageFile.Version);
             Assert.True(packageFile.IsPreRelease);
             Assert.False(packageFile.ContainsSymbols);
           });
    }

    [Fact]
    public void SymbolsPackage()
    {
      Test("GinjaSoft.Foo.1.2.3.symbols.nupkg",
           packageFile => {
             Assert.Equal("GinjaSoft.Foo", packageFile.PackageName);
             Assert.Equal("1.2.3", packageFile.Version);
             Assert.False(packageFile.IsPreRelease);
             Assert.True(packageFile.ContainsSymbols);
           });
    }


    private void Test(string filename, Action<PackageFile> assertFn)
    {
      using(var directory = new TemporaryDirectory()) {
        var directoryPath = directory.DirectoryPath;
        var filepath = Path.Combine(directoryPath, filename);

        var file = new FileInfo(filepath);
        using(var fileStream = file.CreateText()) { }

        var tools = new TestTools(Singletons.Tools, _output.WriteLine);
        var packageFile = new PackageFile(filepath, tools);

        assertFn(packageFile);
      }
    }
  }
}
