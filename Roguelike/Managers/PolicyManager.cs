using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
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
        HeroBulkAttackTargets = gm.CurrentNode.GetNeighborTiles<Enemy>(hero)
        .Where(i => i != lastTarget)
        .ToList();

        if (HeroBulkAttackTargets.Any())
          gm.AppendAction(new LivingEntityAction(LivingEntityActionKind.BulkAttack)
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
        var en = ap.Victim as Enemy;
        var bulkOK = HandleBulk(en, EntityStatKind.ChanceToBulkAttack);
        if (!bulkOK)
        {
          var repeatOK = gm.Hero.IsStatRandomlyTrue(EntityStatKind.ChanceToRepeatMelleeAttack);
          if (repeatOK)
            gm.Context.ApplyPhysicalAttackPolicy(gm.Hero, en, (p) => { });
        }
      }

      HandleHeroActionDone();
    }

    protected bool HandleBulk(Enemy en, EntityStatKind esk)
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
            var target = HeroBulkAttackTargets.GetRandomElem();
            HeroBulkAttackTargets.Remove(target);
            gm.Context.ApplyPhysicalAttackPolicy(gm.Hero, target, (p) => { });
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
