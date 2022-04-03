using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Policies
{
  public enum PolicyKind { Generic, Move, Attack, SpellCast }

  public class Policy
  {
    public PolicyKind Kind { get; set; }
    public event EventHandler<Policy> OnApplied;
    public event EventHandler<IObstacle> OnTargetHit;

    protected virtual void ReportHit(IObstacle entity)
    {
      if(OnTargetHit!=null)
        OnTargetHit(this, entity);
    }

    protected virtual void ReportApplied(LivingEntity entity)
    {
      entity.State = EntityState.Idle;
      if (OnApplied != null)
        OnApplied(this, this);
    }
  }
}
