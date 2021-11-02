using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System.Drawing;

namespace Dungeons.Tiles
{
  //TODO rename
  public class InteractiveTile : Dungeons.Tiles.Tile, IObstacle
  {
    public Point Position => point;

    public InteractiveTile(Point point, char symbol) : base(point, symbol)
    {
    }

    public InteractiveTile(char symbol) : this(new Point(-1, -1), symbol)
    {

    }

    public HitResult OnHitBy(IProjectile md)
    {
      return HitResult.Hit;
    }
  }
}
