namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Diagnostics;
  using System.Text.RegularExpressions;


  internal class Tools : ITools
  {
    //
    // Public static properties
    //

    public bool InJenkins => !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("JENKINS_URL"));


    //
    // Public static methods
    //

    public void ExecSubProcessCommand(string command,
                                      string args,
                                      string workingDirectory,
                                      out string stdout,
                                      out string stderr,
                                      TimeSpan timeout)
    {
      // Spawn a sub-process to run the command and capture stdout and stderr output
      using(var process = new Process()) {
        process.StartInfo = new ProcessStartInfo {
          FileName = command,
          CreateNoWindow = true,
          Arguments = args,
          UseShellExecute = false,
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          WorkingDirectory = workingDirectory
        };

        process.Start();

        // Read our streams asynchronously 
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        // Wait for the process to exit.  The command should complete almost instantaneously.  If it doesn't then
        // something is wrong and we should report that.  This is much better than just calling WaitForExit() which
        // could lead us to hang.
        if(process.WaitForExit((int)timeout.TotalMilliseconds)) {
          // Have to call WaitForExit() here in order to allow async reads on the redirected streams to complete.
          // The process has already ended so this will not block any longer than is needed for the streams to flush.
          process.WaitForExit();
          stdout = stdoutTask.Result;
          stderr = stderrTask.Result;
          return;
        }

        var commandLine = command + " " + args;
        throw new TimeoutException($"Sub-process to run '{commandLine}' took more than '{timeout}' to exit");
      }
    }

    public bool GetMatches(string s,
                           string regexPattern,
                           IDictionary<string, string> keys,
                           RegexOptions options = RegexOptions.None)
    {
      var regex = new Regex(regexPattern, options);
      var match = regex.Match(s);
      if(!match.Success) return false;

      //
      // When using a regex with named groups, e.g.
      //
      //   (?<Tag>(?<MajorVersion>\d+)\.(?<MinorVersion>\d+)\.(?<Revision>\d+))
      //
      // you will get an additional named group for the match of the entire regex.  So when the above matches you
      // will actually get the following named groups: '0', 'Tag', 'MajorVersion', 'MinorVersion' & 'Revision'.  When
      // matched against, for example, the string '1.2.3' you will get the following:
      //
      //  '0'            => '1.2.3'
      //  'Tag'          => '1.2.3'
      //  'MajorVersion' => '1'
      //  'MinorVersion' => '2'
      //  'Revision'     => '3'
      //  
      // The '0' named match is redundant for all our use cases here and we will ignore it.
      //
      foreach(var groupName in regex.GetGroupNames().Where(groupName => groupName != "0")) {
        // Override existing keys or add new ones
        if(keys.ContainsKey(groupName))
          keys[groupName] = match.Groups[groupName].Value;
        else
          keys.Add(groupName, match.Groups[groupName].Value);
      }

      return true;
    }

    public void Log(string s) { Console.WriteLine(s); }
  }
}
