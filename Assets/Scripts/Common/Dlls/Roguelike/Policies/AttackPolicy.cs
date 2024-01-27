using Dungeons.Core;
using Dungeons.Core.Policy;
using Dungeons.Fight;
using Dungeons.Tiles;
using Dungeons.Tiles.Abstract;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Roguelike.Policies
{
  public abstract class AttackPolicy : Policy
  {
    public List<Dungeons.Tiles.Abstract.IHitable> Targets = new List<Dungeons.Tiles.Abstract.IHitable>();
    public abstract void AttackNextTarget
    (
      LivingEntity caster,
      Dungeons.Tiles.Abstract.IHitable nextTarget
    );

    public void AddTarget(Dungeons.Tiles.Abstract.IHitable obstacle)
    {
      Targets.Add(obstacle);
    }

    public abstract ISpell CreateSpell(LivingEntity caster, SpellSource spellSource);

    public virtual void TryAttack(IPolicy policy, LivingEntity attacker, IHitable le)
    {
      attacker.LastMeleeAttackWasOK = false;
      if (TryAttack(policy, attacker, le, AttackKind.Melee, null) == Dungeons.Fight.HitResult.Hit)
        attacker.LastMeleeAttackWasOK = true;

    }

    protected virtual HitResult TryAttack(IPolicy policy, LivingEntity attacker, 
      IHitable target, AttackKind ak, IProjectile proj)
    {
      if (attacker == null)
        return HitResult.Unset;
      if (attacker.CalculateIfHitWillHappen(target, ak, null))
      {
        if (ak == AttackKind.Melee)
          target.OnHitBy(attacker);
        else //if (ak == AttackKind.PhysicalProjectile)
          target.OnHitBy(proj, policy);
        //else
        //  return HitResult.Unset;
        policy.ReportHit(target);
        return HitResult.Hit;
      }
      
      attacker.EventsManager.AppendAction(new LivingEntityAction(LivingEntityActionKind.Missed)
      {
        InvolvedEntity = attacker,
        Info = attacker.Name + " missed " + target.Name,
        AttackKind = ak
      });
      attacker.Container.GetInstance<ILogger>().LogInfo(attacker + " missed " + target + " using "+ proj);
      return HitResult.Evaded;
      
    }
  }
}
