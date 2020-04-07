namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using Xunit;
  using Xunit.Abstractions;


  public class GitRepoTests
  {
    private readonly ITestOutputHelper _output;


    public GitRepoTests(ITestOutputHelper output)
    {
      _output = output;
      Environment.SetEnvironmentVariable("JENKINS_URL", "");
      Environment.SetEnvironmentVariable("GIT_BRANCH", "");
    }


    [Fact]
    public void BasicTag()
    {
      const uint majorVersion = 1u;
      const uint minorVersion = 2u;
      const uint revision = 3u;
      var tag = $"{majorVersion}.{minorVersion}.{revision}";

      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             Command(directoryPath, $"git tag -a {tag} -m {tag}");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Equal(tag, repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(majorVersion, repo.MajorVersion);
             Assert.Equal(minorVersion, repo.MinorVersion);
             Assert.Equal(revision, repo.Revision);
             Assert.Equal("", repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void TagWithPreReleaseInfo()
    {
      const uint majorVersion = 1u;
      const uint minorVersion = 2u;
      const uint revision = 3u;
      const string preReleaseInfo = "foo";
      var tag = $"{majorVersion}.{minorVersion}.{revision}-{preReleaseInfo}";

      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             Command(directoryPath, $"git tag -a {tag} -m {tag}");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Equal(tag, repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(majorVersion, repo.MajorVersion);
             Assert.Equal(minorVersion, repo.MinorVersion);
             Assert.Equal(revision, repo.Revision);
             Assert.Equal(preReleaseInfo, repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void TagWithSubsequentCommits()
    {
      const uint majorVersion = 1u;
      const uint minorVersion = 2u;
      const uint revision = 3u;
      var tag = $"{majorVersion}.{minorVersion}.{revision}";

      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             Command(directoryPath, $"git tag -a {tag} -m {tag}");
             file.AppendText("bar");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit2");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Equal(tag, repo.LatestTag);
             Assert.Equal(1u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(majorVersion, repo.MajorVersion);
             Assert.Equal(minorVersion, repo.MinorVersion);
             Assert.Equal(revision, repo.Revision);
             Assert.Equal("", repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void TagPlusUncommittedChanges()
    {
      const uint majorVersion = 1u;
      const uint minorVersion = 2u;
      const uint revision = 3u;
      var tag = $"{majorVersion}.{minorVersion}.{revision}";

      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             Command(directoryPath, $"git tag -a {tag} -m {tag}");
             file.AppendText("bar");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Equal(tag, repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.True(repo.HasUncommittedChanges);
             Assert.Equal(majorVersion, repo.MajorVersion);
             Assert.Equal(minorVersion, repo.MinorVersion);
             Assert.Equal(revision, repo.Revision);
             Assert.Equal("", repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void NotAGitRepo()
    {
      Assert.Throws<GitRepo.NotAGitRepoException>(() => Test((directoryPath, file) => { }, repo => { }));
    }

    [Fact]
    public void NoCommits()
    {
      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Null(repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.True(repo.HasUncommittedChanges);
             Assert.Equal(0u, repo.MajorVersion);
             Assert.Equal(0u, repo.MinorVersion);
             Assert.Equal(0u, repo.Revision);
             Assert.Null(repo.PreReleaseInfo);
             Assert.Null(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void CommitsNoTag()
    {
      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Null(repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(0u, repo.MajorVersion);
             Assert.Equal(0u, repo.MinorVersion);
             Assert.Equal(0u, repo.Revision);
             Assert.Null(repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void CommitsNoTagPlusUncommittedChanges()
    {
      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             file.AppendText("bar");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Null(repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.True(repo.HasUncommittedChanges);
             Assert.Equal(0u, repo.MajorVersion);
             Assert.Equal(0u, repo.MinorVersion);
             Assert.Equal(0u, repo.Revision);
             Assert.Null(repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void SpecificBranch()
    {
      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             Command(directoryPath, "git branch foo");
             Command(directoryPath, "git checkout foo");
           },
           repo => {
             Assert.Equal("foo", repo.CurrentBranch);
             Assert.Null(repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(0u, repo.MajorVersion);
             Assert.Equal(0u, repo.MinorVersion);
             Assert.Equal(0u, repo.Revision);
             Assert.Null(repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }

    [Fact]
    public void SimulateJenkins()
    {
      Test((directoryPath, file) => {
             Command(directoryPath, "git init");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit1");
             file.AppendText("bar");
             Command(directoryPath, "git add *");
             Command(directoryPath, "git commit -m commit2");
             Command(directoryPath, "git checkout HEAD~1");

             Environment.SetEnvironmentVariable("JENKINS_URL", "https://jenkins.ginjasoft.com/");
             Environment.SetEnvironmentVariable("GIT_BRANCH", "origin/master");
           },
           repo => {
             Assert.Equal("master", repo.CurrentBranch);
             Assert.Null(repo.LatestTag);
             Assert.Equal(0u, repo.CommitCountSinceTag);
             Assert.False(repo.HasUncommittedChanges);
             Assert.Equal(0u, repo.MajorVersion);
             Assert.Equal(0u, repo.MinorVersion);
             Assert.Equal(0u, repo.Revision);
             Assert.Null(repo.PreReleaseInfo);
             Assert.NotNull(repo.LatestCommitHash);
           });
    }


    //
    // Private methods
    //

    private static void Command(string directoryPath, string command)
    {
      var tools = Singletons.Tools;
      var timeout = TimeSpan.FromSeconds(3);
      tools.ExecSubProcessCommand("cmd.exe", $"/c {command}", directoryPath, out var stdout, out var stderr, timeout);
    }

    private void Test(Action<string, TemporaryTextFile> setup, Action<GitRepo> tests)
    {
      const string textFileBody = "foo";
      using(var directory = new TemporaryDirectory()) {
        var directoryPath = directory.DirectoryPath;
        using(var file = new TemporaryTextFile(textFileBody, "txt", directoryPath)) {
          setup(directoryPath, file);
          var tools = new TestTools(Singletons.Tools, _output.WriteLine);
          var repo = new GitRepo(directory.DirectoryInfo, tools);
          tests(repo);
        }
      }
    }
  }
}
