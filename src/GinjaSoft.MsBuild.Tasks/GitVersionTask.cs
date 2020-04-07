namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.IO;
  using Microsoft.Build.Framework;


  //
  // SemVer 1.0 Spec (https://semver.org/spec/v1.0.0.html)
  //
  //   major_version.minor_version.revision[-pre_release_info]
  //
  //   major_version is required and must match \d+
  //   minor_version is required and must match \d+
  //   revision is required and must match \d+
  //   pre_release_info is optional but must match [a-zA-Z0-9-]+ if present
  //
  // SemVer 2.0 Spec (https://semver.org/spec/v2.0.0.html)
  //
  //   major_version.minor_version.revision[-pre_release_info][+build_metadata]
  //
  //   major_version is required and must match \d+
  //   minor_version is required and must match \d+
  //   revision is required and must match \d+
  //   pre_release_info is optinonal and is a series of dot separated tokens, each matching [a-zA-Z0-9-]+
  //   build_metadata is optional and is a series of dot separated tokens, each matching [a-zA-Z0-9-]+
  //  
  // NuGet Version Spec
  //
  //   NuGet originally only supported SemVer 1.0.  NuGet now claims to support SemVer 2.0 but although the client
  //   tool may now do so, as of today (2018-05-17) many package repositories, including nuget.org do not.  Trying to
  //   generate a package with a 2.0 version string will result in many warnings coming from the CLI.  It's fair to
  //   say that we need to stick with SemVer 1.0 for now.  There are also some differences between strict SemVer 1.0
  //   and the way that NuGet versioning works.  Let's review.
  //
  //   A pre-release NuGet package is one that includes pre_release_info in its version (as per above).  A package with
  //   pre-release info counts as a lower version than the same version without the pre-release info.  I.e. 1.2.3 is a
  //   newer version than 1.2.3-foo.  Next, NuGet imposes a 20 character limit on the length of pre-release info.
  //   Finally NuGet orders the pre-release string lexacographically so 1.2.3-rc2 is a newer version than 1.2.3-rc10.
  //   So if you want to use numbers in your pre-release info strings then you need to pad them, e.g. 1.2.3-build00013.
  //
  // Assembly Versioning
  //
  //   For a long time Microsoft has used a four part version string for Windows components ...
  //
  //   major_version.minor_version.build_number.revision
  //
  //   This carried over into .Net too.  We will not use the revision part and will model SemVer using ...
  // 
  //   major_version.minor_version.build_number.0
  //
  //   There are various properties that you can set on an Assembly when you build it ...
  //
  //   Version              - The actual version of the assembly as far as .Net sees it for version compatibility
  //                        - This is a 4 part version but we will only use the first 3 parts
  //   FileVersion          - The version of the DLL file
  //   InformationalVersion - A descriptive version string
  //
  //   If you right click on a .Net assembly DLL in Windows Explorer and then click on Properties and then the Details
  //   tab of the resulting dialog box then you will see a number of properties in a Description section.  These
  //   include ...
  //
  //   File version    - The value of the FileVersion property set on the assembly during build (shown with 4 parts)
  //   Product version - The value of the InformationalVersion property set on the assembly during build
  //
  //   The .Net assembly version is not directly visible here.  
  //
  //   You can view the .Net assembly version, along with all the other assembly attributes, using PowerShell ...
  //
  //   $assembly = [Reflection.Assembly]::ReflectionOnlyLoadFrom("<assemblyFilePath>")
  //   [Reflection.CustomAttributeData]::GetCustomAttributes($assembly)
  //

  public class GitVersionTask : TaskBase
  {
    //
    // Public properties
    //

    /// <summary>Set by the MSBuild script that calls this task to indicate the path of the Git repo</summary>
    public string RepoPath { get; set; }

    /// <summary>Output parameter for passing the Version string back to MSBuild</summary>
    [Output]
    public string Version { get; private set; }

    /// <summary>Output parameter for passing the FileVersion string back to MSBuild</summary>
    [Output]
    public string FileVersion { get; private set; }

    /// <summary>Output parameter for passing the InformationalVersion string back to MSBuild</summary>
    [Output]
    public string InformationalVersion { get; private set; }


    //
    // Public methods
    //

    public override bool Execute()
    {
      //
      // Assumptions/checks
      //  * The current working directory is a Git repo
      //  * There is a tag on the current branch
      //  * The latest tag on the current branch is of the form ...
      //      major_version.minor_version.revision(-pre_release_tag)
      //    ... where major_version, minor_version, revision and pre_release_tag (if present) match the semver spec
      //
      // Query the Git repo for ...
      //  * The latest tag on the current branch
      //  * The number of commits on the branch since the latest tag
      //  * Whether there are any changes in the working directoryof staging area that have not yet been committed
      // 

      try {
        // Default values for output properties
        Version = FileVersion = InformationalVersion = "0.0.0";

        // This will throw if RepoPath does not contain a Git repo or if the tag is not a valid SemVer 1.0 string
        var repo = new GitRepo(new DirectoryInfo(RepoPath));

        // If there is no tag on the current branch of the Git repo then return.  Default version values will be used.
        if(repo.LatestTag == null) return true;

        // RepoPath contains a Git repo and there is a current tag.  Construct our version strings ...

        // If there have been commits on the current branch since the tag or there are uncommitted changes then we
        // increment the revision to ensure that the new assembly will be considered newer than the version built from
        // the tagged commit
        var changesSinceTag = repo.CommitCountSinceTag > 0 || repo.HasUncommittedChanges;
        var revision = changesSinceTag ? repo.Revision + 1 : repo.Revision;

        Version = FileVersion = $"{repo.MajorVersion}.{repo.MinorVersion}.{revision}";

        // TODO: Reconsider how to format the pre-release string

        var localChanges = repo.HasUncommittedChanges ? "-local" : "";
        var buildInfo = changesSinceTag ? $"{repo.CurrentBranch}{localChanges}" : "";

        var repoPreReleaseInfo = repo.PreReleaseInfo ?? "";
        var buildInfoPrefix = repoPreReleaseInfo.Length > 0 ? "-" : "";
        var preReleaseInfo = $"{repoPreReleaseInfo}{buildInfoPrefix}{buildInfo}";

        var extra = preReleaseInfo.Length > 0 ? $"-{preReleaseInfo}" : "";
        InformationalVersion = $"{Version}{extra}";
      }
      catch(GitRepo.NotAGitRepoException e) {
        // Not fatal.  The build will just use the default output values for the version properties.
        LogMessage(e.ToPrettyString());
      }
      catch(Exception e) {
        // Throw.  Stop the build.
        LogMessage(e.ToPrettyString());
        throw;
      }

      return true;
    }
  }
}

