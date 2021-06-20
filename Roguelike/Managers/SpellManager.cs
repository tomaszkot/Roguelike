using Roguelike.Abstract.Projectiles;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Drawing;

namespace Roguelike.Managers
{
  public class SpellManager : PolicyManager
  {
    public SpellManager(GameManager mgr) : base(mgr)
    {
      this.gm = mgr;
      Container = gm.Container;
    }

    public PassiveSpell ApplyPassiveSpell(LivingEntity caster, SpellSource scroll, Point? destPoint = null)
    {
      var spell = scroll.CreateSpell(caster);
      if (!gm.Context.CanUseScroll(caster, scroll, spell))
        return null;

      if (spell is PassiveSpell ps)
      {
        if (ps.Kind == SpellKind.Teleport)
        {
          if (destPoint != null)
          {
            var currentTile = gm.CurrentNode.GetTile(destPoint.Value);
            var teleportSpell = ps as TeleportSpell;
            if (teleportSpell.Range < gm.Hero.DistanceFrom(currentTile))
            {
              gm.SoundManager.PlayBeepSound();
              gm.EventsManager.AppendAction(new Events.GameInstructionAction() { Info = "Range of spell is too small (max:" + teleportSpell.Range + ")" });
              return null;
            }

            if (currentTile.IsEmpty || currentTile is Loot)
              gm.CurrentNode.SetTile(gm.Hero, destPoint.Value);
            else
            {
              gm.SoundManager.PlayBeepSound();
              return null;
            }
          }
        }
        else
          caster.ApplyPassiveSpell(ps);

        gm.UtylizeScroll(caster, scroll, spell);
        gm.AppendAction<LivingEntityAction>((LivingEntityAction ac) =>
        { ac.Kind = LivingEntityActionKind.Teleported; ac.Info = gm.Hero.Name + " used " + scroll.Kind.ToDescription() + " scroll"; ac.InvolvedEntity = caster; });

        if (caster is Hero)
          HandleHeroActionDone();

        return ps;
      }
      else
        gm.Logger.LogError("!PassiveSpell " + scroll);

      return null;
    }

    public OffensiveSpell ApplySpell(LivingEntity caster, SpellSource spellSource)
    {
      var spell = spellSource.CreateSpell(caster);

      if (!gm.UtylizeScroll(caster, spellSource, spell))
        return null;

      if (spell is OffensiveSpell ps)
      {
        if (ps is SkeletonSpell skeletonSpell)
        {
          gm.AddAlly(skeletonSpell.Ally);
        }
        if (caster is Hero)
          HandleHeroActionDone();

        return ps;
      }
      else
        gm.Logger.LogError("!OffensiveSpell " + spellSource);

      return null;
    }

    public bool ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      Roguelike.Tiles.Abstract.IObstacle target,
      SpellSource spellSource,
      Action<Policy> BeforeApply = null
      , Action<Policy> AfterApply = null
    )
    {
      var spell = spellSource.CreateSpell(caster);

      if (!gm.UtylizeScroll(caster, spellSource, spell))
        return false;

      var policy = Container.GetInstance<SpellCastPolicy>();
      policy.Target = target;
      policy.ProjectilesFactory = Container.GetInstance<IProjectilesFactory>();
      policy.Spell = spellSource.CreateSpell(caster) as Spell;
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        var le = policy.Target is LivingEntity;
        if (!le)//le is handled specially
        {
          this.gm.LootManager.TryAddForLootSource(policy.Target as ILootSource);
          //if(policy.Target is IDestroyable dest)
          //dest.Destroyed = true;
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
