namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.IO;
  using System.Text.RegularExpressions;


  internal partial class GitRepo
  {
    private class Helper : IDisposable
    {
      //
      // Private data
      //

      private readonly DirectoryInfo _folder;
      private readonly ITools _tools;
      private readonly bool _debug;
      private readonly Dictionary<string, string> _keys;

      private string _currentBranch;
      private string _description;
      private string _latestTag;
      private uint _commitCountSinceTag;
      private string _latestCommitHash;
      private uint _majorVersion;
      private uint _minorVersion;
      private uint _revision;
      private string _preReleaseInfo;
      private bool _hasUncommittedChanges;
      

      //
      // Constructor
      //

      public Helper(GitRepo repo)
      {
        _folder = repo._folder;
        _tools = repo._tools;
        _debug = repo._debug;
        _keys = new Dictionary<string, string>();
        Help();
      }


      //
      // Public properties
      //

      public string CurrentBranch => _currentBranch;
      public string Description => _description;
      public string LatestTag => _latestTag;
      public uint CommitCountSinceTag => _commitCountSinceTag;
      public string LatestCommitHash => _latestCommitHash;
      public uint MajorVersion => _majorVersion;
      public uint MinorVersion => _minorVersion;
      public uint Revision => _revision;
      public string PreReleaseInfo => _preReleaseInfo;
      public bool HasUnCommittedChanges => _hasUncommittedChanges;


      //
      // Public methods
      //

      public void Dispose() { }


      //
      // Private methods
      //

      private void Help()
      {
        EnsureGitRepo();
        GetGitDescribeInfo();
        GetGitStatusInfo();
        GetGitLogInfo();
      }

      //
      // _folder must contain a Git repo
      //
      private void EnsureGitRepo()
      {
        if(!Directory.Exists(Path.Combine(_folder.FullName, ".git"))) throw new NotAGitRepoException(_folder);
      }

      //
      // Parse info from 'git describe'
      //
      private void GetGitDescribeInfo()
      {
        Log("Get tag info via 'git describe' ...");

        ExecGitDescribe(out var stdout, out var stderr);

        var output = FormatCommandOutput(stdout, stderr);
        Log($"git describe output:\n{output}");

        if(!string.IsNullOrEmpty(stderr)) return;

        //
        // Example strings to match:
        //   1.2.3                   // HEAD has tag 1.2.3 (maj.min.rev)
        //   1.2.3-2-g3abf56c        // HEAD is two commits after commit with tag 1.2.3 and its hash is 3abf56c
        //   1.2.3-alpha             // HEAD has tag 1.2.3-alpha (maj.min.rev-name)
        //   1.2.3-alpha-3-g3a4f345  // HEAD is two commits after commit with tag 1.2.3-alpha and its hash is 3a4f345
        //
        const string pattern = @"
^
(?<description>
  (?<tag>
    (?<majorVersion>\d+)
    \.
    (?<minorVersion>\d+)
    \.
    (?<revision>\d+)
    (?<extra1>
      -
      (?<preReleaseInfo>[^-]+)
    )?
  )
  (?<extra2>
    -
    (?<commitCountSinceTag>\d+)
    -g
    (?<latestCommitHash>[a-z0-9]+)
  )?
)
$";

        if(!_tools.GetMatches(stdout, pattern, _keys, RegexOptions.IgnorePatternWhitespace))
          throw new Exception($"'git describe' output didn't match pattern '{pattern}'");

        _description = _keys["description"];
        _latestTag = _keys["tag"];
        _majorVersion = uint.Parse(_keys["majorVersion"]);
        _minorVersion = uint.Parse(_keys["minorVersion"]);
        _revision = uint.Parse(_keys["revision"]);
        _preReleaseInfo = string.IsNullOrEmpty(_keys["preReleaseInfo"]) ? "" : _keys["preReleaseInfo"];
        var count = _keys["commitCountSinceTag"];
        _commitCountSinceTag = string.IsNullOrEmpty(count) ? 0 : uint.Parse(count);
        _keys["_commitCountSinceTag"] = _commitCountSinceTag.ToString();
        _latestCommitHash = _keys["latestCommitHash"];

        Log("... Done");
      }

      //
      // Parse info from 'git status'
      //
      private void GetGitStatusInfo()
      {
        //
        // When a project is being built by Jenkins from source obtained from GitLab by the default Jenkins Git plugin
        // then the plugin executes git commands ...
        //
        //   a) to determine the actual commit hash of the most recent commit on the specified branch (specified in the
        //      config of the Jenkins job) and then
        //   b) to specifically checkout that commit via: git checkout -f <commit_hash>
        //
        // This results in a situation where the HEAD commit of the local Git repo is detached and thus we are not
        // really on any branch.  As a result, calls to commands like 'git status' and 'git branch' will not return
        // valid output and the above code (that expects a certain format of output from 'git status') will fail.
        //
        // So it seems that we don't have a way to determine what branch is being built.  Jenkins gives us a way out
        // though since it sets a number of environment variables to give information about the build environment and
        // several of these are related to git, including GIT_BRANCH which will contain a string of the form ...
        //
        //     origin/<branch_name>
        //
        // So we can check for that and use it to set the branch.
        //

        if(_tools.InJenkins) {
          GetGitStatusInfoInJenkins();
          return;
        }

        // Local build ...
        Log("Local build.  Get branch info via 'git status' ...");
        ExecGitStatus(out var stdout, out var stderr);

        var output = FormatCommandOutput(stdout, stderr);
        Log($"git status output:\n{output}");

        if(!string.IsNullOrEmpty(stderr)) throw new Exception("Error executing 'git status'");

        var pattern = @"^On branch (?<branch>\w+)";
        if(!_tools.GetMatches(stdout, pattern, _keys))
          throw new Exception($"'git status' output didn't match pattern '{pattern}'");

        _currentBranch = _keys["branch"];

        _hasUncommittedChanges = false;
        pattern = @"(Changes to be committed)|(Changes not staged for commit)|(Untracked files)";
        if(_tools.GetMatches(stdout, pattern, _keys)) _hasUncommittedChanges = true;

        Log("... Done");
      }

      //
      // Parse info from 'git log'
      //
      private void GetGitLogInfo()
      {
        Log("Get commit info via 'git log' ...");

        ExecGitLog(out var stdout, out var stderr);

        var output = FormatCommandOutput(stdout, stderr);
        Log($"git log output:\n{output}");

        if(!string.IsNullOrEmpty(stderr)) return;

        const string pattern = @"^commit (?<commit>[a-z0-9]+)";

        if(!_tools.GetMatches(stdout, pattern, _keys))
          throw new Exception($"'git log' output didn't match pattern '{pattern}'");

        _latestCommitHash = _keys["commit"];

        Log("... Done");
      }

      //
      // Special handling for when we are running as part of a build in Jenkins
      //
      private void GetGitStatusInfoInJenkins()
      {
        Log("We are running in Jenkins.  Getting branch info from environment ...");

        const string gitBranchEnvKey = "GIT_BRANCH";
        var remoteBranch = Environment.GetEnvironmentVariable(gitBranchEnvKey) ?? "";

        const string gitBranchPattern = @"^origin/(?<branch>\w+)";
        if(!_tools.GetMatches(remoteBranch, gitBranchPattern, _keys)) {
          const string template = "ENV['{0}'] output ('{1}') didn't match pattern '{2}'";
          var message = string.Format(template, gitBranchEnvKey, remoteBranch, gitBranchPattern);
          throw new Exception(message);
        }
        _currentBranch = _keys["branch"];
        Log("... Done");
      }

      private void ExecGitStatus(out string stdout, out string stderr)
      {
        const string command = "cmd.exe";
        const string args = "/c git status";
        var dir = _folder.FullName;
        _tools.ExecSubProcessCommand(command, args, dir, out stdout, out stderr, new TimeSpan(0, 0, 3));
        stdout = stdout.TrimEnd('\r', '\n');
      }

      private void ExecGitDescribe(out string stdout, out string stderr)
      {
        const string command = "cmd.exe";
        const string args = "/c git describe --tags";
        var dir = _folder.FullName;
        _tools.ExecSubProcessCommand(command, args, dir, out stdout, out stderr, new TimeSpan(0, 0, 3));
        stdout = stdout.TrimEnd('\r', '\n');
      }

      private void ExecGitLog(out string stdout, out string stderr)
      {
        const string command = "cmd.exe";
        const string args = "/c git log --abbrev-commit";
        var dir = _folder.FullName;
        _tools.ExecSubProcessCommand(command, args, dir, out stdout, out stderr, new TimeSpan(0, 0, 3));
        stdout = stdout.TrimEnd('\r', '\n');
      }

      private void Log(string s) { if(_debug) _tools.Log(s); }

      private static string FormatCommandOutput(string stdout, string stderr)
      {
        return $"STDOUT>>{stdout}<<STDOUT\nSTDERR>>{stderr}<<STDERR";
      }
    }
  }
}
