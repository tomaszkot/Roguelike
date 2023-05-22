using System.Drawing;

namespace Roguelike.Tiles
{
  public interface ILootSource
  {
    int Level { get; }
    bool SetLevel(int level, Difficulty? diff = null);
    Point GetPoint();
    string OriginMap { get; set; }

    bool LevelSet { get; set; }
    Loot ForcedReward { get; set; }
    bool IsLooted { get; set; }
    bool RewardGenerated { get; set; }
  }
}

