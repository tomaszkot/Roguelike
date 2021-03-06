using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Tiles
{
  public interface IAlly
  {
    bool Active { get; set; }
    AllyKind Kind { get; }
  }
}
