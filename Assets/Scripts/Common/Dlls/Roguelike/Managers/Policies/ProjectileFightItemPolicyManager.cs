using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Abilities;
using Roguelike.Abstract.Projectiles;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Policies;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Roguelike.Managers.Policies
{
  internal class ProjectileFightItemPolicyManager
  {
    GameManager gm;
    internal ProjectileFightItemPolicyManager(GameManager gm)
    {
      this.gm = gm;
    }

    public void Log(string log)
    {
      //logger.LogInfo("gm: "+log);
    }

    public ILogger Logger { get => gm.Logger;  }
    public Hero Hero { get => gm.Hero; }

    public bool ApplyAttackPolicy
    (
      LivingEntity caster,//hero, enemy, ally
      Dungeons.Tiles.IHitable target,
      ProjectileFightItem pfi,
      Action<Policy> BeforeApply = null,
      Action<Policy> AfterApply = null
    )
    {
      Log("ApplyAttackPolicy start " + pfi.FightItemKind + " on " + target);
      if (pfi.Count <= 0 && !pfi.EndlessAmmo)
      {
        Logger.LogError("gm fi.Count <= 0");
        gm.ReportFailure("out of prejectiles");
        return false;
      }
      var ab = caster.SelectedActiveAbility;
      var maxVictims = gm.GetAttackVictimsCount(caster);
      if (maxVictims > pfi.Count)
        maxVictims = pfi.Count;

      var fiCountToRemove = 1;
      var abUsed = false;
      if (ab == null)
        ab = caster.GetActiveAbility(FightItem.GetAbilityKind(pfi));
      if (ab != null)
      {
        string reason;
        var canUse = caster.CanUseAbility(ab.Kind, gm.CurrentNode, out reason, target);
        if (!canUse)
        {
          if (ab.Kind == AbilityKind.Cannon || ab.Kind == AbilityKind.Smoke)
          {
            if (!canUse)
            {
              var message = "Can not use " + ab.Kind;
              if (reason.Any())
                message += " - " + reason;
              gm.ReportFailure(message);
              return false;
            }
          }
          else
            maxVictims = 1;
        }
        else
          abUsed = true;

        if (ab.Kind == AbilityKind.ArrowVolley ||
            ab.Kind == AbilityKind.Cannon)
        {
          fiCountToRemove = maxVictims;
        }
      }

      bool res = false;
      if (ab != null && ab.Kind == AbilityKind.Smoke)
      {
        res = gm.UseActiveAbility(ab as ActiveAbility, caster, true);
      }
      else
      {
        pfi.AttackDescription = gm.CreateAttackDescription(caster, pfi);

        var destFi = pfi.Clone(maxVictims) as ProjectileFightItem;

        res = DoApply(caster, target, destFi, maxVictims, BeforeApply, AfterApply);

        var advCaster = caster as AdvancedLivingEntity;
        var cb = 0;
        if (advCaster != null)
          cb = advCaster.Inventory.GetStackedCount(pfi);
        for (int i = 0; i < destFi.Count; i++)//destFi.Count has amount of thrown items
        {
          if (!caster.RemoveFightItem(pfi))
            Logger.LogError("RemoveFightItem failed " + pfi + ", " + caster);
        }

        var ca = 0;
        if (advCaster != null)
          ca = advCaster.Inventory.GetStackedCount(pfi);
        var diff = cb - ca;
      }
      if (abUsed)
        gm.HandleActiveAbilityUsed(caster, ab.Kind);

      Log("ApplyAttackPolicy done res: " + res);
      Log("");
      Log("");
      return res;
    }

    private bool DoApply
    (
      LivingEntity caster,
      Dungeons.Tiles.IHitable target,
      ProjectileFightItem fi,
      int maxVictimsCount,
      Action<Policy> BeforeApply,
      Action<Policy> AfterApply
    )
    {
      fi.Caller = caster;
      var policy = gm.Container.GetInstance<ProjectileCastPolicy>();
      policy.Kind = PolicyKind.ProjectileCast;
      policy.AddTarget(target);
      policy.GameManager = gm;


      policy.MaxVictimsCount = maxVictimsCount;
      policy.ProjectilesFactory = gm.Container.GetInstance<IProjectilesFactory>();
      policy.Projectile = fi;
      fi.MaxVictimsCount = maxVictimsCount;
      policy.Caster = caster;
      if (BeforeApply != null)
        BeforeApply(policy);

      policy.OnTargetHit += (s, e) =>
      {
        gm.CallTryAddForLootSource(e);
      };

      policy.OnApplied += (s, e) =>
      {
        //CallTryAddForLootSource(policy);

        gm.FinishPolicyApplied(caster, AfterApply, policy);
      };

      policy.Apply(caster);
      return true;
    }

    public bool TryApplyAttackPolicy(ProjectileFightItem fi, Tile pointedTile, Action<Tile> beforAttackHandler = null)
    {
      Log("TryApplyAttackPolicy start");
      if (!gm.CanHeroDoAction(false))
        return false;

      var hero = this.Hero;
      fi.Caller = this.Hero;

      if (fi.FightItemKind.IsBowLikeAmmunition())
      {
        var wpn = hero.GetActiveWeapon();
        if (wpn == null
           || (wpn.Kind != Roguelike.Tiles.Looting.Weapon.WeaponKind.Crossbow &&
               wpn.Kind != Roguelike.Tiles.Looting.Weapon.WeaponKind.Bow))
        {
          gm.AppendAction(new Roguelike.Events.GameEvent() { Info = "Proper weapon not equipped" });
          return false;
        }
      }

      var target = pointedTile;/// CurrentGameGrid.GetTileAt(pointedTile);

      if (fi.RangeBasedCasting)
      {
        var inReach = hero.IsInProjectileReach(fi, target.point);

        if (!inReach)// && target.DistanceFrom(hero) > 8)//HACK
        {
          gm.SoundManager.PlayBeepSound();
          gm.AppendAction(new GameEvent() { Info = "Target is out of range" });
          return false;
        }
      }

      FakeTarget fakeTarget;
      if (fi.RequiresEmptyCellOnCast && target.IsEmpty)
      {
        fakeTarget = new FakeTarget();
        fakeTarget.point = target.point;
        target = fakeTarget;
      }

      var hitable = target as Dungeons.Tiles.IHitable;
      if (hitable == null)
      {
        var oil = gm.GetSurfacesOil().GetAt(target.point);
        if (oil != null)
          hitable = oil as HitableSurface;
      }
      if (hitable != null || fi.FightItemKind == FightItemKind.Smoke)
      {
        if (beforAttackHandler != null)
          beforAttackHandler(target);
        return ApplyAttackPolicy(hero, hitable, fi);
      }

      gm.SoundManager.PlayBeepSound();
      return false;
    }
  }
}
