using Dungeons.Core;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class Lever : InteractiveTile//, IDescriptable
  {
    public bool IsOff { get; set; }
    public bool Enabled;
    public bool Broken { get; set; }

    public Lever(Point point) : base(',')
    {
      Enabled = true;//while animating will be false
      IsOff = true;
      Kind = InteractiveTileKind.Lever;
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    public Lever() : this(new Point().Invalid())
    {
    }

    public string GetPrimaryStatDescription()
    {
      return "Can open locked doors";
    }

    public string[] GetExtraStatDescription()
    {
      return null;
    }
  }

  public class LeverPart : Loot
  {
    public LeverPart()
    {
      Symbol = ',';
      tag1 = "lever_part";
      PrimaryStatDescription = "Part of a lever mechanism";
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
    }
  }
}
