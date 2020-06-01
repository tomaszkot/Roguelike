﻿using Roguelike.Tiles;
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
    protected Dungeons.Tiles.Tile victim;//typically LivingEntity

    public AttackPolicy()
    {
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
      if (le != null)
      {
        if (attacker.CalculateIfHitWillHappen(le))
          attacker.ApplyPhysicalDamage(le);
      }

      base.ReportApplied(attacker);
    }


  }
}
