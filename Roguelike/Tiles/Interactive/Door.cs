using Dungeons.Core;
using System;
using System.Drawing;

namespace Roguelike.Tiles.Interactive
{
  public class Door : InteractiveTile, Dungeons.Tiles.IDoor  //Dungeons.Tiles.Door
  {
    public bool Opened 
    { 
      get; 
      set; 
    }

    string bossBehind = "";
    public string BossBehind
    {
      get { return bossBehind; }
      set
      {
        bossBehind = value;
        Color = ConsoleColor.Red;
      }
    }

    public Door(Point point) : base(Dungeons.Tiles.Constants.SymbolDoor)
    {
      Point = point;
      Color = ConsoleColor.Yellow;
    }

    public Door() : this(GenerationConstraints.InvalidPoint)
    {

    }
  }
}
