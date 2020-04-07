namespace GinjaSoft.MsBuild.Tasks.Tests
{
  using System;
  using System.Reflection;
  using System.Runtime.CompilerServices;
  using Xunit;


  public class ExceptionExtensionsTests
  {

    [Fact]
    public void NoInnerException()
    {
      var e = new Exception("test message");
      const string expected = "System.Exception: test message";
      var result = e.ToPrettyString();
      Assert.Equal(expected, result);
    }

    [Fact]
    public void InnerException()
    {
      var e = new Exception("outer message", new Exception("inner message"));
      const string expected =
@"System.Exception: outer message
>> Inner Exception
  System.Exception: inner message
<< Inner Exception";
      var result = e.ToPrettyString();
      Assert.Equal(expected, result);
    }

    [Fact]
    public void InnerInnerException()
    {
      var e = new Exception("outer message", new Exception("inner message", new Exception("inner inner message")));
      const string expected =
@"System.Exception: outer message
>> Inner Exception
  System.Exception: inner message
  >> Inner Exception
    System.Exception: inner inner message
  << Inner Exception
<< Inner Exception";
      var result = e.ToPrettyString();
      Assert.Equal(expected, result);
    }

    [Fact]
    public void StackTrace()
    {
      try {
        ThrowException();
      }
      catch(Exception e) {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        var expected =
$@"System.Exception: test message
at {assemblyName}.ExceptionExtensionsTests.ThrowException()
at {assemblyName}.ExceptionExtensionsTests.StackTrace()";
        var result = e.ToPrettyString();
        Assert.Equal(expected, result);
      }
    }

    [Fact]
    public void InnerExceptionWithStackTrace()
    {
      try {
        NestedCall();
      }
      catch(Exception e) {
        var assemblyName = Assembly.GetExecutingAssembly().GetName().Name;
        var expected =
$@"System.Exception: wrapper message
at {assemblyName}.ExceptionExtensionsTests.NestedCall()
at {assemblyName}.ExceptionExtensionsTests.InnerExceptionWithStackTrace()
>> Inner Exception
  System.Exception: test message
  at {assemblyName}.ExceptionExtensionsTests.ThrowException()
  at {assemblyName}.ExceptionExtensionsTests.NestedCall()
<< Inner Exception";
        var result = e.ToPrettyString();
        Assert.Equal(expected, result);
      }
    }


    //
    // Private methods
    //

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void ThrowException() { throw new Exception("test message"); }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void NestedCall()
    {
      try {
        ThrowException();
      }
      catch(Exception e) {
        throw new Exception("wrapper message", e);
      }
    }
  }
}
