using Dungeons;
using Dungeons.Core;
using System.Collections.Generic;
using System.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using System;
using Roguelike.Events;

namespace Roguelike.Managers
{
  public class MovePolicy
  {
    LivingEntity entity;
    public event EventHandler OnApplied;
    AbstractGameLevel level;
    Point newPos;

    public Point NewPos { get => newPos; set => newPos = value; }
    public AbstractGameLevel Level { get => level; set => level = value; }
    public LivingEntity Entity { get => entity; set => entity = value; }

    public MovePolicy()
    {
      
    }

    public bool Apply(AbstractGameLevel level, LivingEntity entity, Point newPos)
    {
      this.Entity = entity;
      this.NewPos = newPos;
      this.Level = level;
      entity.State = EntityState.Moving;
      if (level.SetTile(entity, newPos))
      {
        ReportApplied();
        return true;
      }
      else
        entity.State = EntityState.Idle;

      return false;
    }

    protected virtual void ReportApplied()
    {
      Entity.State = EntityState.Idle;
      if (OnApplied != null)
        OnApplied(this, EventArgs.Empty);
    }
  }

  public class EntitiesManager
  {
    public Hero Hero { get => context.Hero; }

    protected List<LivingEntity> entities = new List<LivingEntity>();
    LivingEntity skipInTurn;
    public AbstractGameLevel Node { get => context.CurrentNode; }
    public GameContext Context { get => context; set => context = value; }

    protected EventsManager eventsManager;
    GameContext context;
    public Func<LivingEntity, LivingEntity, AttackPolicy> AttackPolicy { get; set; }

    public EntitiesManager(GameContext context, EventsManager eventsManager)
    {
      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;

      AttackPolicy = (LivingEntity e1, LivingEntity e2) => { return new AttackPolicy(e1, e2); };
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

    public void MakeRandomMove(LivingEntity entity)
    {
      var pt = Node.GetEmptyNeighborhoodPoint(entity);
      if (pt.IsValid())
      {
        MoveEntity(entity, pt);
        //logger.WriteLine(entity + " moved to "+ pt);
      }
    }

    public void SetEntities(List<LivingEntity> list)
    {
      entities = list;
    }

    public void AddEntity(LivingEntity ent)
    {
      entities.Add(ent);
    }

    protected virtual void OnPolicyApplied()
    {
      var notIdle = entities.FirstOrDefault(i => i.State != EntityState.Idle);
      if (notIdle == null)
        OnPolicyAppliedAllIdle();
    }

    protected virtual void OnPolicyAppliedAllIdle()
    {
    }

    public virtual bool MoveEntity(LivingEntity entity, Point newPos)
    {
      //Debug.Log("moving hero to " + newPoint);
      var mp = context.Container.GetInstance<MovePolicy>();
      mp.OnApplied += (s, e) =>
      {
        if (mp.Entity is Hero)
        {
          context.HeroTurn = false;
        }
        else
          OnPolicyApplied();
      };

      if(mp.Apply(Context.CurrentNode, entity, newPos))
      {
        eventsManager.AppendAction(new LivingEntityAction(kind: LivingEntityActionKind.Moved)
        {
          Info = entity + " moved",
          InvolvedEntity = entity,
          MovePolicy = mp
        }
);
      }

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
      foreach (var ent in entities)
      {
        MakeRandomMove(ent);
      }
    }
  }
}
