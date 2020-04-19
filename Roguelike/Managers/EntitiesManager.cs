using Dungeons;
using Dungeons.Core;
using System.Collections.Generic;
using System.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using System;
using Roguelike.Events;
using Roguelike.Policies;
using SimpleInjector;

namespace Roguelike.Managers
{
  

  public class EntitiesManager
  {
    public Hero Hero { get => context.Hero; }

    protected List<LivingEntity> entities = new List<LivingEntity>();
    LivingEntity skipInTurn;
    public AbstractGameLevel Node { get => context.CurrentNode; }
    public GameContext Context { get => context; set => context = value; }

    protected EventsManager eventsManager;
    protected GameContext context;
    protected Container container;

    public EntitiesManager(GameContext context, EventsManager eventsManager, Container container)
    {
      this.container = container;
      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //SetEntities(Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> !(i is Hero)).ToList());
    }

    public virtual void MakeEntitiesMove(LivingEntity skipInTurn = null)
    {
      this.skipInTurn = skipInTurn;
      if (entities.Any())
        MakeRandomMove(entities.First());
    }

    public virtual void MakeRandomMove(LivingEntity entity)
    {
      var pt = Node.GetEmptyNeighborhoodPoint(entity);
      if (pt.Item1.IsValid())
      {
        MoveEntity(entity, pt.Item1);
        //logger.WriteLine(entity + " moved to "+ pt);
      }
    }

    bool entitiesSet = false;
    public void SetEntities(List<LivingEntity> list)
    {
      entities = list;
      entitiesSet = true;
    }

    public void AddEntity(LivingEntity ent)
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
      //if (context.TurnOwner != TurnOwner.Enemies)
      //  return;//in ascii/UT mode this can happend

      if (!entitiesSet)
        context.Logger.LogError("!entitiesSet");
      var notIdle = entities.FirstOrDefault(i => i.State != EntityState.Idle);
      if (notIdle == null)
      {
        OnPolicyAppliedAllIdle();
      }
    }

    protected virtual void OnPolicyAppliedAllIdle()
    {
      //if (Context.TurnOwner == TurnOwner.Enemies)//this check is mainly for ASCII/UT
      {
        //Context.Logger.LogInfo(" OnPolicyAppliedAllIdle");
        Context.MoveToNextTurnOwner();
      }
    }

    protected virtual bool MoveEntity(LivingEntity entity, Point newPos)
    {
      context.ApplyMovePolicy(entity, newPos, (e) => OnPolicyApplied(e));

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

    internal void MoveHeroAllies()
    {
      if (!entities.Any())
      {
        OnPolicyAppliedAllIdle();
        return;
      }
      foreach (var ent in entities)
      {
        MakeRandomMove(ent);
      }
    }
  }

  public class AlliesManager : EntitiesManager
  {
    public AlliesManager(GameContext context, EventsManager eventsManager, Container container) :
      base(context, eventsManager, container)
    {
      context.TurnOwnerChanged += OnTurnOwnerChanged;
      context.ContextSwitched += Context_ContextSwitched;
    }

    private void Context_ContextSwitched(object sender, ContextSwitch e)
    {
      //base.SetEntities(enemies);
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
      //if (turnOwner == TurnOwner.Allies)
      //  MoveHeroAllies();
    }
  }
}
