using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Extensions
{
  static class SystemExtensions
  {
    public static void Increment<T>(this Dictionary<T, int> dictionary, T key)
    {
      int count;
      dictionary.TryGetValue(key, out count);
      dictionary[key] = count + 1;
    }
  }
}
