using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Policies;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Spells;
using Roguelike.Tiles;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers.Policies
{
  public class ReportDelayedSpellDoneEventArgs : EventArgs
  {
    public float DelaySecs;
    public LivingEntity Caster;
  }

  public enum ApplyAttackPolicyResult { Unset, OK, NotEnoughResources, OutOfRange }

  public class SpellPolicyManager : PolicyManager
  {
    public SpellPolicyManager(GameManager mgr) : base(mgr)
    {
      gm = mgr;
      Container = gm.Container;
    }

    void AddApple(Tile dest)
    {
      var apple = new Food(FoodKind.Apple);
      apple.SetPoisoned();

      //gm.CurrentNode.SetTile(apple, dest.point);
      gm.AppendTile(apple, dest.point);
    }

    public Abstract.Spells.ISpell ApplyPassiveSpell<T>
    (
      LivingEntity caster,
      SpellSource spellSource,
      Point? destPoint = null,
      Func<T> fac = null
      )
      where T :
      Abstract.Spells.ISpell
    {
      var spell = fac != null ? fac() : spellSource.CreateSpell(caster);
      string preventReason = "";
      if (!gm.Context.CanUseScroll(caster, spellSource, spell, ref preventReason))
      {
        gm.ReportFailure(preventReason);
        return default(T);
      }

      bool reportSpellDone = true;
      //Roguelike.Abstract.Spells.ISpell spell = null;
      if (spell is PassiveSpell || spell is Portal)
      {
        var kind = SpellKind.Unset;
        if (spell is PassiveSpell ps)
        {
          kind = ps.Kind;
          //spell = ps;
        }
        else
        {
          kind = SpellKind.Portal;
        }

        if (kind == SpellKind.Teleport)
        {
          if (destPoint != null)
          {
            var currentTile = gm.CurrentNode.GetTile(destPoint.Value);
            var teleportSpell = spell as TeleportSpell;
            if (teleportSpell.Range < gm.Hero.DistanceFrom(currentTile))
            {
              gm.ReportFailure("Range of the spell is too small (max:" + teleportSpell.Range + ")");
              return default(T);
            }

            if (currentTile.IsEmpty || currentTile is Loot)
              gm.CurrentNode.SetTile(gm.Hero, destPoint.Value);
            else
            {
              gm.ReportFailure("Can not cast on the pointed tile");
              return default(T);
            }
          }
        }
        else if (kind == SpellKind.SwapPosition)
        {
          if (destPoint != null)
          {
            var currentTile = gm.CurrentNode.GetTile(destPoint.Value) as LivingEntity;
            if (currentTile == null)
            {
              gm.ReportFailure("Target must be a living entity");
              return default(T);
            }

            var teleportSpell = spell as SwapPositionSpell;
            if (teleportSpell.Range < gm.Hero.DistanceFrom(currentTile))
            {
              gm.ReportFailure("Range of the spell is too small (max:" + teleportSpell.Range + ")");
              return default(T);
            }

            var destPos = currentTile.Position;
            var heroPos = gm.Hero.Position;
            gm.CurrentNode.SetTile(gm.Hero, destPos);
            gm.CurrentNode.SetTile(currentTile, heroPos);
            gm.AppendAction(new LivingEntityAction() { Kind = LivingEntityActionKind.SwappedPos, InvolvedEntity = gm.Hero }); ;
            gm.AppendAction(new LivingEntityAction() { Kind = LivingEntityActionKind.SwappedPos, InvolvedEntity = currentTile });
            gm.SoundManager.PlaySound("teleport");
          }
        }
        else if (kind == SpellKind.Swarog)
        {
          //TODO
          var ems = gm.EnemiesManager.GetActiveEnemiesInvolved();
          foreach (var en in ems)
          {
            en.HitRandomTarget = true;
          }
        }

        else if (kind == SpellKind.Dziewanna)
        {
          reportSpellDone = false;
          int maxApples = 1;
          if (RandHelper.GetRandomDouble() > 0.5)
            maxApples += 1;
          for (int appleIndex = 0; appleIndex < maxApples; appleIndex++)
          {
            var enemies = gm.CurrentNode.GetNeighborTiles<Enemy>(gm.Hero);
            bool added = false;
            foreach (var en in enemies)
            {
              var emptyOnes = gm.CurrentNode.GetEmptyNeighborhoodTiles(en, false);
              if (emptyOnes.Any())
              {
                AddApple(emptyOnes.First());
                added = true;
                break;
              }
            }
            if (!added)
            {
              var emp = gm.CurrentNode.GetClosestEmpty(gm.Hero);
              AddApple(emp);
            }
          }
        }
        else if (kind == SpellKind.CrackedStone)
        {
          gm.AppendTileByScrollUsage<Tile>(new CrackedStone(Container), destPoint.Value);
        }
        else if (spell is PassiveSpell ps1)
          caster.ApplyPassiveSpell(ps1);

        else if (spell is Portal)
          gm.AppendTileByScrollUsage<Tile>(spell as Portal, destPoint.Value);

        gm.UtylizeSpellSource(caster, spellSource, spell);
        var suffix = "";
        if (spellSource is Scroll)
          suffix = "scroll";
        else if (spellSource is Book)
          suffix = "book";

        var info = gm.Hero.Name + " used " + spellSource.Kind.ToDescription() + " " + suffix;

        gm.AppendAction((LivingEntityAction ac) =>
        {
          ac.Kind = LivingEntityActionKind.UsedSpell;
          ac.UsedSpellKind = spellSource.Kind;
          ac.Info = info;
          ac.InvolvedEntity = caster;
        });

        if(reportSpellDone)
          OnSpellDone(caster);
        else 
          OnReportDelayedSpellDone(1, caster);

        return spell;
      }
      else
      {
        gm.Logger.LogError("!PassiveSpell " + spellSource);
        gm.ReportFailure("");
      }

      return default(T);
    }

    public event EventHandler<ReportDelayedSpellDoneEventArgs> ReportDelayedSpellDone;

    protected virtual void OnReportDelayedSpellDone(float delaySecs, LivingEntity caster)
    {
      if (ReportDelayedSpellDone != null)
        ReportDelayedSpellDone(this, new ReportDelayedSpellDoneEventArgs() { DelaySecs = delaySecs , Caster = caster});
    }

    public OffensiveSpell ApplySpell(LivingEntity caster, SpellSource spellSource, Tile pointedTile = null)
    {
      var spell = spellSource.CreateSpell(caster);

      bool callSpellDone = true;
      var res = ApplyAttackPolicyResult.Unset;
      if (spell is OffensiveSpell os)
      {
        if (os is SkeletonSpell skeletonSpell)
        {
          gm.AddAlly(skeletonSpell.Ally);
          res = ApplyAttackPolicyResult.OK;
        }
        else if (spellSource is SwiatowitScroll)
        {
          var ems = gm.EnemiesManager.GetActiveEnemiesInvolved()
          .Where(i => i.DistanceFrom(gm.Hero) <= SwiatowitScroll.MaxRange)
          .OrderBy(i => i.DistanceFrom(gm.Hero))
          .Take(5);
          if (ems.Any())
          {
            HeroBulkAttackTargets = ems.Cast<Enemy>().ToList();
            res = ApplyAttackPolicy(gm.Hero, ems.First(), spellSource);
            callSpellDone = false;
          }
          else
          {
            gm.AppendAction(new GameEvent() { Info = "No valid targets for a spell" });
            return null;
          }
        }
        else if (spell is PerunSpell ps)
        {
          if (pointedTile is IDestroyable dest)
          {
            res = ApplyAttackPolicy(gm.Hero, dest, spellSource);
            callSpellDone = false;
          }
          else
          {
            gm.AppendAction(new GameEvent() { Info = "No valid targets for a spell" });
            return null;
          }
        }

        if (res != ApplyAttackPolicyResult.OK)
          return null;

        if (callSpellDone)
          OnSpellDone(caster);

        return os;
      }
      else
      {
        gm.Logger.LogError("!OffensiveSpell " + spellSource);
        gm.ReportFailure("");
      }

      return null;
    }

    public void OnSpellDone(LivingEntity caster)
    {
      if (caster is Hero)
        FinishHeroTurn(null);
    }

    bool applyingBulk = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="caster"></param>
    /// <param name="target"></param>
    /// <param name="spellSource"></param>
    /// <param name="BeforeApply"></param>
    /// <param name="AfterApply"></param>
    /// <param name="looped"></param>
    /// <returns></returns>
    public ApplyAttackPolicyResult ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      IHitable target,
      SpellSource spellSource,
      Action<Policy> BeforeApply = null,
      Action<Policy> AfterApply = null,
      bool looped = false

    )
    {
      if (!looped)
        applyingBulk = false;

      AttackPolicy policy = null;
      gm.Logger.LogInfo("SpellManager.ApplyAttackPolicy caster: " + caster + " target: " + target + " spellSource: " + spellSource);
      var spell = spellSource.CreateSpell(caster);
      var projectileSpell = spell as IProjectileSpell;

      if (projectileSpell != null)
      {
        if (!caster.IsInProjectileReach(projectileSpell, target.Position) /*&& target.Position.DistanceFrom(caster.Position) > 8*/)//HACK
        {
          if (caster is Hero)
          {
            gm.SoundManager.PlayBeepSound();
            gm.AppendAction(new GameEvent() { Info = "Target is out of range" });
          }
          return ApplyAttackPolicyResult.OutOfRange;
        }
      }

      if (!looped && !gm.UtylizeSpellSource(caster, spellSource, spell))
        return ApplyAttackPolicyResult.NotEnoughResources;

      if (projectileSpell != null)
      {
        var projectileCastPolicy = Container.GetInstance<ProjectileCastPolicy>();
        policy = projectileCastPolicy;
        projectileCastPolicy.CreateSpell(caster, spellSource);
      }
      else if (spell is PerunSpell)
      {
        var staticSpellCastPolicy = Container.GetInstance<StaticSpellCastPolicy>();
        policy = staticSpellCastPolicy;
        staticSpellCastPolicy.CreateSpell(caster, spellSource);

      }
      policy.AddTarget(target);

      policies.Add(policy);
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, e) =>
      {
        gm.CallTryAddForLootSource(target);

        if (looped)
          return;
        if (!applyingBulk)
        {
          var repeatOK = spellSource.Kind != SpellKind.Swiatowit && caster.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatElementalProjectileAttack);
          if (repeatOK)
          {
            ApplyAttackPolicy(caster, target, spellSource, BeforeApply, AfterApply, true);
          }
        }

        gm.FinishPolicyApplied(caster, AfterApply, policy);
      };

      policy.Apply(caster);

      var bulkOK = false;
      if (target is Enemy en &&
         (spellSource is WeaponSpellSource || spellSource.Kind == SpellKind.Swiatowit)
        )
      {
        Action<Enemy> funcApplyAttackPolicy = (en1) =>
        {
          applyingBulk = true;
          ApplyAttackPolicy(caster, en1, spellSource, BeforeApply, AfterApply, true);
          gm.AppendAction(caster.Name + " used Projectile Bulk Attack", ActionLevel.Important);
        };

        if (spellSource.Kind == SpellKind.Swiatowit)
          bulkOK = ApplyBulkAttack(EntityStatKind.ChanceToElementalProjectileBulkAttack, funcApplyAttackPolicy);
        else
          bulkOK = TryStartBulkAttack(en, EntityStatKind.ChanceToElementalProjectileBulkAttack, funcApplyAttackPolicy);
      }

      return ApplyAttackPolicyResult.OK;
    }

    protected bool TryStartBulkAttack(Enemy en, EntityStatKind esk, Action<Enemy> func)
    {
      gm.Logger.LogInfo("TryStartBulkAttack " + en + " esk:" + esk);
      var bulkOK = false;
      if (HeroBulkAttackTargets == null)
      {
        if (en != null)
        {
          FindBulkAttackTargets(en, esk);
          bulkOK = ApplyBulkAttack(esk, func);
          if (!bulkOK)
            HeroBulkAttackTargets = null;
        }
      }
      else
      {
        int k = 0;
        k++;
      }

      return bulkOK;
    }

    protected virtual bool ApplyBulkAttack(EntityStatKind esk, Action<Enemy> func)
    {
      bool bulkOK = HeroBulkAttackTargets.Any();
      if (bulkOK)
      {
        while (HeroBulkAttackTargets.Any())//simplest from of attack
        {
          var target = GetNextTarget();
          func(target);
        }
        HeroBulkAttackTargets = null;
      }
      return bulkOK;
    }
  }
}
