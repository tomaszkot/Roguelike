using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Policies
{
  public class AttackPolicy : Policy
  {
    protected LivingEntity attacker;
    protected LivingEntity victim;

    public AttackPolicy()
    {
    }

    public virtual void Apply(LivingEntity attacker, LivingEntity victim)
    {
      this.attacker = attacker;
      this.victim = victim;

      attacker.State = EntityState.Attacking;

      ReportApplied(attacker);
    }

    protected override void ReportApplied(LivingEntity attacker)
    {
      if (attacker.CalculateIfHitWillHappen(victim))
        attacker.ApplyPhysicalDamage(victim);

      base.ReportApplied(attacker);
    }


  }
}
