using System;

namespace Dungeons.Core
{
  public interface ILogger
  {
    void LogError(Exception ex);
    void LogError(string err, bool throwExc = false);
    void LogInfo(string info);
  }
}
