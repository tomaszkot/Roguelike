using System.Drawing;

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
