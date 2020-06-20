using Dungeons.Core;
using System;
using System.Drawing;

namespace Dungeons.Tiles
{
  public class Door : Tile
  {
    string bossBehind;
    public string BossBehind
    {
      get { return bossBehind; }
      set {
        bossBehind = value;
        Color = ConsoleColor.Red;
      }
    }

    public Door(Point point) : base(point, Constants.SymbolDoor)
    {
      Color = ConsoleColor.Yellow;
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }
  }
}
