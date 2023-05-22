using Dungeons.Core.Policy;
using Roguelike.Abilities;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;

namespace Roguelike.Policies
{
  public enum PolicyKind { Generic, Move, MeleeAttack, SpellCast, ProjectileCast }

  public abstract class Policy : IPolicy
  {
    public AbilityKind AbilityKind { get; internal set; }
    public bool Bulked { get; set; }
    public PolicyKind Kind 
    { 
      get; 
      set; 
    }
    public bool ChangesTurnOwner {  get; set; } = true;
    public event EventHandler<Dungeons.Tiles.Abstract.IHitable> TargetHit;
    public event EventHandler<Policy> OnApplied;
    

    public abstract void Apply(LivingEntity caster);


    public virtual void ReportHit(Dungeons.Tiles.Abstract.IHitable entity)
    {
      if(TargetHit!=null)
        TargetHit(this, entity);
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
