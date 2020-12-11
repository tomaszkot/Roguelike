using Dungeons.Core;
using System.Collections.Generic;
using System.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using System;
using Roguelike.Policies;
using SimpleInjector;
using System.Diagnostics;
using static Dungeons.TileContainers.DungeonNode;
using Roguelike.Strategy;

namespace Roguelike.Managers
{
  public class EntitiesManager
  {
    public Hero Hero { get => context.Hero; }

    private List<LivingEntity> entities = new List<LivingEntity>();

    public AbstractGameLevel Node { get => context.CurrentNode; }
    public GameContext Context { get => context; set => context = value; }
    public List<LivingEntity> AllEntities { get => entities; set => entities = value; }

    protected EventsManager eventsManager;
    protected GameContext context;
    protected Container container;
    TurnOwner turnOwner;
    bool pendingForAllIdle = false;

    public EntitiesManager(TurnOwner turnOwner, GameContext context, EventsManager eventsManager, Container container)
    {
      this.turnOwner = turnOwner;
      this.container = container;
      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;
    }

    public virtual List<LivingEntity> GetActiveEntities()
    {
      return this.entities.Where(i => i.Revealed && i.Alive).ToList();
    }

    private bool ReportAllDone(bool justEndedLoop)
    {
      if (Context.TurnOwner == turnOwner)
      {
        var busyOnes = entities.Where(i => i.State != EntityState.Idle).ToList();
        if (!busyOnes.Any())
        {
          OnPolicyAppliedAllIdle();
          return true;
        }
        else if(justEndedLoop)
          pendingForAllIdle = true;
      }

      return false;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //SetEntities(Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> !(i is Hero)).ToList());
    }

    public virtual void MakeTurn()
    {
      //this.skipInTurn = skipInTurn;
      if (pendingForAllIdle)
      {
        //context.Logger.LogInfo(this + " MakeTurn pendingForAllIdle!, return " );
        return;
      }

      RemoveDead();

      var activeEntities = GetActiveEntities();
      //context.Logger.LogInfo(this+" MakeTurn start, count: " + activeEntities.Count);

      pendingForAllIdle = false;
      
      if (!activeEntities.Any())
      {
        OnPolicyAppliedAllIdle();
        //context.Logger.LogInfo("no one to move...");
        return;
      }
      
      foreach (var entity in activeEntities)
      {
        //context.Logger.LogInfo("turn of: " + entity);
        try
        {
          //Debug.Assert(context.CurrentNode.GetTiles<LivingEntity>().Any(i => i == entity));//TODO

          entity.ApplyLastingEffects();
          if (!entity.Alive)
            continue;

          MakeTurn(entity);

          if (!context.Hero.Alive)
          {
            context.ReportHeroDeath();
            break;
          }
        }
        catch (Exception ex)
        {
          context.Logger.LogError("ex: "+ ex);
        }
      }

      RemoveDead();

      //context.Logger.LogInfo("EnemiesManager  MakeTurn ends");
      Debug.Assert(Context.TurnOwner == turnOwner);
      
      ReportAllDone(true);
    }

    public virtual void MakeTurn(LivingEntity entity)
    {
      MakeRandomMove(entity);
    }

    public virtual void MakeRandomMove(LivingEntity entity)
    {
      var pt = Node.GetEmptyNeighborhoodPoint(entity, EmptyNeighborhoodCallContext.Move, null, Node.GetExtraTypesConsideredEmpty());
      if (pt.Item1.IsValid())
      {
        MoveEntity(entity, pt.Item1, null);
        //logger.WriteLine(entity + " moved to "+ pt);
      }
    }

    bool entitiesSet = false;
    public void SetEntities(List<LivingEntity> list)
    {
      entities = list;
      entitiesSet = true;
    }

    public virtual void AddEntity(LivingEntity ent)
    {
      entities.Add(ent);
      entitiesSet = true;
    }

