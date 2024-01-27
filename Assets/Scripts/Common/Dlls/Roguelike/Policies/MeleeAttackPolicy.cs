using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;

namespace Roguelike.Policies
{
  public class MeleeAttackPolicy : AttackPolicy
  {
    private LivingEntity attacker;
    private IHitable victim;//typically LivingEntity

    public MeleeAttackPolicy()
    {
      Kind = PolicyKind.MeleeAttack;
    }

    public IHitable Victim { get => victim; protected set => victim = value; }
    public LivingEntity Attacker { get => attacker;  }

    public override void Apply(LivingEntity attacker)
    {
      Apply(attacker, victim);
    }

    public virtual void Apply(LivingEntity attacker, IHitable victim)
    {
      this.attacker = attacker;
      this.victim = victim;

      attacker.State = EntityState.Attacking;

      
      this.TryAttack(this, attacker, victim);

      ReportApplied(attacker);
    }

    public override void AttackNextTarget(LivingEntity attacker, IHitable victim)
    {
      this.TryAttack(this, attacker, victim);
    }

    /// <summary>
    /// //TODO remove it
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="spellSource"></param>
    /// <exception cref="System.NotImplementedException"></exception>
    public override ISpell CreateSpell(LivingEntity caster, SpellSource spellSource)
    {
      throw new System.NotImplementedException();
    }

    protected override void ReportApplied(LivingEntity attacker)
    {
      var le = victim as LivingEntity;
      
      base.ReportApplied(attacker);
    }
  }
}
