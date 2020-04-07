namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;


  internal interface ITools
  {
    bool InJenkins { get; }

    void ExecSubProcessCommand(string command,
                               string args,
                               string workingDirectory,
                               out string stdout,
                               out string stderr,
                               TimeSpan timeout);

    bool GetMatches(string s,
                    string regexPattern,
                    IDictionary<string, string> keys,
                    RegexOptions options = RegexOptions.None);

    void Log(string s);
  }
}
