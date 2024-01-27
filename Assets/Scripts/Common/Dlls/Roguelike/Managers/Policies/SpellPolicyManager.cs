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
using Dungeons.Core.Policy;
using Dungeons.Tiles.Abstract;
using Roguelike.Effects;
using System.Collections.Generic;
using Roguelike.Abstract.Tiles;

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

    public bool UsePassiveSpell
    (
      Tile pointedTile, 
      LivingEntity caster,
      Dungeons.Tiles.Abstract.ISpell spell, 
      bool applied, 
      SpellSource spellSource
    )
    {
      Point? pt = null;
      bool spellReqPoint = false;
      if (spell is Spell sp && sp.RequiresDestPoint)
      {
        if (pointedTile != null)
          pt = pointedTile.point;
      }
      if (pt != null || !spellReqPoint)
      {
        applied = gm.SpellManager.ApplyPassiveSpell<PassiveSpell>(caster, spellSource, pt) != null;
      }

      return applied;
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
          var ems = gm.EnemiesManager.GetActiveEnemiesInvolved();
          foreach (var en in ems)
          {
            en.HitRandomTarget = true;
          }
        }
        else if (kind == SpellKind.Frighten)
        {
          var typedSpell = spell as FrightenSpell;
          var ems = gm.EnemiesManager.GetActiveEnemiesInvolved().Where(i=>i.DistanceFrom(caster) <= typedSpell.Range);
          foreach (var en in ems)
          {
            en.LastingEffectsSet.AddLastingEffectFromSpell(Effects.EffectType.Frighten, spell);
          }
          gm.SoundManager.PlaySound("hey");

        }
        else if (kind == SpellKind.Dziewanna)
        {
          reportSpellDone = ApplyDziewannaSpell();
        }
        else if (kind == SpellKind.Jarowit)
        {
          ApplyJarowitSpell(caster, spell);
        }
        else if (kind == SpellKind.Wales)
        {
          ApplyWalesSpell(kind);
        }
        else if (kind == SpellKind.CrackedStone)
        {
          gm.AppendTileByScrollUsage<Tile>((spell as CrackedStoneSpell).TypedTile, destPoint.Value);
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

        var info = caster is God ? "God "+ caster.Name + " manifested its power" :
          gm.Hero.Name + " used " + spellSource.Kind.ToDescription() + " " + suffix;

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

    private bool ApplyDziewannaSpell()
    {
      bool reportSpellDone = false;
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

      return reportSpellDone;
    }

    private void ApplyJarowitSpell(LivingEntity caster, Abstract.Spells.ISpell spell)
    {
      var iron = new IronSkinSpell(gm.Hero);
      gm.Hero.LastingEffectsSet.AddLastingEffectFromSpell(Effects.EffectType.IronSkin, iron);

      foreach (var ally in gm.AlliesManager.AllAllies)
      {
        if (ally is God)
          continue;
        (ally as LivingEntity).LastingEffectsSet.AddLastingEffectFromSpell(Effects.EffectType.IronSkin, iron);
      }

      var typedSpell = spell as JarowitSpell;
      var ems = gm.EnemiesManager.GetActiveEnemiesInvolved().Where(i => i.DistanceFrom(caster) <= typedSpell.Range);
      foreach (var en in ems)
      {
        var weak = new WeakenSpell(en);
        en.LastingEffectsSet.AddLastingEffectFromSpell(Effects.EffectType.Weaken, weak);
      }
    }

    private void ApplyWalesSpell(SpellKind kind)
    {
      var les = gm.GetLivingEntitiesForGodSpell(true, kind);
      var laEffs = new[] { EffectType.Poisoned, EffectType.Frozen, EffectType.Firing };
      foreach (var le in les)
      {
        le.ConsumePotion(new Potion(PotionKind.Health));
        le.ConsumePotion(new Potion(PotionKind.Mana));
        //le.ConsumePotion(new Potion(PotionKind.Antidote));

        foreach (var lef in laEffs)
        {
          var eff = le.LastingEffectsSet.GetByType(lef);
          if (eff != null)
            le.RemoveLastingEffect(eff);
        }
      }
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
          if (gm.AddAlly(skeletonSpell.Ally))
            res = ApplyAttackPolicyResult.OK;
          else
            res = ApplyAttackPolicyResult.NotEnoughResources;
        }
        else if (spellSource is SwiatowitScroll)
        {
          var ems = GetEnemiesForGodAttack();
          if (ems.Any())
          {
            HeroBulkAttackTargets = ems.Cast<Enemy>().ToList();
            res = ApplyAttackPolicy(caster, ems.First(), spellSource);
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
          var ems = GetEnemiesForGodAttack();
          if (ems.Any())
          {
            IHitable target = ems.FirstOrDefault();
            var le = pointedTile as LivingEntity;
            if (le != null && gm.EnemiesManager.Contains(le))
            {
              target = le;
            }
            if (target != null)
            {
              res = ApplyAttackPolicy(caster, target, spellSource);
              callSpellDone = false;
            }
            else
            {
              gm.AppendAction(new GameEvent() { Info = "No valid targets for a spell" });
              return null;
            }
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

    public IEnumerable<LivingEntity> GetEnemiesForGodAttack()
    {
      return gm.GetEnemiesForGodAttack(SwiatowitScroll.MaxRange);
                
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

      if (spellSource == null)
      {
        gm.Logger.LogInfo("Error: ApplyAttackPolicy !spellSource, "+ caster);
        return ApplyAttackPolicyResult.NotEnoughResources;
      }
      AttackPolicy policy = null;
      gm.Logger.LogInfo("SpellManager.ApplyAttackPolicy caster: " + caster + " target: " + target + " spellSource: " + spellSource);
            //var spell = spellSource.CreateSpell(caster);


      Abstract.Spells.ISpell spell = null;
      if (spellSource.IsProjectile)
      {
        var projectileCastPolicy = Container.GetInstance<ProjectileCastPolicy>();
        policy = projectileCastPolicy;
        spell = projectileCastPolicy.CreateSpell(caster, spellSource) as Abstract.Spells.ISpell;
      }
      else if (spellSource.Kind == SpellKind.Perun)//TODO
      {
        var staticSpellCastPolicy = Container.GetInstance<StaticSpellCastPolicy>();
        policy = staticSpellCastPolicy;
        spell = staticSpellCastPolicy.CreateSpell(caster, spellSource) as Abstract.Spells.ISpell ;
      }
      else
        spell = spellSource.CreateSpell(caster);

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

      policy.AddTarget(target);

      policies.Add(policy);

      policy.TargetHit += (s, hitObject) =>
      {
        HandeTileHit(caster, hitObject, policy);
      };

      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnApplied += (s, ev) =>
      {
       // gm.CallTryAddForLootSource(target, policy);
        //HandeTileHit(caster, target as Tile, policy);
        if (looped)
          return;

        var destr = target as IDestroyable;

        if (!applyingBulk && 
        (destr == null || !destr.Destroyed))//sec ball hanged
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
        while (HeroBulkAttackTargets !=null && HeroBulkAttackTargets.Any())//simplest from of attack
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
