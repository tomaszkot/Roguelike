using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Dungeons.Tiles.Abstract
{
  public interface IHitable
  {
    Point Position { get; }
    HitResult OnHitBy(IProjectile md, IPolicy policy);

    HitResult OnHitBy(IDamagingSpell ds, IPolicy policy);

    HitResult OnHitBy(ILivingEntity livingEntity);

    void PlayHitSound(IProjectile proj);
    void PlayHitSound(IDamagingSpell spell);

    public string Name { get; }

  }
}
