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
      //err += Environment.NewLine + new StackTrace().ToString();
      return err;
    }
  }
}
