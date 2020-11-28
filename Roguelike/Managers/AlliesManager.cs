﻿using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Linq;

namespace Roguelike.Managers
{
  public class AlliesManager : EntitiesManager
  {
    EnemiesManager enemiesManager;
    AttackStrategy attackStrategy;
    public event EventHandler<LivingEntity> AllyAdded;
    //public event EventHandler<LivingEntity> AllyRemoved;

    public AlliesManager(GameContext context, EventsManager eventsManager, Container container, EnemiesManager enemiesManager) :
                         base(TurnOwner.Allies, context, eventsManager, container)
    {
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
      this.enemiesManager = enemiesManager;
      attackStrategy = new AttackStrategy(context);
      attackStrategy.OnPolicyApplied = (Policy pol) => { OnPolicyApplied(pol); };
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //var allies = Context.CurrentNode.GetTiles<LivingEntity>().Where(i => i is Abstract.IAlly).Cast<IAlly>();
      //var entities = allies.Where(i => i.Active).Cast<LivingEntity>().ToList();
      var entities = Context.CurrentNode.GetTiles<LivingEntity>().Where(i => i.HeroAlly).ToList();
      base.SetEntities(entities);
    }

    //protected override bool MoveEntity(LivingEntity entity, Point newPos)
    //{
    //  //return base.MoveEntity(entity, newPos);
    //  MakeHeroAllyMove(entity);
    //  return true;
    //}
    public override void MakeTurn(LivingEntity entity)
    {
      MakeHeroAllyMove(entity);
    }

    const int MaxAllyDistToEnemyToChase = 6;

    public void MakeHeroAllyMove(LivingEntity ally)
    {
      if (!ally.Alive)
        return;
      if (ally.FixedWalkTarget != null)
      {
        if (ally.PathToTarget != null && ally.PathToTarget.Count == 1)
        {
          ally.FixedWalkTarget = null;
          //allyToRemove = ally;//TODO
        }
        if (!MakeMoveOnPath(ally, ally.FixedWalkTarget.Point, true))
          MakeRandomMove(ally);
        return;
      }

      if (ally.AllyModeTarget != null)
      {
        var found = enemiesManager.AllEntities.Where(i => i == ally.AllyModeTarget).FirstOrDefault();
        if (found == null)//dead?
          ally.AllyModeTarget = null;
      }

      if (ally.AllyModeTarget == null || ally.AllyModeTarget.DistanceFrom(ally) >= MaxAllyDistToEnemyToChase)
      {
        ally.AllyModeTarget = enemiesManager.AllEntities.Where(i => i.DistanceFrom(ally) < MaxAllyDistToEnemyToChase).OrderBy(i => i.DistanceFrom(ally)).ToList().FirstOrDefault();
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
          moveCloserToHero = !MakeMoveOnPath(ally, ally.AllyModeTarget.Point, true);//, true);
        }
      }

      if (moveCloserToHero)
      {
        if (ally.DistanceFrom(context.Hero) > 2)//do not block hero moves
        {
          //user false here as forHeroAlly as we want to find a hero on path
          MakeMoveOnPath(ally, context.Hero.Point, false);
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
      base.AddEntity(ent);
      if (AllyAdded != null)
        AllyAdded(this, ent);
    }
  }
}
