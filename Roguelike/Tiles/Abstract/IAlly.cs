using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Tiles
{
  public interface IAlly
  {
    bool Active { get; set; }
    AllyKind Kind { get; }
    bool IncreaseExp(double factor);
    Point Point { get; set; }

    event EventHandler LeveledUp;
  }
}
