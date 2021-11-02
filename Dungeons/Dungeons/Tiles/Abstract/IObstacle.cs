using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System.Drawing;

namespace Dungeons.Fight
{
  public enum HitResult { Unset, Hit, Evaded }
}

namespace Dungeons.Tiles
{
  public interface IObstacle
  {
    HitResult OnHitBy(IProjectile md);
    //bool OnHitBy(ISpell md);
    Point Position { get; }
  }
}
