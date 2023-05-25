using Dungeons.Core;
using Dungeons.Core.Policy;
using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Roguelike.Abilities;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Managers.Policies
{
  public class PolicyManager
  {
    protected List<Policy> policies = new List<Policy>();
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

    protected virtual void HandeTileHit(LivingEntity attacker, IHitable hitTile, Policy policy)
    {
      gm.HandeTileHit(policy, attacker, hitTile);
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="lastTarget"></param>
    /// <param name="entityStatKind">can be ChanceToBulkAttack || ChanceToElementalProjectileBulkAttack </param>
      protected List<Enemy> FindBulkAttackTargets(Enemy lastTarget, EntityStatKind entityStatKind)
    {
      HeroBulkAttackTargets = new List<Enemy>();
      var hero = gm.Hero;
      var bulkFromZealAttack = hero.SelectedActiveAbility != null && hero.SelectedActiveAbility.Kind == Abilities.AbilityKind.ZealAttack;
      string reason;
      if (bulkFromZealAttack && !hero.CanUseAbility(AbilityKind.ZealAttack, gm.CurrentNode, out reason))
        return HeroBulkAttackTargets;
      var ok = hero.IsStatRandomlyTrue(entityStatKind) || bulkFromZealAttack;

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
        {
          if (bulkFromZealAttack)
          {
            {
              HeroBulkAttackTargets = HeroBulkAttackTargets.Take((int)hero.SelectedActiveAbility.PrimaryStat.Factor).ToList();
              //HeroBulkAttackTargets.RemoveRange(hero.SelectedActiveAbility.PrimaryStat.Factor, HeroBulkAttackTargets.Count - hero.SelectedActiveAbility.PrimaryStat.Factor)
            }
          }
          var ak = AbilityKind.BulkAttack;
          if (bulkFromZealAttack)
            ak = AbilityKind.ZealAttack;
          if (ak == AbilityKind.BulkAttack)
            gm.AppendUsedAbilityAction(hero, ak);

        }
      }

      return HeroBulkAttackTargets;
    }

    protected virtual void OnPolicyApplied(Policy policy)
    {
      if (!policies.Contains(policy))
        return;
      policies.Remove(policy);
      //HandleHeroActionDone(policy);
    }

    protected void FinishHeroTurn(Policy policy)
    {
      HeroBulkAttackTargets = null;
      gm.FinishHeroTurn(policy);
    }

    protected List<Policy> GetPolicies(LivingEntity attacker)
    {
      return policies.Where(i => i is MeleeAttackPolicy ap && ap.Attacker == attacker).ToList();
    }

    protected Enemy CreateNext(out MeleeAttackPolicy ap)
    {
      ap = null;
      var target = GetNextTarget();
      if(target == null)
        return null;
      ap = Container.GetInstance<MeleeAttackPolicy>();
      ap.Bulked = true;
      return target;
    }

    protected Enemy GetNextTarget()
    {
      gm.RemoveDead();
      var targets = HeroBulkAttackTargets.Where(i => i.Alive).ToList();
      var rand = false;
      var target = rand ? targets.GetRandomElem() : targets.First();
        if (target == null)
          return null;
      HeroBulkAttackTargets.Remove(target);
      return target;
    }

    //protected virtual bool HandlePolicyEnd(Policies.Policy policy, Enemy enemyVictim)
    //{
    //  //if (HeroBulkAttackTargets == null)
    //  //{
    //  //  TryStartBulkAttack(enemyVictim, EntityStatKind.ChanceToBulkAttack);//bulk spell
    //  //}

    //  return true;
    //}
  }
   
}
