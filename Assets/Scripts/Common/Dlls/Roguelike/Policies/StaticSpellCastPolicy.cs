using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Roguelike.Abstract.Spells;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;

namespace Roguelike.Policies
{
  public class StaticSpellCastPolicy : AttackPolicy
  {
    LivingEntity caster;
    public GameManager GameManager { get; set; }

    public StaticSpellCastPolicy() : this(null)
    {
      
    }

    public StaticSpellCastPolicy(Container container) 
    {
      this.Kind = PolicyKind.SpellCast;
      if(container!=null)
        Init(container);
    }

    public void Init(Container container)
    {
      StaticSpellFactory = container.GetInstance<IStaticSpellFactory>();
    }

    public LivingEntity Caster { get => caster; set => caster = value; }
    public Dungeons.Tiles.Abstract.IDamagingSpell DamagingSpell { get; internal set; }
    public IStaticSpellFactory StaticSpellFactory { get; internal set; }

    public override void Apply(LivingEntity caster)
    {
      var target = this.Targets[0];
      caster.State = EntityState.CastingProjectile;

      //Log("calling  DoApply(caster)");
      DoApply(caster);

      ReportApplied(caster);
    }

    //called in ascii version
    protected virtual void DoApply(LivingEntity caster)
    {
      Targets.ForEach(i => AttackNextTarget(caster, i));
    }

    public override void AttackNextTarget
   (
     LivingEntity caster,
     Dungeons.Tiles.Abstract.IHitable nextTarget
   )
    {
      if (ShallHitTarget(nextTarget))
        nextTarget.OnHitBy(DamagingSpell, this);
    }

    private bool ShallHitTarget(IHitable nextTarget)
    {
      return true;
    }

    public override Dungeons.Tiles.Abstract.ISpell CreateSpell(LivingEntity caster, SpellSource spellSource)
    {
      DamagingSpell = spellSource.CreateSpell(caster) as Abstract.Spells.IDamagingSpell;
      return DamagingSpell;
    }
  }
}
