using Roguelike.Abilities;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;

namespace Roguelike.Policies
{
  public enum PolicyKind { Generic, Move, Attack, SpellCast, ProjectileCast }

  public abstract class Policy
  {
    public AbilityKind AbilityKind { get; internal set; }
    public bool Bulked { get; set; }
    public PolicyKind Kind 
    { 
      get; 
      set; 
    }
    public bool ChangesTurnOwner {  get; set; } = true;

    public event EventHandler<Policy> OnApplied;
    public event EventHandler<Dungeons.Tiles.IHitable> OnTargetHit;

    public abstract void Apply(LivingEntity caster);


    protected virtual void ReportHit(Dungeons.Tiles.IHitable entity)
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

  public class GenericPolicy : Policy
  {
    public override void Apply(LivingEntity caster)
    {
      
    }
  }

  
}
