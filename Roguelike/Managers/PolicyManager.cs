using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Managers
{
  public class PolicyManager
  {
    public List<Enemy> HeroBulkAttackTargets { get; set; }
    protected GameManager gm;

    public Container Container
    {
      get;
      set;
    }


    public PolicyManager(GameManager mgr)
    {
      this.gm = mgr;
      Container = gm.Container;
    }

    void HandlePolicyApplied(Policy policy)
    {
      var attackPolicy = policy as AttackPolicy;

      gm.HandeTileHit(attackPolicy.Victim);
    }


    void FindBulkAttackTargets(Enemy lastTarget, EntityStatKind entityStatKind)
    {
      HeroBulkAttackTargets = new List<Enemy>();
      var hero = gm.Hero;
      var ok = hero.IsStatRandomlyTrue(entityStatKind);

      if (ok)
      {
        if (entityStatKind == EntityStatKind.ChanceToBulkAttack)
        {
          HeroBulkAttackTargets = gm.CurrentNode.GetNeighborTiles<Enemy>(hero)
          .Where(i => i != lastTarget)
          .ToList();
        }
        else 
        {
          HeroBulkAttackTargets = gm.EnemiesManager.GetInRange(hero, 7, lastTarget);
        }
        if (HeroBulkAttackTargets.Any())
          gm.AppendAction(new LivingEntityAction(LivingEntityActionKind.UsedAbility)
          { Info = hero.Name + " used ability Bulk Attack", Level = ActionLevel.Important, InvolvedEntity = hero });
      }
    }

    public virtual void OnHeroPolicyApplied(Policies.Policy policy)
    {
      if (policy.Kind == PolicyKind.Move)
      {

      }
      else if (policy.Kind == PolicyKind.Attack)
      {
        HandlePolicyApplied(policy);
        var ap = policy as AttackPolicy;
        var enemyVictim = ap.Victim as Enemy;
        if (enemyVictim != null)
        {
          if (enemyVictim.Alive && gm.Hero.CanUseAbility(Abilities.AbilityKind.StrikeBack))
          {
            gm.UseAbility(enemyVictim, gm.Hero, Abilities.AbilityKind.StrikeBack, false);
          }

          var bulkOK = HandleBulk(enemyVictim, EntityStatKind.ChanceToBulkAttack);
          if (!bulkOK)
          {
            var repeatOK = gm.Hero.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatMeleeAttack);
            if (repeatOK)
              gm.ApplyHeroPhysicalAttackPolicy(enemyVictim, false);
          }
        }
      }

      HandleHeroActionDone();
    }

    protected bool HandleBulk(Enemy en, EntityStatKind esk, Action<Enemy> func = null)
    {
      var bulkOK = false;
      if (HeroBulkAttackTargets == null)
      {
        if (en != null)
        {
          FindBulkAttackTargets(en, esk);
          bulkOK = HeroBulkAttackTargets.Any();
          //if (bulkOK && repeatOK)
          //  if (RandHelper.GetRandomDouble() > 0.5)
          //    repeatOK = false;
          if (bulkOK)
          {
            while (HeroBulkAttackTargets.Any())
            {
              gm.RemoveDead();
              var targets = HeroBulkAttackTargets.Where(i => i.Alive).ToList();
              var target = targets.GetRandomElem();
              if (target == null)
                break;
              HeroBulkAttackTargets.Remove(target);
              if (esk == EntityStatKind.ChanceToBulkAttack)
                gm.ApplyHeroPhysicalAttackPolicy(target, false);
              if (func != null)
                func(target);
            }
          }
        }
      }

      return bulkOK;
    }

    protected void HandleHeroActionDone()
    {
      HeroBulkAttackTargets = null;
      gm.RemoveDead();
      gm.Context.IncreaseActions(TurnOwner.Hero);
      gm.Context.MoveToNextTurnOwner();
    }
  }
}
