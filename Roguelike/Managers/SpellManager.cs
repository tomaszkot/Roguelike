using Dungeons.Tiles;
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

    void AddApple(Tile dest)
    {
      var apple = new Food(FoodKind.Apple);
      apple.SetPoisoned();
      
      gm.CurrentNode.SetTile(apple, dest.point);
    }

    public PassiveSpell ApplyPassiveSpell(LivingEntity caster, SpellSource spellSource, Point? destPoint = null)
    {
      var spell = spellSource.CreateSpell(caster);
      string preventReason = "";
      if (!gm.Context.CanUseScroll(caster, spellSource, spell, ref preventReason))
      {
        gm.ReportFailure(preventReason);
        return null;
      }

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
              gm.ReportFailure("Range of spell is too small (max:" + teleportSpell.Range + ")");
              return null;
            }

            if (currentTile.IsEmpty || currentTile is Loot)
              gm.CurrentNode.SetTile(gm.Hero, destPoint.Value);
            else
            {
              gm.ReportFailure("Can not cast on the pointed tile");
              return null;
            }
          }
        }
        else if (ps.Kind == SpellKind.Dziewanna)
        {
          //for (int i = 0; i < 2; i++)
          {
            int counter = 0;
            var enemies = gm.CurrentNode.GetNeighborTiles<Enemy>(gm.Hero);
            var tiles = new List<Tile>();
            foreach (var en in enemies)
            {
              var emptyOnes = gm.CurrentNode.GetEmptyNeighborhoodTiles(en, false).Where(i=> !tiles.Contains(i));
              if (emptyOnes.Any())
              {
                tiles.Add(emptyOnes.First());
                counter++;
                if (counter >= 2)
                  break;
              }
            }
            if (counter == 0)
            {
              var emp = gm.CurrentNode.GetClosestEmpty(gm.Hero);
              tiles.Add(emp);
            }
            foreach (var tile in tiles)
            {
              AddApple(tile);              
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
      {
        gm.Logger.LogError("!PassiveSpell " + spellSource);
        gm.ReportFailure("");
      }

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
      {
        gm.Logger.LogError("!OffensiveSpell " + spellSource);
        gm.ReportFailure("");
      }

      return null;
    }


    bool applyingBulk = false;
    public bool ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      Tiles.Abstract.IObstacle target,
      SpellSource spellSource,
      Action<Policy> BeforeApply = null,
      Action<Policy> AfterApply = null,
      bool looped = false
    )
    {
      if(!looped)
        applyingBulk = false;
      var spell = spellSource.CreateSpell(caster) as IProjectileSpell;

      if (spell != null)
      {
        if (!caster.IsInProjectileReach(spell, target.Position))
        {
          this.gm.SoundManager.PlayBeepSound();
          this.gm.AppendAction(new GameEvent() { Info = "Target out of range" });
          return false;
        }
      }
      
      if (!looped && !gm.UtylizeSpellSource(caster, spellSource, spell))
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

        
        if (looped)
          return;
        if (!applyingBulk)
        {
          var repeatOK = caster.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatElementalProjectileAttack);
          if (repeatOK)
          {
            ApplyAttackPolicy(caster, target, spellSource, BeforeApply, AfterApply, true);
          }
        }

        if (caster is Hero)
          OnHeroPolicyApplied(policy);

        if (AfterApply != null)
          AfterApply(policy);
      };

      policy.Apply(caster);

      var bulkOK = false;
      if (target is Enemy en && spellSource is WeaponSpellSource)
        bulkOK = HandleBulk(en, EntityStatKind.ChanceToElementalProjectileBulkAttack, (Enemy en1) => {
          applyingBulk = true;
          ApplyAttackPolicy(caster, en1, spellSource, BeforeApply, AfterApply, true);
        });

      return true;
    }



  }
}
