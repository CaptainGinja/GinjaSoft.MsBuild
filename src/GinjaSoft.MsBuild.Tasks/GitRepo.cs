namespace GinjaSoft.MsBuild.Tasks
{
  using System.IO;


  internal partial class GitRepo
  {
    //
    // Private readonly data
    //

    private readonly DirectoryInfo _folder;
    private readonly ITools _tools;
    private readonly bool _debug;

    private readonly string _currentBranch;
    private readonly string _latestTag;
    private readonly bool _hasUncommittedChanges;
    private readonly uint _commitCountSinceTag;
    private readonly string _latestCommitHash;
    private readonly uint _majorVersion;
    private readonly uint _minorVersion;
    private readonly uint _revision;
    private readonly string _preReleaseInfo;


    //
    // Constructor
    //

    public GitRepo(DirectoryInfo folder, ITools tools = null, bool debug = false)
    {
      _folder = folder;
      _tools = tools ?? Singletons.Tools;
      _debug = debug;

      using(var helper = new Helper(this)) {
        _currentBranch = helper.CurrentBranch;
        _latestTag = helper.LatestTag;
        _hasUncommittedChanges = helper.HasUnCommittedChanges;
        _commitCountSinceTag = helper.CommitCountSinceTag;
        _latestCommitHash = helper.LatestCommitHash;
        _majorVersion = helper.MajorVersion;
        _minorVersion = helper.MinorVersion;
        _revision = helper.Revision;
        _preReleaseInfo = helper.PreReleaseInfo;
      }
    }


    //
    // Public properties
    //

    public DirectoryInfo Folder => _folder;
    public string CurrentBranch => _currentBranch;
    public string LatestTag => _latestTag;
    public bool HasUncommittedChanges => _hasUncommittedChanges;
    public uint CommitCountSinceTag => _commitCountSinceTag;
    public string LatestCommitHash => _latestCommitHash;
    public uint MajorVersion => _majorVersion;
    public uint MinorVersion => _minorVersion;
    public uint Revision => _revision;
    public string PreReleaseInfo => _preReleaseInfo;
  }
}
