using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class EnemiesManager : EntitiesManager
  {
    AttackStrategy attackStrategy;
    AlliesManager alliesManager;

    public AlliesManager AlliesManager { get => alliesManager; set => alliesManager = value; }

    public EnemiesManager(GameContext context, EventsManager eventsManager, Container container, AlliesManager alliesManager) :
      base(TurnOwner.Enemies, context, eventsManager, container)
    {
      this.AlliesManager = alliesManager;
      this.context = context;
      attackStrategy = new AttackStrategy(context);
      attackStrategy.OnPolicyApplied = (Policy pol)=>{ OnPolicyApplied(pol); };

      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }

    public virtual List<Enemy> GetActiveEnemies()
    {
      return this.AllEntities.Where(i => i.Revealed && i.Alive).Cast<Enemy>().ToList();
    }

    public virtual List<Enemy> GetEnemies()
    {
      return this.AllEntities.Cast<Enemy>().ToList();
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      var entities = Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> i is Enemy && !i.HeroAlly).ToList();
      base.SetEntities(entities);
    }

    private void OnTurnOwnerChanged(object sender, TurnOwner turnOwner)
    {
      if (turnOwner == TurnOwner.Enemies)
      { 
      }
    }

    public void MakeMove(LivingEntity enemy)
    {
    }
        
    public override void MakeTurn(LivingEntity entity)
    {
      var target = Hero;

      if (AttackAlly(entity))
        return;

      if (entity.DistanceFrom(Hero) > 10)
        return;

      if (!target.IsTransformed())
      {
        if (AttackIfPossible(entity, target))
          return;
      }

      bool makeRandMove = false;
      if (ShallChaseTarget(entity, target))
      {
        makeRandMove = !MakeMoveOnPath(entity, target.Point, false);
      }
      else
        makeRandMove = true;
      if (makeRandMove)
      {
        MakeRandomMove(entity);
      }
    }

    public bool AttackIfPossible(LivingEntity entity, LivingEntity target)
    {
      return attackStrategy.AttackIfPossible(entity, target);
    }

    public override void MakeRandomMove(LivingEntity entity)
    {
      if (entity.InitialPoint != LivingEntity.DefaultInitialPoint)
      {
        if (entity.MoveKind != EntityMoveKind.ReturningHome)
        {
          var distFromInitPoint = entity.DistanceFrom(entity.InitialPoint);
          if (distFromInitPoint > 5)
          {
            entity.MoveKind = EntityMoveKind.ReturningHome;
          }
        }

        if (entity.MoveKind == EntityMoveKind.ReturningHome)
        {
          if (MakeMoveOnPath(entity, entity.InitialPoint, false))
          {
            if(entity.Point == entity.InitialPoint)
              entity.MoveKind = EntityMoveKind.Freestyle;

            return;
          }
        }
      }
      base.MakeRandomMove(entity);
    }
            
    protected override bool MoveEntity(LivingEntity entity, Point newPos, List<Point> fullPath)
    {
      if (entity.InitialPoint == LivingEntity.DefaultInitialPoint)
      {
        entity.InitialPoint = entity.Point;
      }

      var moved = base.MoveEntity(entity, newPos, fullPath);
      return moved;
    }

    protected override void OnPolicyAppliedAllIdle()
    {
      if (context.TurnOwner == TurnOwner.Enemies)//for ASCII/UT
      {
        context.IncreaseActions(TurnOwner.Enemies);
        //Context.Logger.LogInfo("Enemies OnPolicyAppliedAllIdle");
        base.OnPolicyAppliedAllIdle();
      }
    }

    bool AttackAlly(LivingEntity enemy)
    {
      var ally = AlliesManager.AllEntities.Where(i=>i.DistanceFrom(enemy) < 2).FirstOrDefault();
      if (ally != null)
      {
        if (RandHelper.Random.NextDouble() < 0.3f)
          return AttackIfPossible(enemy, ally);
      }
      return false;
    }

  }
}
