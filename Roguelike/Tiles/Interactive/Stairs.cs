﻿#define ASCII_BUILD
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Interactive
{
  public enum StairsKind { Unset, PitDown, PitUp, LevelUp, LevelDown };

  public class Stairs : InteractiveTile, IObstacle
  {
    StairsKind kind;
    public string pitName = "";
    public string PitName
    {
      get { return pitName; }
      set { pitName = value; }
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
          case StairsKind.LevelDown:
            Symbol = '<';
            break;
          case StairsKind.PitUp:
          case StairsKind.LevelUp:
            Symbol = '>';
            break;

          default:
            Debug.Assert(false);
            break;
        }
      }
    }

    public Stairs() : this(StairsKind.LevelDown)
    {
    }

    public Stairs(StairsKind kind) : base('>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      StairsKind = kind;
    }
  }
}