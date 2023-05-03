using System;
using System.Diagnostics;

namespace Dungeons.Core
{
  public class Logger : ILogger
  {
    public Logger()
    {
      Debug.WriteLine("Logger ctor");
    }

    public LogLevel LogLevel 
    { 
      get; 
      set; 
    }

    public void LogError(string err, bool throwExc = true)
    {
      Debug.WriteLine("ERROR: " + err);
      if (throwExc)
        throw new Exception(err);
    }

    public void LogInfo(string info)
    {
      if (LogLevel >= LogLevel.Info)
        Debug.WriteLine(info);
    }

    public void LogError(Exception ex)
    {
      string err = CreateError(ex);
      Debug.WriteLine("ERROR: " + err);
    }

    public static string CreateError(Exception ex)
    {
      var err = ex.Message;
      if (ex.InnerException != null)
      {
        err += "ex.InnerException: " + ex.InnerException.Message;
      }
      //InputManager.HandleHeroShift looged error - no stack trace was visible
      err += Environment.NewLine + ex.StackTrace;
      return err;
    }
  }
}
