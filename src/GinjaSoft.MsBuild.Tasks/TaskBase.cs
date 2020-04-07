namespace GinjaSoft.MsBuild.Tasks
{
  using Microsoft.Build.Framework;
  using Microsoft.Build.Utilities;


  public abstract class TaskBase : Task
  {
    //
    // Protected methods
    //

    protected void LogMessage(string s)
    {
      // Log a message to MsBuild output
      Log.LogMessage(MessageImportance.High, s);
    }
  }
}

