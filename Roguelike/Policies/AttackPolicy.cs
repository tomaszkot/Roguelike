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
    LivingEntity attacker;
    LivingEntity victim;

    public AttackPolicy(LivingEntity attacker, LivingEntity victim)
    {
      this.attacker = attacker;
      this.victim = victim;
    }

    public void Apply()
    {
      if (attacker.CalculateIfHitWillHappen(victim))
        attacker.ApplyPhysicalDamage(victim);

      attacker.State = EntityState.Attacking;

      ReportApplied(attacker);
    }

    
  }
}
