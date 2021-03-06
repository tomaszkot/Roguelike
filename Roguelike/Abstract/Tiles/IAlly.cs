using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract
{
  public interface IAlly
  {
    bool Active { get; set; }
    Tiles.AllyKind Kind { get; }
  }
}
