using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles.Abstract;
using System;
using System.Drawing;
using System.Linq;

namespace Dungeons.Tiles
{
  //TODO rename
  public class InteractiveTile : Dungeons.Tiles.Tile, IObstacle
  {
    public event EventHandler Interaction;
    public Point Position => point;


    public InteractiveTile(Point point, char symbol) : base(point, symbol)
    {
    }

    public InteractiveTile(char symbol) : this(new Point(-1, -1), symbol)
    {

    }

    protected virtual void EmitInteraction()
    {
      if (Interaction != null)
        Interaction(this, EventArgs.Empty);
    }

    
    public virtual void PlayHitSound(IProjectile proj)
    {

    }
    public virtual void PlayHitSound(IDamagingSpell spell)
    {

    }

    public virtual HitResult OnHitBy(IDamagingSpell damager, IPolicy policy)
    {
      return HandleHit(HitResult.Hit);
    }

    private HitResult HandleHit(HitResult res)
    {
      if (res == HitResult.Hit)
        EmitInteraction();
      return res;
    }

    public virtual HitResult OnHitBy(IProjectile md, IPolicy policy)
    {
      return HandleHit(HitResult.Hit);
    }

    public virtual HitResult OnHitBy(ILivingEntity livingEntity)
    {
      return HandleHit(HitResult.Hit);
    }
  }
}
