using Dungeons.Core;
using Dungeons.Tiles.Abstract;
using System.Drawing;

namespace Dungeons.Tiles
{
  public class Wall : Tile, IObstacle
  {
    public bool IsSide { get; set; }

    public Point Position => point;

    public Wall(Point point) : base(point, Constants.SymbolWall)
    {
    }

    public Wall() : this(new Point().Invalid()) { }

    public bool OnHitBy(IProjectile md)
    {
      return false;
    }
  }
}
