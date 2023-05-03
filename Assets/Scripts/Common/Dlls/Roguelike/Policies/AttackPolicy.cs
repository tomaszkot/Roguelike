using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Policies
{
  public abstract class AttackPolicy : Policy
  {
    public List<Dungeons.Tiles.IHitable> Targets = new List<Dungeons.Tiles.IHitable>();
    public abstract void AttackNextTarget
    (
      LivingEntity caster,
      Dungeons.Tiles.IHitable nextTarget
    );

    public void AddTarget(Dungeons.Tiles.IHitable obstacle)
    {
      Targets.Add(obstacle);
    }

    public abstract void CreateSpell(LivingEntity caster, SpellSource spellSource);
    
  }
}
