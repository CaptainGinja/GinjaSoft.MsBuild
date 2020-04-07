namespace GinjaSoft.MsBuild.Tasks
{
  internal static class Singletons
  {
    //
    // Static constructor
    //

    static Singletons() { Tools = new Tools(); }


    //
    // Public static properties
    //

    public static ITools Tools { get; }
  }
}
