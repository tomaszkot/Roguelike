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

    protected void EmitInteraction()
    {
      if (Interaction != null)
        Interaction(this, EventArgs.Empty);
    }

    public virtual HitResult OnHitBy(IProjectile md)
    {
      return HitResult.Hit;
    }

    public virtual HitResult OnHitBy(IDamagingSpell md)
    {
      return HitResult.Hit;
    }

    public virtual void PlayHitSound(IProjectile proj)
    {

    }
    public virtual void PlayHitSound(Dungeons.Tiles.Abstract.IDamagingSpell spell)
    {

    }


    
  }
}
