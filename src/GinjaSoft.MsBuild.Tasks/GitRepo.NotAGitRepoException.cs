namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.IO;


  internal partial class GitRepo
  {
    public class NotAGitRepoException : Exception
    {
      public NotAGitRepoException(DirectoryInfo dir) : base($"{dir.FullName} is not a Git repo") { }
    }
  }
}
