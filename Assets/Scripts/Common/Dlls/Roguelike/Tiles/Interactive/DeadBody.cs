using Dungeons.Core;
using Newtonsoft.Json;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class DeadBody : InteractiveTile, ILootSource
  {
    public DeadBody(Container cont, Point point) : base(cont, '~')
    {
      Name = "Dead body";
      tag1 = "dead_body";
      DestroySound = "uncloth";
      InteractSound = "uncloth";
      Kind = InteractiveTileKind.DeadBody;
    }
    [JsonIgnore]
    public bool RewardGenerated { get; set; }
    public Loot ForcedReward { get; set; }

    public DeadBody(Container cont) : this(cont, new Point().Invalid())
    {

    }


    public bool LevelSet { get; set; }

    public string OriginMap { get; set; }

    public bool SetLevel(int level, Difficulty? diff = null)
    {
      Level = level;
      LevelSet = true;
      return true;
    }

    public Point GetPoint()
    {
      return point;
    }
  }
}
