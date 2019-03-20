using Roguelike.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Utils
{
  public class Logger : ILogger
  {
    public void LogError(string err)
    {
      Debug.WriteLine(err);
    }

    public void LogInfo(string info)
    {
      Debug.WriteLine(info);
    }
  }
}
