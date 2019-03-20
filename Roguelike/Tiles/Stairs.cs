#define ASCII_BUILD
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum StairsKind { PitDown, PitUp, LevelUp, LevelDown };

  public class Stairs : InteractiveTile, IObstacle
  {
    StairsKind kind;
    public string PitName { get; set; }
    public StairsKind Kind { get => kind; set => kind = value; }

    public Stairs() : base('>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      Kind = StairsKind.LevelDown;
    }

    public Stairs(StairsKind kind) : base('>')
    {
#if ASCII_BUILD
      color = ConsoleColor.Blue;
#endif
      Kind = kind;
    }
  }
}
