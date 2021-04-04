using Dungeons.Tiles;
using Roguelike.Abstract.Projectiles;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public class SpellManager : PolicyManager
  {
    public SpellManager(GameManager mgr) : base(mgr)
    {
      this.gm = mgr;
      Container = gm.Container;
    }

    public bool ApplyActiveSpell(IDestroyable target)
    {
      var scroll = gm.Hero.ActiveScroll;
      return ApplyAttackPolicy
      (
        gm.Hero,
        target,
        scroll
      );
    }

    public OffensiveSpell ApplySpell(LivingEntity caster, Scroll scroll)
    {
      var spell = scroll.CreateSpell(caster);

      if (!gm.Context.UtylizeScroll(caster, scroll, spell))
        return null;

      if (spell is OffensiveSpell ps)
      {
        if (ps is SkeletonSpell skeletonSpell)
        {
          gm.AddAlly(skeletonSpell.Enemy);
        }
        if (caster is Hero)
          HandleHeroActionDone();

        return ps;
      }
      else
        gm.Logger.LogError("!OffensiveSpell " + scroll);

      return null;
    }
        
    public bool ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      Roguelike.Tiles.Abstract.IDestroyable target,
      Scroll scroll,
      Action<Policy> BeforeApply = null
      , Action<Policy> AfterApply = null
    )
    {
      var spell = scroll.CreateSpell(caster);

      if (! gm.Context.UtylizeScroll(caster, scroll, spell))
        return false;

      var policy = Container.GetInstance<SpellCastPolicy>();
      policy.Target = target;
      policy.ProjectilesFactory = Container.GetInstance<IProjectilesFactory>();
      policy.Spell = scroll.CreateSpell(caster) as Spell;
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        var le = policy.Target is LivingEntity;
        if (!le)//le is handled specially
        {
          this.gm.LootManager.TryAddForLootSource(policy.Target as ILootSource);
          policy.Target.Destroyed = true;
        }
        if (caster is Hero)
          OnHeroPolicyApplied(policy);

        if (AfterApply != null)
          AfterApply(policy);
      };

      policy.Apply(caster);
      return true;
    }

    

  }
}
