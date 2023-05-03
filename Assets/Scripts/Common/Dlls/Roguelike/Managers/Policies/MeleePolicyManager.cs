using Dungeons.Tiles;
//using NUnit.Framework.Interfaces;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Linq;

namespace Roguelike.Managers.Policies
{
  public class MeleePolicyManager : PolicyManager
  {

    public MeleePolicyManager(GameManager mgr) : base(mgr)
    {
      gm = mgr;
      Container = gm.Container;
    }

    public void ApplyPhysicalAttackPolicy(LivingEntity attacker, Tile target, Action<Policy> afterApply, MeleeAttackPolicy policy,
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

    private void ApplyPhysicalAttackPolicyInner(LivingEntity attacker, Tile target, Action<Policy> afterApply, MeleeAttackPolicy policy, EntityStatKind esk)
    {
      gm.Hero.PathToTarget = null;//https://github.com/users/tomaszkot/projects/1 11 Automatic turns while fighting
      var attackPolicy = policy ?? Container.GetInstance<MeleeAttackPolicy>();
      if (esk == EntityStatKind.ChanceToStrikeBack || esk == EntityStatKind.ChanceToRepeatMeleeAttack)
        attackPolicy.ChangesTurnOwner = false;
      if (target is Tiles.Interactive.TorchSlot)//TODO to avoid TurnOwner mismatch!, turn will be changed on Collect of torch
        attackPolicy.ChangesTurnOwner = false;

      if (gm.AttackPolicyInitializer != null)
        gm.AttackPolicyInitializer(attackPolicy, attacker, target);

      policies.Add(attackPolicy);
      attackPolicy.OnApplied += (s, policy) =>
      {
        OnPolicyApplied(attacker, target, afterApply, policy);
      };

      //do it
      attackPolicy.Apply(attacker, target);
    }

    private void OnPolicyApplied(LivingEntity attacker, Tile target, Action<Policy> afterApply, Policy policy)
    {
      if (afterApply != null)
        afterApply(policy);

      if (attacker is Hero)
        OnPolicyApplied(policy);

      var attackPolicy = policy as MeleeAttackPolicy;
      gm.HandeTileHit(attacker, attackPolicy.Victim, attackPolicy);

      var ap = policy as MeleeAttackPolicy;
      var enemyVictim = ap.Victim as Enemy;
      if (enemyVictim != null && attacker is Hero)
      {
        MakeNextAttack(policy, enemyVictim);
      }

      if (target is LivingEntity targetLe && targetLe.Alive)
      {
        {
          bool done = false;
          if (attacker is AdvancedLivingEntity ale)
          {
            done = gm.UseActiveAbilities(attacker, targetLe, false);
          }
          string reason;
          if (targetLe.CanUseAbility(Abilities.AbilityKind.StrikeBack, gm.CurrentNode, out reason))
          {
            //TODO ?
            gm.UseAbility(attacker, targetLe, Abilities.AbilityKind.StrikeBack, false);

            //if(used)
            //  AppendUsedAbilityAction(attacker, Abilities.AbilityKind.StrikeBack);
          }
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
      //StrikeBack shall be done when hero is hit
      //if (enemyVictim.Alive && gm.Hero.CanUseAbility(Abilities.AbilityKind.StrikeBack))
      //{
      //  gm.UseAbility(enemyVictim, gm.Hero, Abilities.AbilityKind.StrikeBack, false);
      //}
      //if (HeroBulkAttackTargets == null)
      {
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
      }
      //else if (!AttackNextBulkTarget(EntityStatKind.ChanceToBulkAttack, null))
      //{
      //  int k = 0;
      //  k++;
      //}

      return false;
    }

    private bool HasBulkAttackVictims()
    {
      return HeroBulkAttackTargets != null && HeroBulkAttackTargets.Any();
    }

    //protected bool HandleHeroAttackPolicyDone(Enemy en, Policy policy)
    //{
    //  if (HeroBulkAttackTargets == null)
    //  {
    //    FindBulkAttackTargets(en, EntityStatKind.ChanceToBulkAttack);
    //  }
    //  if (!HeroBulkAttackTargets.Any())
    //  {
    //    FinishHeroTurn(policy);
    //  }
    //  else
    //    AttackNextBulkTarget();

    //  return true;
    //}


    protected bool AttackNextBulkTarget()
    {
      MeleeAttackPolicy pol;
      var target = CreateNext(out pol);
      ApplyPhysicalAttackPolicyInner(gm.Context.Hero, target, null, pol, EntityStatKind.Unset);

      return target != null;
    }



    ///// <summary>
    ///// Can  be melee or a spell
    ///// </summary>
    ///// <param name="esk"></param>
    ///// <param name="func"></param>
    ///// <returns></returns>
    //protected override bool ApplyBulkAttack(EntityStatKind esk, Action<Enemy> func)
    //{
    //  bool bulkOK = HeroBulkAttackTargets.Any();
    //  if (bulkOK)
    //  {
    //    AttackNextBulkTarget(esk, func);
    //  }
    //  return bulkOK;
    //}






  }
}
