using Dungeons;
using Dungeons.Core;
using System.Collections.Generic;
using System.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System.Drawing;
using System;

namespace Roguelike.Managers
{
  public class EntitiesManager
  {
    public Hero Hero { get => context.Hero;  }

    protected List<LivingEntity> entities = new List<LivingEntity>();
    LivingEntity skipInTurn;
    public GameNode Node { get => context.CurrentNode;  }
    public GameContext Context { get => context; set => context = value; }

    EventsManager eventsManager;
    GameContext context;
    Func<LivingEntity, LivingEntity, AttackPolicy> policyFactory;
    public Func<LivingEntity, LivingEntity, AttackPolicy> PolicyFactory { get => policyFactory; set => policyFactory = value; }

    public EntitiesManager(GameContext context, EventsManager eventsManager)
    {
      Context = context;
      Context.ContextSwitched += Context_ContextSwitched;
      this.eventsManager = eventsManager;

      PolicyFactory = (LivingEntity e1, LivingEntity e2) => { return new AttackPolicy(e1, e2); };
    }

    private void Context_ContextSwitched(object sender, EventArgs e)
    {
      //SetEntities(Context.CurrentNode.GetTiles<LivingEntity>().Where(i=> !(i is Hero)).ToList());
    }

    public virtual void MakeEntitiesMove(LivingEntity skipInTurn = null)
    {
      this.skipInTurn = skipInTurn;
      if(entities.Any())
        MakeRandomMove(entities.First());

      Context.HeroTurn = false;
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

    //public Tuple<bool, Point> MoveEntity(LivingEntity entity, int horizontal, int vertical)
    //{
    //  var newPos = GetNewPositionFromMove(entity.point, horizontal, vertical);
    //  if (!newPos.First)
    //    return newPos;

    //  if (!CanMoveEntity(entity, newPos.Second))
    //    return new Tuple<bool, Point>(false, Point.Invalid);

    //  MoveEntity(entity, newPos.Second);
    //  return new Tuple<bool, Point>(true, newPos.Second); 
    //}

    public bool MoveEntity(LivingEntity entity, Point newPos)
    {
      //Debug.Log("moving hero to " + newPoint);
      if (Node.SetTile(entity, newPos))
      {
        eventsManager.AppendAction(new LivingEntityAction(kind: LivingEntityAction.Kind.Moved) { /*TileData = entity.Data,*/ Info = entity + " moved", InvolvedEntity = entity });
        //entity.EmitSmoothMovement();
        return true;
      }
      return false;

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
