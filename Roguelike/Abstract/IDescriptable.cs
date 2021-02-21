using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Abstract
{
  public interface IDescriptable
  {
    string GetPrimaryStatDescription();
    string[] GetExtraStatDescription(bool currentLevel);

    bool Revealed { get; set; }
    string Name { get; set; }
  }
}
