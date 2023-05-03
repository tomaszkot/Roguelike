using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Linq;
using System.Collections.Generic;

namespace Roguelike.Managers
{

  //if (ally is God god)
  //    {
  //      god.Point = Hero.Position;//TODO
  //      god.MakeTurn(this);
  //      return;
  //}
  public class AlliesManager : EntitiesManager
  {
    EnemiesManager enemiesManager;

    public AllyBehaviour AllyBehaviour { get; set; } = AllyBehaviour.GoFreely;

    public AlliesManager(GameContext context, EventsManager eventsManager, Container container, EnemiesManager enemiesManager, GameManager gm) :
                         base(TurnOwner.Allies, context, eventsManager, container, gm)
    {
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
      this.enemiesManager = enemiesManager;

    }

    public override void MakeTurn()
    {
      gameManager.OnBeforeAlliesTurn();
      base.MakeTurn();
      gameManager.OnAfterAlliesTurn();
    }

    protected override void MakeSpecialActions()
    {
      
    }

    public override bool RemoveDeadEntity(LivingEntity ent)
    {
      var res = RemoveEntity(ent);
      Context.EventsManager.AppendAction(new AllyAction() { Info = "Ally was lost", AllyActionKind = AllyActionKind.Died, InvolvedTile = ent as Ally });
      return res;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetActiveAllies();
      base.SetEntities(entities);
    }

    public override void MakeTurn(LivingEntity entity)
    {
      MakeHeroAllyMove(entity);
    }

    public void MakeHeroAllyMove(LivingEntity ally)
    {
      if (ally is God god)
      {
        god.MakeTurn();
        return;
      }
      if (!ally.Alive)
        return;

      if (ally.Stats.HealthBelow(0.5f))
      {
        var allyCasted = ally as Ally;
        if (allyCasted != null)
        {
          if (allyCasted.LastingEffects.Any(i => i.Type == Effects.EffectType.Poisoned))
          {
            var anty = allyCasted.Inventory.Items.FirstOrDefault(i => i is Potion pot && pot.Kind == PotionKind.Antidote);
            if (anty != null)
            {
              allyCasted.Consume(anty as Consumable);
              return;
            }
          }
          var food = allyCasted.Inventory.Items.FirstOrDefault(i => i is Consumable);
          if (food != null)
          {
            allyCasted.Consume(food as Consumable);
            return;
          }
        }
      }

      //if (ally.FixedWalkTarget != null)
      //{
      //  if (ally.PathToTarget != null && ally.PathToTarget.Count == 1)
      //  {
      //    ally.FixedWalkTarget = null;
      //  }
      //  if (!MakeMoveOnPath(ally, ally.FixedWalkTarget.point, true))
      //    MakeRandomMove(ally);
      //  return;
      //}
      var allEnemies = enemiesManager.GetActiveEnemies();
      var enemiesInvolved = enemiesManager.GetActiveEnemiesInvolved();
      if (AllyBehaviour == AllyBehaviour.StayStill)
      {
        FindTarget(ally, allEnemies, enemiesInvolved, 1, ally);
        attackStrategy.AttackIfPossible(ally, ally.AllyModeTarget);
        return;
      }

      
      if (AllyBehaviour == AllyBehaviour.StayClose)
      {
        DoMixedMode(AllyBehaviour.StayClose, ally, allEnemies, enemiesInvolved, 3);
      }
      else if (AllyBehaviour == AllyBehaviour.GoFreely)
      {
        DoMixedMode(AllyBehaviour.GoFreely, ally, allEnemies, enemiesInvolved, MaxEntityDistanceToToChase);
      }
    }

    private void DoMixedMode(AllyBehaviour allyBehaviour, LivingEntity ally, List<Enemy> allEnemies, List<Enemy> enemiesInvolved, int maxEntityDistanceToToChase)
    {
      bool moveCloserToHero = false;
      LivingEntity maxDistanceFrom = allyBehaviour == AllyBehaviour.GoFreely ? ally: Hero;
      FindTarget(ally, allEnemies, enemiesInvolved, maxEntityDistanceToToChase, ally);

      if (ally.AllyModeTarget == null)
        moveCloserToHero = true;
      else
      {
        if (attackStrategy.AttackIfPossible(ally, ally.AllyModeTarget))
          return;
        if (ShallChaseTarget(ally, ally.AllyModeTarget, maxEntityDistanceToToChase, maxDistanceFrom))
        {
          moveCloserToHero = !MakeMoveOnPath(ally, ally.AllyModeTarget.point, true);
        }
      }

      if (moveCloserToHero)
      {
        OnMoveCloseToHero(ally);
      }
    }

    private void FindTarget(LivingEntity ally, 
      List<Enemy> allEnemies, 
      List<Enemy> enemiesInvolved,
      int maxDistance,
      LivingEntity maxDistanceFrom
      )
    {
      if (ally.AllyModeTarget != null)
      {
        var found = allEnemies.Where(i => i == ally.AllyModeTarget).FirstOrDefault();
        if (found == null)//dead?
          ally.AllyModeTarget = null;
      }

      if (ally.AllyModeTarget == null || ally.AllyModeTarget.DistanceFrom(maxDistanceFrom) > maxDistance)
      {
        ally.AllyModeTarget = enemiesInvolved
          .Where(i => i.Revealed && i.DistanceFrom(maxDistanceFrom) <= maxDistance)
          .OrderBy(i => i.DistanceFrom(ally))
          .ToList()
          .FirstOrDefault();
      }
    }

    private void OnMoveCloseToHero(LivingEntity ally)
    {
      if (ally.DistanceFrom(context.Hero) > 2)//do not block hero moves
      {
        //user false here as forHeroAlly as we want to find a hero on path
        MakeMoveOnPath(ally, context.Hero.point, false);
      }
      else
      {
        ally.AllyModeTarget = null;//find a new target
        if (RandHelper.Random.NextDouble() > .5f)
          MakeRandomMove(ally);
      }
    }

    protected override void OnPolicyAppliedAllIdle()
    {
      if (context.TurnOwner == TurnOwner.Allies)//for ASCII/UT
      {
        context.IncreaseActions(TurnOwner.Allies);
        base.OnPolicyAppliedAllIdle();
      }
    }

    private void OnTurnOwnerChanged(object sender, TurnOwner turnOwner)
    {
    }

    public override void AddEntity(LivingEntity ent)
    {
      if (ent is IAlly ially)
      {
        ially.AllyBehaviour = AllyBehaviour;
        base.AddEntity(ent as LivingEntity);
      }
      else
        throw new Exception("AddEntity ent is !IAlly");
    }

    public void AddEntity(IAlly ent)
    {
      AddEntity(ent as LivingEntity);
    }

    //public God RemoveGod()
    //{
    //  var god = AllEntities.Where(i => i is God).FirstOrDefault();
    //  AllEntities.Remove(god);
    //  return god as God;
    //}
  }
}
