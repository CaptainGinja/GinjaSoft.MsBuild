namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text.RegularExpressions;


  internal class PackageFile
  {
    //
    // Private data
    //

    private readonly FileInfo _packageFile;
    private readonly ITools _tools;
    private readonly string _packageName;
    private readonly string _version;
    private readonly bool _isPreRelease;
    private readonly bool _containsSymbols;


    //
    // Public properties
    //

    public string PackageName => _packageName;
    public string Version => _version;
    public bool IsPreRelease => _isPreRelease;
    public bool ContainsSymbols => _containsSymbols;
    public string FilePath => _packageFile.FullName;


    //
    // Constructor
    //

    public PackageFile(string packageFilePath, ITools tools = null)
    {
      _packageFile = new FileInfo(packageFilePath);
      _tools = tools ?? Singletons.Tools;
      if(!_packageFile.Exists) throw new Exception($"{packageFilePath} does not exist");
      const string extension = ".nupkg";
      if(_packageFile.Extension != extension) throw new Exception($"Package extension != '{extension}'");

      const string pattern = @"
^
(?<fileName>
  (?<packageName>.*)
  \.
  (?<version>
    (?<majorVersion>\d+)
    \.
    (?<minorVersion>\d+)
    \.
    (?<revision>\d+)
    (?<extra>
      -
      (?<preReleaseInfo>[^\.]+)
    )?
  )
  (?<symbols>\.symbols)?
  \.nupkg
)
$";
      var keys = new Dictionary<string, string>();
      if(!_tools.GetMatches(_packageFile.Name, pattern, keys, RegexOptions.IgnorePatternWhitespace))
        throw new Exception($"'{_packageFile.Name}' didn't contain expected version pattern");

      _packageName = keys["packageName"];
      _version = keys["version"];
      _isPreRelease = keys.ContainsKey("preReleaseInfo") && !string.IsNullOrEmpty(keys["preReleaseInfo"]);
      _containsSymbols = keys.ContainsKey("symbols") && !string.IsNullOrEmpty(keys["symbols"]);
    }
  }
}
