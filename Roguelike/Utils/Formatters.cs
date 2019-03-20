using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Utils
{
  public static class StringFormats
  {
    public static string Formatted(this float val)
    {
      return val.ToString("0.00");
    }
  }
}
