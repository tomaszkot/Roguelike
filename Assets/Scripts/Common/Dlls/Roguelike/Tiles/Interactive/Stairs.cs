#define ASCII_BUILD
using Dungeons;
using SimpleInjector;
using System;
using System.Diagnostics;

namespace Roguelike.Tiles.Interactive
{
  public enum StairsKind { Unset, PitDown, PitUp, LevelUp, LevelDown };

  public class Stairs : InteractiveTile, IApproachableByHero
  {
    bool closed = false;
    public bool Closed { get => closed; set => closed = value; }
    public event EventHandler Activated;
    StairsKind kind;
    public string pitName = "";
    public string PitName
    {
      get { return pitName; }
      set 
      { 
        pitName = value; 
      }
    }

    public StairsKind StairsKind
    {
      get => kind;
      set
      {
        kind = value;
        switch (kind)
        {
          case StairsKind.PitDown:
            Symbol = '<';
            if(string.IsNullOrEmpty(tag1))
              tag1 = "pit_down_big";
            break;
          case StairsKind.LevelDown:
            Symbol = '<';
            if (string.IsNullOrEmpty(tag1))
              tag1 = "level_down";
            break;
          case StairsKind.PitUp:
            Symbol = '>';
            if (string.IsNullOrEmpty(tag1))
              tag1 = "level_up";
            break;
          case StairsKind.LevelUp:
            Symbol = '>';
            if (string.IsNullOrEmpty(tag1))
              tag1 = "level_up";
            break;

          default:
            DebugHelper.Assert(false);
            break;
        }
      }
    }

    public Stairs(Container cont) : this(cont, StairsKind.LevelDown)
    {
    }

    public Stairs(Container cont, StairsKind kind) : base(cont, '>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      StairsKind = kind;
    }

    public bool Activate()
    {
      if (!ApproachedByHero)
      {
        ApproachedByHero = true;
        if (Activated != null)
          Activated(this, EventArgs.Empty);

        return true;
      }

      return false;
    }

    public string GetPlaceName()
    {
      return "";// DungeonPit.GetPitDisplayName(st);
    }

    public string ActivationSound { get; set; } = "";

    public override string ToString()
    {
      return base.ToString() + ", pit: "+PitName;
    }
  }
}