    public bool Contains(LivingEntity ent)
    {
      return entities.Contains(ent);
    }

    protected virtual void OnPolicyApplied(Policy policy)
    {
      if (!entitiesSet)
        context.Logger.LogError("!entitiesSet");

      if (context.TurnOwner != turnOwner)
      {
        context.Logger.LogError("OnPolicyApplied " + policy + " context.TurnOwner != turnOwner, context.TurnOwner: "+ context.TurnOwner);
        return;
      }
      //  return;//in ascii/UT mode this can happend
      if (pendingForAllIdle)
      {
        //context.Logger.LogInfo("calling  ReportAllDone... Context.TurnOwner: " + Context.TurnOwner);
        ReportAllDone(false);
      }
    }

    protected virtual void OnPolicyAppliedAllIdle()
    {
      //context.Logger.LogInfo(this+ " OnPolicyAppliedAllIdle Context.TurnOwner "+ Context.TurnOwner);
      if (Context.TurnOwner == turnOwner)//this check is mainly for ASCII/UT
      {
        //Context.Logger.LogInfo(this+ " OnPolicyAppliedAllIdle calling MoveToNextTurnOwner");
        pendingForAllIdle = false;
        Context.MoveToNextTurnOwner();
      }
    }

    protected virtual bool MoveEntity(LivingEntity entity, Point newPos, List<Point> fullPath)
    {
      context.ApplyMovePolicy(entity, newPos, fullPath, (e) => OnPolicyApplied(e));

      return true;//TODO
    }

    public bool CanMoveEntity(LivingEntity entity, Point pt)
    {
      if (!Node.IsPointInBoundaries(pt))
        return false;

      var collider = Node.GetTile(pt);
      if (collider != null && !collider.IsEmpty && !(collider is Loot))
        return false;

      return true;
    }

    public void RemoveDead()
    {
      var deadOnes = entities.Where(i => !i.Alive).ToList();
      //context.Logger.LogInfo("deadOnes : "+ deadOnes.Count);
      foreach (var dead in deadOnes)
      {
        Context.EventsManager.AppendAction(dead.GetDeadAction());
        entities.Remove(dead);
      }
    }

    protected bool MakeMoveOnPath(LivingEntity entity, Point target, bool forHeroAlly)
    {
      bool moved = false;
      entity.PathToTarget = Node.FindPath(entity.Point, target, forHeroAlly, true);
      if (entity.PathToTarget != null && entity.PathToTarget.Count > 1)
      {
        var fullPath = new List<Point>();
        var node = entity.PathToTarget[1];
        var pt = new Point(node.Y, node.X);
        fullPath.Add(pt);

        if (entity.PathToTarget.Count > 2)
        {
          var canMoveFaster = false;
          var enemy = entity as Enemy;
          if (enemy != null)
          {
            var sk = context.CurrentNode.GetSurfaceKindUnderTile(enemy);
            if (enemy.GetSurfaceSkillLevel(sk) > 0)
              canMoveFaster = true;

          }
          if (canMoveFaster)
          {
            node = entity.PathToTarget[2];
            pt = new Point(node.Y, node.X);
            if(target != pt)
              fullPath.Add(pt);
          }
        }
                
        if (Node.GetTile(pt) is LivingEntity)
        {
          //gm.Assert(false, "Level.GetTile(pt) is LivingEntity "+ enemy + " "+pt);
          return false;
        }

        MoveEntity(entity, pt, fullPath);
        moved = true;
      }

      return moved;
    }

    public bool ShallChaseTarget(LivingEntity enemy, LivingEntity target)
    {
      if (target.IsTransformed())
      {
        return false;
      }

      if (enemy.WasEverHitBy(target))
      {
        enemy.MoveKind = EntityMoveKind.FollowingHero;
        return true;
      }

      var dist = enemy.DistanceFrom(target);
      if (dist < 5)
        return true;

      return false;
    }
  }

  
}
