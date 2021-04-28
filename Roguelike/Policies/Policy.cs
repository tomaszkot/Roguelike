using Roguelike.Tiles.LivingEntities;
using System;

namespace Roguelike.Policies
{
  public enum PolicyKind { Generic, Move, Attack, SpellCast }

  public class Policy
  {
    public PolicyKind Kind { get; set; }
    public event EventHandler<Policy> OnApplied;

    protected virtual void ReportApplied(LivingEntity entity)
    {
      entity.State = EntityState.Idle;
      if (OnApplied != null)
        OnApplied(this, this);
    }
  }
}
