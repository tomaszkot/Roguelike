using Dungeons.Core;
using Roguelike.Abstract.Tiles;
using Roguelike.Events;
using Roguelike.Policies;
using Roguelike.Strategy;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
//using static UnityEngine.EventSystems.EventTrigger;

namespace Roguelike.Managers
{
  public enum BattleOrder { Unset, Surround }

  public class EntitiesManager
  {
    
    protected AttackStrategy attackStrategy;
    public Hero Hero { get => context.Hero; }

    private List<LivingEntity> entities = new List<LivingEntity>();

    public AbstractGameLevel Node { get => context.CurrentNode; }
    public GameContext Context { get => context; set => context = value; }
    public List<LivingEntity> AllEntities
    {
      get => entities;
      private set => entities = value;
    }
    public IEnumerable<IAlly> AllAllies { get => entities.Cast<IAlly>(); }
    public enum State { Idle, Looping, WaitingForAllIdle };

    public State CurrentState { get; set; }

    public bool CheckBusyOnes { get; set; }

    protected GameManager gameManager;

    protected EventsManager eventsManager;
    protected GameContext context;
    protected Container container;
    TurnOwner turnOwner;
    

    public EntitiesManager(TurnOwner turnOwner, GameContext context, EventsManager eventsManager, Container container,
                          GameManager gameManager)
    {
      this.turnOwner = turnOwner;
      this.container = container;
      this.gameManager = gameManager;

      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;

      attackStrategy = new AttackStrategy(gameManager);
      attackStrategy.OnPolicyApplied = (Policy pol) => { OnPolicyApplied(pol); };
    }

    public virtual List<LivingEntity> GetActiveEntities()
    {
      var basicList = GetBasicActiveEntitiesList();

      var res = basicList.Where(i => i is God ||
      i.DistanceFrom(gameManager.Hero) < 15 ||
      (i is INPC npc && npc.WalkKind == WalkKind.GoToHome)
      )
      .ToList();

      //var res1 = basicList.Where(i => i.State != EntityState.Idle)
      //.ToList();
      //if (res1.Count > res.Count)
      //{
      //  context.Logger.LogError("ret res1!");
      //  res = res1;
      //}
      return res;
    }

    protected List<LivingEntity> GetBasicActiveEntitiesList()
    {
      return this.entities
              .Where(i => i.Revealed && i.Alive)
              .ToList();
    }

    protected virtual bool CheckState(BusyOnesCheckContext context)
    {
      if (Context.TurnOwner == turnOwner)
      {
        return DoCheckState(context);
      }

      return false;
    }

    protected bool DoCheckState(BusyOnesCheckContext context)
    {
      var busyOnes = FindBusyOnes(context);
      if (!busyOnes.Any() && (CurrentState == State.WaitingForAllIdle || CurrentState == State.Idle))
      {
        OnPolicyAppliedAllIdle();
        return true;
      }
      else if (context == BusyOnesCheckContext.EndOfTurn)
      {
        CurrentState = State.WaitingForAllIdle;
      }
      return false;
    }

    protected bool BrutalPendingForAllIdleFalseMode = false;
    public void ForcePendingForAllIdleFalse()
    {
      var busyOnes = FindBusyOnes(BusyOnesCheckContext.ForceIdle);
      busyOnes.ForEach(i => i.State = EntityState.Idle);
      CurrentState = State.Idle;
    }

    public enum BusyOnesCheckContext { Unset, StartOfTurn, EndOfTurn, PolicyApplied, ForceIdle }

    public virtual List<LivingEntity> FindBusyOnes(BusyOnesCheckContext context)
    {
      //HACK - state change!
      //var farOnes = entities.Where(i => i.DistanceFrom(gameManager.Hero) > 15 && i.State != EntityState.Sleeping).ToList();
      //farOnes.ForEach(i => i.State = EntityState.Idle);
      var ents = entities.Where(i => IsBusy(i)).ToList();
      return ents;
    }

    protected virtual bool IsBusy(LivingEntity i)
    {
      if (!i.Alive)
        return false;
      return i.State != EntityState.Idle &&
             i.State != EntityState.Sleeping
            //i.State != EntityState.DestPointTask &&
            //!ignoredForPendingForAllIdle.Contains(i)
            ;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //SetEntities(Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> !(i is Hero)).ToList());
    }

    void LogInfo(string log)
    {
      context.Logger.LogInfo(log);
    }

