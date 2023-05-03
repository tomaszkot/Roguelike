using Roguelike.Tiles.LivingEntities;
using System;
using System.Drawing;

namespace Roguelike.Abstract.Tiles
{
  public interface IAlly
  {
    AllyBehaviour AllyBehaviour { get; set; }
    bool Active { get; set; }
    AllyKind Kind { get; }
    bool IncreaseExp(double factor);
    Point Point { get; set; }
    bool SetLevel(int level, Difficulty? diff = null);
    bool TakeLevelFromCaster { get; }
    string Name { get; }

    event EventHandler LeveledUp;

    void SetNextLevelExp(double exp);
  }
}
