namespace GinjaSoft.MsBuild.Tasks
{
  using System;
  using System.Threading;
  using System.IO;


  internal class TemporaryDirectory : IDisposable
  {
    //
    // Private data
    //

    private readonly DirectoryInfo _directory;


    //
    // Constructor
    //

    public TemporaryDirectory()
    {
      var directoryTemplate = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}-{0}");
      var count = 0;
      var directoryPath = string.Format(directoryTemplate, count);
      while(Directory.Exists(directoryPath)) {
        directoryPath = string.Format(directoryTemplate, count);
        ++count;
      }

      _directory = new DirectoryInfo(directoryPath);
      _directory.Create();
    }


    //
    // Public properties
    //

    public string DirectoryPath => _directory.FullName;
    public DirectoryInfo DirectoryInfo => _directory;


    //
    // Public methods
    //

    public void Dispose()
    {
      if(_directory.Exists) DeleteDirectory(_directory);
    }


    //
    // Private methods
    //

    private void DeleteDirectory(DirectoryInfo directory)
    {
      foreach(var file in directory.GetFiles()) DeleteFileSystemObject(file);

      foreach(var subDirectory in directory.GetDirectories()) DeleteDirectory(subDirectory);

      DeleteFileSystemObject(directory);
    }

    private void DeleteFileSystemObject(FileSystemInfo fileSystemObject, int retry = 0)
    {
      try {
        fileSystemObject.Attributes = FileAttributes.Normal;
        fileSystemObject.Delete();
      }
      catch(Exception e) {
        if(!(e is IOException || e is UnauthorizedAccessException)) throw;
        if(retry >= 3) throw;
        Thread.Sleep(10);
        DeleteFileSystemObject(fileSystemObject, ++retry);
      }
    }
  }
}