    void LogError(string log)
    {
      context.Logger.LogError(log);
    }

    void WriteDetailedLog(string log)
    {
      if (detailedLogs)
        LogInfo(this + " " + log);
    }
    protected bool detailedLogs = false;
    List<LivingEntity> _busyOnes = new List<LivingEntity>();
    protected bool EmitAllIdleWhenNooneBusy = false;
    protected const int ZyndramBackMovesCounter = 40;
    public static bool IsZyndrams(LivingEntity i)
    {
      return i.tag1.StartsWith("zyndram_");
    }

    public virtual void MakeTurn()
    {
      if (CurrentState == State.WaitingForAllIdle)
      {
        var busyOnes = FindBusyOnes(BusyOnesCheckContext.StartOfTurn);
        if (detailedLogs)
        {
          WriteDetailedLog(" MakeTurn waitingForAllIdle!, returning... busy:" + busyOnes.FirstOrDefault());
        }
        
        if (EmitAllIdleWhenNooneBusy && !busyOnes.Any())
        {
          OnPolicyAppliedAllIdle();
          return;
        }

        bool shallRet = true;
        if (CheckBusyOnes)
        {
          CheckBusyOnes = false;
          busyOnes = HackBusyOnes(busyOnes);
          if (!busyOnes.Any())
          {
            shallRet = false;
          }

        }
        
        if (shallRet)
          return;
        CurrentState = State.Idle;
      }
      CurrentState = State.Looping;

      RemoveDead();

      var activeEntities = GetActiveEntities();
      WriteDetailedLog("MakeTurn start, count: " + activeEntities.Count);

      if (!activeEntities.Any())
      {
        WriteDetailedLog("no one to move...");

        OnPolicyAppliedAllIdle();
        return;
      }

      foreach (var entity in activeEntities)
      {

        try
        {
          var startPoint = entity.Position;

          if(detailedLogs)
            WriteDetailedLog("turn of: " + entity + " started");

          if (entity.State != EntityState.Idle && entity.State != EntityState.Sleeping)
          {
            if(!IsZyndrams(entity))
              LogError("entity.State != EntityState.Idle "+ entity + " " + entity.tag1);
            continue;
          }
          if (entity is AdvancedLivingEntity ade)
            ade.ApplyAbilities();
          entity.ApplyLastingEffects();
          entity.ReduceHealthDueToSurface(gameManager.CurrentNode);//TODO ReduceHealthDueToSurface shall be no matter if entity is active
          if (!entity.Alive)
          {
            WriteDetailedLog(" !entity.Alive "+ entity.Name);
            continue;
          }

          if (entity.IsSleeping)
          {
            continue;
          }

          string reason = "";
          if (entity.IsMoveBlockedDueToLastingEffect(out reason))
          {
            WriteDetailedLog("!" + reason);
            continue;
          }
          entity.MovesCounter++;
          MakeTurn(entity);

          if (!context.Hero.Alive)
          {
            context.ReportHeroDeath();
            break;
          }
          var endPoint = entity.Position;
          //if (detailedLogs && entity is Enemy && endPoint == startPoint)
          //{
          //  context.Logger.LogError("EnemiesManager  endPoint == startPoint !!! entity: " + entity);
          //}
          if (detailedLogs)
            WriteDetailedLog("turn of: " + entity + " ended");
        }
        catch (Exception ex)
        {
          context.Logger.LogError("ex: " + ex);
          entity.State = EntityState.Idle;
        }
      }

      MakeSpecialActions();

      RemoveDead();

      WriteDetailedLog("MakeTurn ends");
      Dungeons.DebugHelper.Assert(Context.TurnOwner == turnOwner);
      CurrentState = State.WaitingForAllIdle;
      CheckState(BusyOnesCheckContext.EndOfTurn);
    }

    private List<LivingEntity> HackBusyOnes(List<LivingEntity> busyOnes)
    {
      var busyOnesAgain = _busyOnes.Where(i => busyOnes.Contains(i)).ToList();
      if (busyOnesAgain.Any())
      {
        var names = String.Join(";", busyOnesAgain.Select(i => i.Name));
        LogError("Forcing idle for entities: " + String.Join("; ", names + " "+ busyOnesAgain.First()));
        foreach (var bo in busyOnesAgain)
        {
          bo.State = EntityState.Idle;
        }
      }
      busyOnes = FindBusyOnes(BusyOnesCheckContext.ForceIdle);
      _busyOnes = busyOnes;
      return busyOnes;
    }

