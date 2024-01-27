using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Roguelike.Abilities;
//using NUnit.Framework.Interfaces;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Linq;
using System.Reflection;

namespace Roguelike.Managers.Policies
{
  public class MeleePolicyManager : PolicyManager
  {

    public MeleePolicyManager(GameManager mgr) : base(mgr)
    {
      gm = mgr;
      Container = gm.Container;
    }

    public void ApplyPhysicalAttackPolicy(LivingEntity attacker, IHitable target, Action<Policy> afterApply, MeleeAttackPolicy policy,
      EntityStatKind esk)
    {
      //gm.Logger.LogInfo("ApplyPhysicalAttackPolicy: "+ attacker + ", target: "+ target);

      if (HeroBulkAttackTargets != null)
      {
        gm.Logger.LogError("HeroBulkAttackTargets != null");
        HeroBulkAttackTargets = null;
      }

      if (GetPolicies(gm.Hero).Any())
        gm.Logger.LogError("GetPolicies(gm.Hero).Any()");

      if (attacker == gm.Hero)
      {
        if (policies.Any())
          policies.Clear();
      }
      ApplyPhysicalAttackPolicyInner(attacker, target, afterApply, policy, esk);
    }

    private void ApplyPhysicalAttackPolicyInner(LivingEntity attacker, IHitable target, Action<Policy> afterApply, 
      MeleeAttackPolicy policy, EntityStatKind esk)
    {
      gm.Hero.PathToTarget = null;//https://github.com/users/tomaszkot/projects/1 11 Automatic turns while fighting
      var attackPolicy = policy ?? Container.GetInstance<MeleeAttackPolicy>();
      if (esk == EntityStatKind.ChanceToStrikeBack || esk == EntityStatKind.ChanceToRepeatMeleeAttack)
        attackPolicy.ChangesTurnOwner = false;
      if (target is Tiles.Interactive.TorchSlot)//TODO to avoid TurnOwner mismatch!, turn will be changed on Collect of torch
        attackPolicy.ChangesTurnOwner = false;

      if (gm.AttackPolicyInitializer != null)
        gm.AttackPolicyInitializer(attackPolicy, attacker, target);

      attackPolicy.TargetHit += (object sender, IHitable e)=>
      {
        HandeTileHit(attacker, attackPolicy.Victim, attackPolicy);
      };

      policies.Add(attackPolicy);
      attackPolicy.OnApplied += (s, policy) =>
      {
        OnPolicyApplied(attacker, target, afterApply, policy);
      };

      //do it
      attackPolicy.Apply(attacker, target);
    }


    private void OnPolicyApplied(LivingEntity attacker, IHitable target, Action<Policy> afterApply, Policy policy)
    {
      if (afterApply != null)
        afterApply(policy);

      if (attacker is Hero)
        OnPolicyApplied(policy);

      var attackPolicy = policy as MeleeAttackPolicy;

      var enemyVictim = attackPolicy.Victim as Enemy;
      if (enemyVictim != null && attacker is Hero)
      {
        MakeNextAttack(policy, enemyVictim);
      }

      if (target is LivingEntity targetLe && targetLe.Alive)
      {
        bool done = false;
        if (attacker is AdvancedLivingEntity ale)
          done = gm.AbilityManager.UseActiveAbility(ale, false, targetLe);

        string reason;
        if (targetLe.CanUseAbility(AbilityKind.StrikeBack, gm.CurrentNode, out reason))
        {
          gm.AbilityManager.UseAbility(attacker, targetLe, AbilityKind.StrikeBack, activeAbility : false);
        }
      }

      if (attacker is Hero)
      {
        if (policy.ChangesTurnOwner
        && !policy.Bulked//|| AllowBulkPolicyFinishHeroTurn)//ut TestBulkAttackReal failed without it , ut is synhronus and that why it fails
        && (HeroBulkAttackTargets == null || !HeroBulkAttackTargets.Any())
        && !GetPolicies(attacker).Any())
        {
          FinishHeroTurn(policy);
        }
      }
    }

    public static bool AllowBulkPolicyFinishHeroTurn = true;

    protected bool MakeNextAttack(Policy policy, Enemy enemyVictim)
    {
      var ap = policy as MeleeAttackPolicy;

      if (!policy.Bulked)
      {
        FindBulkAttackTargets(enemyVictim, EntityStatKind.Unset);//for Zeal attack
        if (!HasBulkAttackVictims())
          FindBulkAttackTargets(enemyVictim, EntityStatKind.ChanceToBulkAttack);//for rand bulk attack
      }
      if (HasBulkAttackVictims())
      {
        return AttackNextBulkTarget();
      }
      else
      {
        var repeatOK = gm.Hero.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatMeleeAttack);
        if (repeatOK)
        {
          ApplyPhysicalAttackPolicy(ap.Attacker, enemyVictim, null, null, EntityStatKind.ChanceToRepeatMeleeAttack);
          return true;
        }
      }


      return false;
    }

    private bool HasBulkAttackVictims()
    {
      return HeroBulkAttackTargets != null && HeroBulkAttackTargets.Any();
    }

    protected bool AttackNextBulkTarget()
    {
      MeleeAttackPolicy pol;
      var target = CreateNext(out pol);
      if(target!=null)
        ApplyPhysicalAttackPolicyInner(gm.Context.Hero, target, null, pol, EntityStatKind.Unset);

      return target != null;
    }
  }
}
