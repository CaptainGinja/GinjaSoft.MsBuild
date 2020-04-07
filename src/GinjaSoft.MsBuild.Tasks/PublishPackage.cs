namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Text.RegularExpressions;


  internal class PublishPackage
  {
    //
    // Private data
    //

    private readonly string _repoPath;
    private readonly string _packageFilePath;
    private readonly Action<string> _logMessageFn;
    private readonly ITools _tools;
    private readonly PackageFile _packageFile;
    private readonly bool _debug;


    //
    // Public properties
    //

    /// <summary>Set by the MSBuild script that calls this task to indicate the path of the Git repo</summary>
    public string RepoPath { get; set; }

    /// <summary>Set by the MSBuild script that calls this task to indicate the filepath of the NuGet package</summary>
    public string PackageFilePath { get; set; }


    //
    // Constructor
    //
    public PublishPackage(string repoPath,
      string packageFilePath,
      Action<string> logMessageFn,
      ITools tools = null,
      bool debug = false)
    {
      _repoPath = repoPath;
      _packageFilePath = packageFilePath;
      _logMessageFn = logMessageFn;
      _packageFile = new PackageFile(_packageFilePath);
      _tools = tools ?? Singletons.Tools;
      _debug = debug;
    }


    //
    // Public methods
    //

    public bool Go()
    {
      try {
        if(_packageFile.IsPreRelease) {
          Log("We do not publish pre-release packages");
          return true;
        }
        if(PackageAlreadyPublished()) {
          Log("The version we are attempting to publish is already there");
          return true;
        }
        ActuallyPublishPackage();
      }
      catch(Exception e) {
        Log(e.ToPrettyString());
        throw;
      }

      return true;
    }


    //
    // Private methods
    //

    private void ActuallyPublishPackage()
    {
      ExecNuGetPush(out var stdout, out var stderr);
      var output = FormatCommandOutput(stdout, stderr);
      Log($"nuget push output:\n{output}");

      if(!string.IsNullOrEmpty(stderr)) throw new Exception("Error executing 'nuget push'");
    }

    private bool PackageAlreadyPublished()
    {
      ExecNuGetList(out var stdout, out var stderr);
      var output = FormatCommandOutput(stdout, stderr);
      Log($"nuget list output:\n{output}");

      if(!string.IsNullOrEmpty(stderr)) throw new Exception("Error executing 'nuget list'");

      var lines = stdout.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
      foreach(var line in lines) {
        if(Regex.IsMatch(line, $@"\s+{_packageFile.PackageName}\s+{_packageFile.Version}")) {
          return true;
        }
      }

      return false;
    }

    private void ExecNuGetList(out string stdout, out string stderr)
    {
      const string command = "cmd.exe";
      var args = $"/c nuget list -allversions -Source ToDo {_packageFile.PackageName}";
      var dir = RepoPath;
      _tools.ExecSubProcessCommand(command, args, dir, out stdout, out stderr, new TimeSpan(0, 0, 3));
      stdout = stdout.TrimEnd('\r', '\n');
    }

    private void ExecNuGetPush(out string stdout, out string stderr)
    {
      const string command = "cmd.exe";
      var args = $"/c nuget push {_packageFile.FilePath} -Source ToDo";
      var dir = RepoPath;
      _tools.ExecSubProcessCommand(command, args, dir, out stdout, out stderr, new TimeSpan(0, 0, 3));
      stdout = stdout.TrimEnd('\r', '\n');
    }

    private static string FormatCommandOutput(string stdout, string stderr)
    {
      return $"STDOUT>>{stdout}<<STDOUT\nSTDERR>>{stderr}<<STDERR";
    }

    private void Log(string s) { _logMessageFn(s); }
  }
}

