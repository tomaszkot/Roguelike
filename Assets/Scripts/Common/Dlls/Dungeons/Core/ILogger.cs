using System;

namespace Dungeons.Core
{
  public enum LogLevel { Unset, Info, Warning, Error }
  public interface ILogger
  {
    LogLevel LogLevel { get; set; } 

    void LogError(Exception ex);
    void LogError(string err, bool throwExc = false);
    void LogInfo(string info);
  }
}
