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





    void FindBulkAttackTargets(Enemy lastTarget)
    {
      HeroBulkAttackTargets = new List<Enemy>();
      var hero = gm.Hero;
      var sb = hero.GetTotalValue(EntityStatKind.ChanceToBulkAttack);
      //sb = 100.0f;
      if (sb > 0)
      {
        var fac = sb;// sb.GetFactor(true);
        if (fac / 100f > RandHelper.GetRandomDouble())
        {
          HeroBulkAttackTargets = gm.CurrentNode.GetNeighborTiles<Enemy>(hero)
          .Where(i => i != lastTarget)
          .ToList();

          if (HeroBulkAttackTargets.Any())
            gm.AppendAction(new LivingEntityAction(LivingEntityActionKind.BulkAttack)
            { Info = hero.Name + " used ability Bulk Attack", Level = ActionLevel.Important, InvolvedEntity = hero });
        }
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
        if (HeroBulkAttackTargets == null)
        {
          var ap = policy as AttackPolicy;
          var en = ap.Victim as Enemy;
          if (en != null)
          {
            FindBulkAttackTargets(ap.Victim as Enemy);
            if (HeroBulkAttackTargets.Any())
            {
              var target = HeroBulkAttackTargets.GetRandomElem();
              HeroBulkAttackTargets.Remove(target);
              gm.Context.ApplyPhysicalAttackPolicy(gm.Hero, target, (p) => { });
            }
          }
        }
      }

      HandleHeroActionDone();
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
