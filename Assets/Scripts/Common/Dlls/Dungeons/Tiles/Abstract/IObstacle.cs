using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System;
using System.Drawing;

namespace Dungeons.Fight
{
  public enum HitResult { Unset, Hit, Evaded }
}

namespace Dungeons.Tiles
{
  public interface IHitable
  {
    Point Position { get; }
    HitResult OnHitBy(IProjectile md);

    HitResult OnHitBy(IDamagingSpell ds);

    void PlayHitSound(IProjectile proj);
    void PlayHitSound(IDamagingSpell spell);

  }

  public interface IObstacle : IHitable
  {
    
  }
}
