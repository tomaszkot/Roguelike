using Dungeons.Tiles;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Policies
{
  public class MeleeAttackPolicy : Policy
  {
    private LivingEntity attacker;
    private Dungeons.Tiles.Tile victim;//typically LivingEntity

    public MeleeAttackPolicy()
    {
      Kind = PolicyKind.Attack;
    }

    public Tile Victim { get => victim; protected set => victim = value; }
    public LivingEntity Attacker { get => attacker;  }

    public override void Apply(LivingEntity attacker)
    {
      Apply(attacker, victim);
    }

    public virtual void Apply(LivingEntity attacker, Dungeons.Tiles.Tile victim)
    {
      this.attacker = attacker;
      this.victim = victim;

      attacker.State = EntityState.Attacking;

      ReportApplied(attacker);
    }
        
    protected override void ReportApplied(LivingEntity attacker)
    {
      var le = victim as LivingEntity;
      attacker.LastMeleeAttackWasOK = false;
      if (le != null)
      {
        if (attacker.CalculateIfHitWillHappen(le, Attributes.AttackKind.Melee, null))
        {
          attacker.ApplyPhysicalDamage(le);
          attacker.LastMeleeAttackWasOK = true;
        }
        else
        {
          attacker.EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed) 
          { 
            InvolvedEntity = attacker, 
            Info = attacker.Name + " missed " + victim.Name,
            AttackKind = AttackKind.Melee
          });
        }
      }

      base.ReportApplied(attacker);
    }


  }
}