    protected virtual void MakeSpecialActions()
    {

    }

    public virtual void MakeTurn(LivingEntity entity)
    {
      if (entity.IsMercenary && entity.MovesCounter > ZyndramBackMovesCounter && RandHelper.GetRandomDouble() > 0.5f)
        return;
      MakeRandomMove(entity);
    }

    public virtual void MakeRandomMove(LivingEntity entity)
    {
      var emptyTypes = Node.GetExtraTypesConsideredEmpty();
      var pt = Node.GetEmptyNeighborhoodPoint(entity, null, emptyTypes);
      if (pt != null && pt.Item1.IsValid() && !Node.GetSurfaceKindsUnderPoint(pt.Item1).Contains(SurfaceKind.Lava))
      {
        //if (detailedLogs)
        // LogInfo("!MoveEntity " + pt.Item1);
        MoveEntity(entity, pt.Item1, null);
      }
      else if (detailedLogs)
      {
        string types = "";
        foreach (var ty in emptyTypes)
          types += ty + ", ";

        string neibs = "";
        foreach (var ty in Node.GetNeighborTiles(entity))
        {
          neibs += ty;
        }

        WriteDetailedLog("MakeRandomMove not done pt: " + pt + " empty types: " + types + ", neibs: " + neibs);
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
      entities.ForEach(i => EnsureInitPoint(i));
    }

    public virtual void AddEntity(LivingEntity ent)
    {
      if (entities.Contains(ent))
      {
        LogError("entity already edded "+ent);
        return;
      }
      entities.Add(ent);
      entitiesSet = true;
      EnsureInitPoint(ent);
    }

    public bool Contains(LivingEntity ent)
    {
      return entities.Contains(ent);
    }

    protected virtual bool ShalLReportTurnOwner(Policy policy)
    {
      if (policy is MovePolicy)
        return false;//when hero had a hound in the camp this was logged

      return true;
    }

    /// <summary>
    /// Animation is done if all done end the turn
    /// </summary>
    /// <param name="policy"></param>
    public //TODO for god
      virtual void OnPolicyApplied(Policy policy)
    {
      if (!entitiesSet)
        context.Logger.LogError("!entitiesSet");

      if (context.TurnOwner != turnOwner)
      {
        if (ShalLReportTurnOwner(policy) && GameContext.ShalLReportTurnOwner(policy))
        {
          context.Logger.LogError("OnPolicyApplied " + policy + " context.TurnOwner != turnOwner, context.TurnOwner: " + context.TurnOwner 
            + " this: " + this + ", policy : " + policy + " CurrentState: " + CurrentState);
        }
        //return;//that return probably causes a critical bug that hero is not moving at all
      }
      //  return;//in ascii/UT mode this can happend
      if (CurrentState == State.WaitingForAllIdle)
      {
        if (detailedLogs)
          WriteDetailedLog("calling  ReportAllDone... Context.TurnOwner: " + Context.TurnOwner);
        CheckState(BusyOnesCheckContext.PolicyApplied);
      }
    }

    protected virtual void OnPolicyAppliedAllIdle()
    {
      if (detailedLogs)
        WriteDetailedLog(" OnPolicyAppliedAllIdle Context.TurnOwner " + Context.TurnOwner);
      if (Context.TurnOwner == turnOwner)//this check is mainly for ASCII/UT
      {
        if (detailedLogs)
          WriteDetailedLog(" OnPolicyAppliedAllIdle calling MoveToNextTurnOwner");
        CurrentState = State.Idle;
        Context.MoveToNextTurnOwner();
      }
    }

    protected virtual bool MoveEntity(LivingEntity entity, Point newPos, List<Point> fullPath = null)
    {
      EnsureInitPoint(entity);
      return gameManager.ApplyMovePolicy(entity, newPos, fullPath, (e) => OnPolicyApplied(e));
    }

    private static void EnsureInitPoint(LivingEntity entity)
    {
      if (entity.InitialPoint == LivingEntity.DefaultInitialPoint)
      {
        entity.InitialPoint = entity.point;
      }
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
        var atDead = gameManager.CurrentNode.GetTile(dead.point);
        if (atDead is ProjectileFightItem pfi && pfi.FightItemKind == FightItemKind.HunterTrap)
        {
          pfi.SetState(FightItemState.Deactivated);
          gameManager.AppendAction(new LootAction(pfi, null) { Kind = LootActionKind.Deactivated });
        }
      }
    }

