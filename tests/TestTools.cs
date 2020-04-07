namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using System.Collections.Generic;
  using System.Text.RegularExpressions;


  internal class TestTools : ITools
  {
    //
    // Private data
    //

    private readonly ITools _tools;
    private readonly Action<string> _logFn;


    //
    // Constructor
    //

    public TestTools(ITools tools, Action<string> logFn)
    {
      _tools = tools;
      _logFn = logFn;
    }


    //
    // Public properties
    //

    public bool InJenkins => _tools.InJenkins;


    //
    // Public methods
    //

    public void ExecSubProcessCommand(string command,
                                      string args,
                                      string workingDirectory,
                                      out string stdout,
                                      out string stderr,
                                      TimeSpan timeout)
    {
      _tools.ExecSubProcessCommand(command, args, workingDirectory, out stdout, out stderr, timeout);
    }

    public bool GetMatches(string s,
                           string regexPattern,
                           IDictionary<string, string> keys,
                           RegexOptions options = RegexOptions.None)
    {
      return _tools.GetMatches(s, regexPattern, keys, options);
    }

    public void Log(string s) { _logFn(s); }
  }
}
