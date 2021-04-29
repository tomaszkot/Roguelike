using Roguelike.Tiles.LivingEntities;
using System;
using System.Drawing;

namespace Roguelike.Abstract.Tiles
{
  public interface IAlly
  {
    bool Active { get; set; }
    AllyKind Kind { get; }
    bool IncreaseExp(double factor);
    Point Point { get; set; }
    bool SetLevel(int level);
    bool TakeLevelFromCaster { get; }

    event EventHandler LeveledUp;
  }
}
