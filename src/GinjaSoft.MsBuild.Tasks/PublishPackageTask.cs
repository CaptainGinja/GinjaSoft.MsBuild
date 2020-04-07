namespace GinjaSoft.MsBuild.Tasks
{
  using System;


  public class PublishPackageTask : TaskBase
  {
    //
    // Public properties
    //

    /// <summary>Set by the MSBuild script that calls this task to indicate the path of the Git repo</summary>
    public string RepoPath { get; set; }

    /// <summary>Set by the MSBuild script that calls this task to indicate the filepath of the NuGet package</summary>
    public string PackageFilePath { get; set; }


    //
    // Public methods
    //

    public override bool Execute()
    {
      try {
        new PublishPackage(RepoPath, PackageFilePath, LogMessage).Go();
      }
      catch(Exception e) {
        LogMessage(e.ToPrettyString());
        throw;
      }
      return true;
    }
  }
}

