﻿using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Linq;

namespace Roguelike.Managers
{
  public class AlliesManager : EntitiesManager
  {
    EnemiesManager enemiesManager;

    public AlliesManager(GameContext context, EventsManager eventsManager, Container container, EnemiesManager enemiesManager, GameManager gm) :
                         base(TurnOwner.Allies, context, eventsManager, container, gm)
    {
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
      this.enemiesManager = enemiesManager;

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
      if (!ally.Alive)
        return;

      if (ally.Stats.HealthBelow(0.5f))
      {
        var allyCasted = ally as Ally;
        if (allyCasted != null)
        {
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

      if (ally.AllyModeTarget != null)
      {
        var found = enemiesManager.AllEntities.Where(i => i == ally.AllyModeTarget).FirstOrDefault();
        if (found == null)//dead?
          ally.AllyModeTarget = null;
      }

      if (ally.AllyModeTarget == null || ally.AllyModeTarget.DistanceFrom(ally) >= MaxEntityDistanceToToChase)
      {
        ally.AllyModeTarget = enemiesManager.AllEntities.Where(i => i.DistanceFrom(ally) < MaxEntityDistanceToToChase).OrderBy(i => i.DistanceFrom(ally)).ToList().FirstOrDefault();
      }

      bool moveCloserToHero = false;
      if (ally.AllyModeTarget == null)
        moveCloserToHero = true;
      else
      {
        if (attackStrategy.AttackIfPossible(ally, ally.AllyModeTarget))
          return;
        if (ShallChaseTarget(ally, ally.AllyModeTarget))//, MaxAllyDistToEnemyToChase))
        {
          moveCloserToHero = !MakeMoveOnPath(ally, ally.AllyModeTarget.point, true);//, true);
        }
      }

      if (moveCloserToHero)
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
      if (ent is IAlly)
        base.AddEntity(ent as LivingEntity);
      else
        throw new Exception("AddEntity ent is !IAlly");
    }

    public void AddEntity(IAlly ent)
    {
      AddEntity(ent as LivingEntity);
    }

  }
}
