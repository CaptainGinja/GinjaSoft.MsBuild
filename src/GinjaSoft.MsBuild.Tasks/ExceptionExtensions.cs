namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Text;
  using System.Text.RegularExpressions;


  internal static class ExceptionExtensions
  {
    //
    // Public methods
    //

    public static string ToPrettyString(this Exception e)
    {
      return ExceptionToPrettyString(e, 0);
    }


    //
    // Private methods
    //

    private static string ExceptionToPrettyString(Exception e, int level)
    {
      var builder = new StringBuilder();

      var padding = new string(' ', level * 2);
      builder.Append($"{padding}{e.GetType()}: {e.Message}");
      if(e.StackTrace != null) {
        builder.AppendLine();
        var firstLine = true;
        var regex = new Regex(@" in .*");
        foreach(var line in e.StackTrace.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None)) {
          if(!firstLine) builder.AppendLine();
          var adjLine = line.TrimStart(' ', '\t');
          adjLine = regex.Replace(adjLine, "");
          builder.Append($"{padding}{adjLine}");
          firstLine = false;
        }
      }

      var inner = e.InnerException;
      if(inner == null) return builder.ToString();

      builder.AppendLine();
      builder.AppendLine($"{padding}>> Inner Exception");
      builder.Append(ExceptionToPrettyString(inner, ++level));
      builder.AppendLine();
      builder.Append($"{padding}<< Inner Exception");

      return builder.ToString();
    }
  }
}
