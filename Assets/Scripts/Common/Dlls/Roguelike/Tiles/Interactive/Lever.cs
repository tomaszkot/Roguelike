using Dungeons.Core;
using SimpleInjector;
using System;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class Lever : InteractiveTile//, IDescriptable
  {
    public bool IsOff { get; private set; }
    public bool IsOn => !IsOff;
    public bool Enabled;
    public bool Broken { get; set; }
    //public event EventHandler<bool> StateChanged;

    public Lever(Container cont,Point point) : base(cont, ',')
    {
      Enabled = true;//while animating will be false
      IsOff = true;
      Kind = InteractiveTileKind.Lever;
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    public Lever(Container cont) : this(cont, new Point().Invalid())
    {
      tag1 = "lever_wheel1";
    }

    public string GetPrimaryStatDescription()
    {
      return "Can open locked doors";
    }

    public string[] GetExtraStatDescription()
    {
      return null;
    }

    public void SwitchState()
    {
      IsOff = !IsOff;
      EmitInteraction();
      //if (StateChanged!=null)
      //  StateChanged(this, IsOff);
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
