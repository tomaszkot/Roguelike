using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dungeons.Tiles
{
  //TODO rename
  public class InteractiveTile : Dungeons.Tiles.Tile, IObstacle
  {
    public InteractiveTile(Point point, char symbol) : base(point, symbol)
    { 
    }

    public InteractiveTile(char symbol) : this(new Point(-1,-1), symbol)
    { }
  }
}
