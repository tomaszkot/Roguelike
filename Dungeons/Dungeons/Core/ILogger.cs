using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Core
{
  public interface ILogger
  {
    void LogError(Exception ex);
    void LogError(string err, bool throwExc = false);
    void LogInfo(string info);
  }
}
