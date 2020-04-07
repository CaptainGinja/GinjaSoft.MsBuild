namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using Xunit;


  public class ToolsTests
  {
    [Fact]
    public void ExecSubProcessCommand_Timeout()
    {
      //
      // Hack batch file command to wait 3 seconds.
      // There's no simple wait or sleep command for use in a Windows batch file but we can take advantage of the -n
      // switch to the ping command which will ping a specified number of times with one ping per second.
      //
      const string batFileContents = "ping 127.0.0.1 -n 3";

      using(var tempBatFile = new TemporaryTextFile(batFileContents, "bat")) {
        var filePath = tempBatFile.FilePath;
        var fileDir = tempBatFile.FileInfo.DirectoryName;
        var timeout = new TimeSpan(0, 0, 1); // 1 second timeout
        var tools = Singletons.Tools;
        Assert.Throws<TimeoutException>(
          () => tools.ExecSubProcessCommand("cmd.exe", $"/c {filePath}", fileDir, out var x, out var y, timeout)
        );
      }
    }

    [Fact]
    public void ExecSubProcessCommand()
    {
      const string batFileContents =
@"@echo off
echo This goes to stdout 2>&1
echo %cd% 2>&1
echo This goes to stderr 1>&2";

      using(var tempBatFile = new TemporaryTextFile(batFileContents, "bat")) {
        var filePath = tempBatFile.FilePath;
        var fileDir = tempBatFile.FileInfo.DirectoryName;
        var timeout = new TimeSpan(0, 0, 1); // 1 second timeout
        var tools = Singletons.Tools;
        tools.ExecSubProcessCommand("cmd.exe", $"/c {filePath}", fileDir, out var stdout, out var stderr, timeout);
        var expectedStdout = $"This goes to stdout \r\n{fileDir} \r\n";
        var expectedStderr = $"This goes to stderr \r\n";
        Assert.Equal(expectedStdout, stdout);
        Assert.Equal(expectedStderr, stderr);
      }
    }
  }
}
