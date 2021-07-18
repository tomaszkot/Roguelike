using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Managers
{
  public class EntitiesManager
  {
    protected AttackStrategy attackStrategy;
    public Hero Hero { get => context.Hero; }

    private List<LivingEntity> entities = new List<LivingEntity>();

    public AbstractGameLevel Node { get => context.CurrentNode; }
    public GameContext Context { get => context; set => context = value; }
    public List<LivingEntity> AllEntities { get => entities; set => entities = value; }
    public IEnumerable<IAlly> AllAllies { get => entities.Cast<IAlly>(); }
    protected GameManager gameManager;

    protected EventsManager eventsManager;
    protected GameContext context;
    protected Container container;
    TurnOwner turnOwner;
    bool pendingForAllIdle = false;

    public EntitiesManager(TurnOwner turnOwner, GameContext context, EventsManager eventsManager, Container container,
                          GameManager gameManager)
    {
      this.turnOwner = turnOwner;
      this.container = container;
      this.gameManager = gameManager;

      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;

      attackStrategy = new AttackStrategy(context, gameManager);
      attackStrategy.OnPolicyApplied = (Policy pol) => { OnPolicyApplied(pol); };
    }

    public virtual List<LivingEntity> GetActiveEntities()
    {
      return this.entities.Where(i => i.Revealed && i.Alive).ToList();
    }

    private bool ReportAllDone(bool justEndedLoop)
    {
      if (Context.TurnOwner == turnOwner)
      {
        var busyOnes = entities.Where(i => i.State != EntityState.Idle && i.State != EntityState.Sleeping).ToList();
        if (!busyOnes.Any())
        {
          OnPolicyAppliedAllIdle();
          return true;
        }
        else if (justEndedLoop)
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
          if (entity is AdvancedLivingEntity ade)
            ade.ApplyAbilities();
          entity.ApplyLastingEffects();
          if (!entity.Alive)
            continue;

          if (entity.IsSleeping)
          {
            continue;
          }

          if (entity.LastingEffects.Where(i => i.Type == Effects.EffectType.Stunned).Any())
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
          context.Logger.LogError("ex: " + ex);
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
      var pt = Node.GetEmptyNeighborhoodPoint(entity, null, Node.GetExtraTypesConsideredEmpty());
      if (pt != null && pt.Item1.IsValid())
      {
        MoveEntity(entity, pt.Item1, null);
        //logger.WriteLine(entity + " moved to "+ pt);
      }
    }

    protected bool entitiesSet = false;

    public void SetEntities(List<IAlly> list)
    {
      SetEntities(list.Cast<LivingEntity>().ToList());
    }

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
        context.Logger.LogError("OnPolicyApplied " + policy + " context.TurnOwner != turnOwner, context.TurnOwner: " + context.TurnOwner);
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
      return gameManager.ApplyMovePolicy(entity, newPos, fullPath, (e) => OnPolicyApplied(e));
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
        RemoveDeadEntity(dead);
        gameManager.HandleDeath(dead);
      }
    }

    public bool RemoveAlly(IAlly ent)
    {
      return RemoveEntity(ent as LivingEntity);
    }

    public virtual bool RemoveDeadEntity(LivingEntity ent)
    {
      return RemoveEntity(ent);
    }

    public bool RemoveEntity(LivingEntity ent)
    {
      return entities.Remove(ent);
    }

    protected bool MakeMoveOnPath(LivingEntity entity, Point target, bool forHeroAlly)
    {
      bool moved = false;
      entity.PathToTarget = Node.FindPath(entity.point, target, forHeroAlly, true, false);
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
            var nextPoint = new Point(node.Y, node.X);
            if (target != nextPoint)
            {
              fullPath.Add(nextPoint);
              pt = nextPoint;
            }
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

    protected const int MaxEntityDistanceToToChase = 6;
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
      if (dist < MaxEntityDistanceToToChase)
        return true;

      return false;
    }
  }


}
