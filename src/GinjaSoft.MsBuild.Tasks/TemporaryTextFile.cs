namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Text.RegularExpressions;
  using System.IO;


  internal class TemporaryTextFile : IDisposable
  {
    //
    // Private data
    //

    private readonly FileInfo _file;


    //
    // Constructor
    //

    public TemporaryTextFile(string body, string extension = "tmp", string path = null)
    {
      if(path == null) path = Path.GetTempPath();
      if(!path.EndsWith(@"\")) path += @"\";
      if(!Directory.Exists(path)) throw new Exception($"Path '{path}' does not exist");
      var regex = new Regex(@"^\s*(\.)?(?<extension>\w+)\s*$");
      var matches = regex.Match(extension);
      if(!matches.Success) throw new ArgumentException($"Invalid file extension {extension}");

      extension = matches.Groups["extension"].Value;
      var fileTemplate = $"{path}{Guid.NewGuid()}-{0}.{extension}";
      var count = 0;
      var file = string.Format(fileTemplate, count);
      while(File.Exists(file)) {
        file = string.Format(fileTemplate, count);
        ++count;
      }

      _file = new FileInfo(file);
      using(var fileStream = _file.CreateText()) fileStream.Write(body);
    }


    //
    // Public properties
    //

    public string FilePath => _file.FullName;
    public FileInfo FileInfo => _file;
    public string Body => File.ReadAllText(FilePath);


    //
    // Public methods
    //

    public void Dispose()
    {
      if(_file.Exists) _file.Delete();
    }

    public void AppendText(string s)
    {
      using(var writer = _file.AppendText()) {
        writer.Write(s);
      }
    }
  }
}