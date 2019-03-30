using Dungeons.Core;
using Dungeons.Tiles;
using System;
using System.Drawing;

namespace Roguelike.Tiles
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
      tag = "lever_part";
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
    }

    public override string GetPrimaryStatDescription()
    {
      return "Part of a lever mechanism";
    }
  }
}