    public bool RemoveAlly(IAlly ent)
    {
      var rem = RemoveEntity(ent as LivingEntity);
      return rem;
    }

    public virtual bool RemoveDeadEntity(LivingEntity ent)
    {
      return RemoveEntity(ent);
    }

    public bool RemoveEntity(LivingEntity ent)
    {
      var res = entities.Remove(ent);
      if (ent is IAlly ally)
      {
        ally.Active = false;
        Context.EventsManager.AppendAction(new AllyAction() { InvolvedTile = ally, AllyActionKind = AllyActionKind.Released }); ;
      }
      return res;
    }

    protected bool MakeMoveOnPath(LivingEntity entity, Point target, bool forHeroAlly, bool alwaysAddNextPoint = false)
    {
      bool moved = false;
      entity.PathToTarget = Node.FindPath(entity, target);
      if (entity.PathToTarget != null)
      {
        if (entity.PathToTarget.Count > 1)
        {
          var fullPath = new List<Point>();
          var node = entity.PathToTarget[1];
          var pt = new Point(node.Y, node.X);
          fullPath.Add(pt);

          if (entity.PathToTarget.Count > 2)
          {
            var canMoveFaster = entity.CalcShallMoveFaster(Node);

            if (canMoveFaster)
            {
              node = entity.PathToTarget[2];
              var nextPoint = new Point(node.Y, node.X);
              if (target != nextPoint || alwaysAddNextPoint)//what for ?
              {
                fullPath.Add(nextPoint);
                pt = nextPoint;
              }
            }
          }

          var atPoint = Node.GetTile(pt);
          if (atPoint is LivingEntity && atPoint != entity)
          {
            //gm.Assert(false, "Level.GetTile(pt) is LivingEntity "+ enemy + " "+pt);
            return false;
          }

          MoveEntity(entity, pt, fullPath);
          moved = true;
        }
      }
      else
      {
        int k = 0;
        k++;
      }
      entity.LastMoveOnPathResult = moved;
      return moved;
    }

    protected const int MaxEntityDistanceToToChase = 6;
    public bool ShallChaseTarget
    (
      LivingEntity chaser,
      LivingEntity target,
      int maxEntityDistanceToToChase = MaxEntityDistanceToToChase,
      LivingEntity maxDistanceFrom = null
    )
    {
      if (target.IsTransformed())
      {
        return false;
      }

      if (chaser.HasLastingEffect(Effects.EffectType.Frighten))
        return false;

      var isSmoke = this.gameManager.CurrentNode.IsAtSmoke(chaser);
      if (isSmoke)
        return false;

      var isAlly = chaser is IAlly;

      if (chaser.WasEverHitBy(target))
      {
        chaser.MoveKind = EntityMoveKind.FollowingHero;
        return true;
      }
      if (maxDistanceFrom == null)
        maxDistanceFrom = chaser;

      if (chaser.ChaseCounter > 4 && !isAlly)
      {
        chaser.ChaseCounter = 0;
        return false;
      }

      var dist = maxDistanceFrom.DistanceFrom(target);
      if (dist < maxEntityDistanceToToChase)
      {
        return true;
      }

      return false;
    }

    public List<Enemy> GetInRange(LivingEntity src, int rangeFrom, Enemy skip)
    {
      return AllEntities
        .Where(i => i != skip && i.DistanceFrom(src) < 7 && i.Revealed)
        .Cast<Enemy>()
        .ToList();
    }

    public void FollowTarget(LivingEntity follower, LivingEntity target)
    {
      if (target == null)
        return;

      if (follower.DistanceFrom(target) > 2)//do not block him
      {
        var moved = MakeMoveOnPath(follower, target.point, false);
        if (!moved)
        {
          var ce = gameManager.CurrentNode.GetClosestEmpty(target);
          if (ce != null)
            moved = MakeMoveOnPath(follower, ce.point, false);
        }
      }
    }

    protected void FollowTarget(INPC npc)
    {
      var target = this.AllEntities.Where(i => i.Name == npc.FollowedTargetName).SingleOrDefault();
      FollowTarget(npc.LivingEntity, target);
    }

  }


}
