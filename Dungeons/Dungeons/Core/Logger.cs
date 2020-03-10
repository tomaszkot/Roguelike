using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Core
{
  public class Logger : ILogger
  {
    public void LogError(string err, bool throwExc = true)
    {
      Debug.WriteLine("ERROR: "+ err);
      if (throwExc)
        throw new Exception(err);
    }

    public void LogInfo(string info)
    {
      Debug.WriteLine(info);
    }
  }
}
