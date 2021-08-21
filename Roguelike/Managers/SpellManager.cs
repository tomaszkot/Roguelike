using Roguelike.Abstract.Projectiles;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class SpellManager : PolicyManager
  {
    public SpellManager(GameManager mgr) : base(mgr)
    {
      this.gm = mgr;
      Container = gm.Container;
    }

    public PassiveSpell ApplyPassiveSpell(LivingEntity caster, SpellSource spellSource, Point? destPoint = null)
    {
      var spell = spellSource.CreateSpell(caster);
      if (!gm.Context.CanUseScroll(caster, spellSource, spell))
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

        gm.UtylizeSpellSource(caster, spellSource, spell);
        gm.AppendAction<LivingEntityAction>((LivingEntityAction ac) =>
        { ac.Kind = LivingEntityActionKind.Teleported; ac.Info = gm.Hero.Name + " used " + spellSource.Kind.ToDescription() + " scroll"; ac.InvolvedEntity = caster; });

        if (caster is Hero)
          HandleHeroActionDone();

        return ps;
      }
      else
        gm.Logger.LogError("!PassiveSpell " + spellSource);

      return null;
    }

    public OffensiveSpell ApplySpell(LivingEntity caster, SpellSource spellSource)
    {
      var spell = spellSource.CreateSpell(caster);

      if (!gm.UtylizeSpellSource(caster, spellSource, spell))
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
      Tiles.Abstract.IObstacle target,
      SpellSource spellSource,
      Action<Policy> BeforeApply = null,
      Action<Policy> AfterApply = null
    )
    {
      var spell = spellSource.CreateSpell(caster) as IProjectileSpell;

      if (!gm.UtylizeSpellSource(caster, spellSource, spell))
        return false;

      var policy = Container.GetInstance<ProjectileCastPolicy>();
      policy.Target = target as Dungeons.Tiles.Tile;
      policy.ProjectilesFactory = Container.GetInstance<IProjectilesFactory>();
      policy.Projectile = spellSource.CreateSpell(caster) as IProjectileSpell;
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        var le = policy.TargetDestroyable is LivingEntity;
        if (!le)//le is handled specially
        {
          this.gm.LootManager.TryAddForLootSource(policy.Target as ILootSource);
          //dest.Destroyed = true;
        }

        var bulkOK = false;
        if (target is Enemy en && spellSource is WeaponSpellSource)
          bulkOK = HandleBulk(en, EntityStatKind.ChanceToElementalBulkAttack);

        if (!bulkOK)
        {
          var repeatOK = caster.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatElementalAttack);
          if (repeatOK)
          {
            ApplyAttackPolicy(caster, target, spellSource, BeforeApply, AfterApply);
            return;
          }
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
