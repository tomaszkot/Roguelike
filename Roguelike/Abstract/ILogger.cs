using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract
{
  public interface ILogger
  {
    void LogError(string err);
    void LogInfo(string info);
  }
}
