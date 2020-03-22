using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.Managers;
using Roguelike.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public enum EntityState { Idle, Moving, Attacking }

  public class LivingEntity : Tile
  {
    //public event EventHandler<GenericEventArgs<LivingEntity>> Died;
    public Point PrevPoint;
    EntityStats stats = new EntityStats();
    public EntityState State { get; set; }
    List<Algorithms.PathFinderNode> pathToTarget;

    bool alive = true;
    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    public LivingEntity(Point point, char symbol) : base(point, symbol)
    {
      //this.EventsManager = eventsManager;
    }

    public static LivingEntity CreateDummy()
    {
      return new LivingEntity(new Point(0, 0), '\0');
    }
    
    [JsonIgnore]
    public List<Algorithms.PathFinderNode> PathToTarget
    {
      get
      {
        return pathToTarget;
      }

      set
      {
        pathToTarget = value;
      }
    }
    
    public virtual bool Alive
    {
      get { return alive; }
      set
      {
        if (alive != value)
        {
          alive = value;
          if (!alive)
          {
            AppendAction(new LivingEntityAction(LivingEntityActionKind.Died) { InvolvedEntity = this, Level = ActionLevel.Important, Info = Name +" Died" });
          }
        }
      }
    }

    EventsManager eventsManager;
    [JsonIgnore]
    public EventsManager EventsManager
    {
      get { return eventsManager; }
      set { eventsManager = value; }
    }

    internal bool CalculateIfHitWillHappen(LivingEntity target)
    {
      return true;
    }

    public float OnPhysicalHit(LivingEntity attacker)
    {
      float defence = GetDefence();
      if (defence == 0)
      {
        //gm.Assert(false, "Stats.Defence == 0");
        return 0;
      }
      var inflicted = attacker.GetCurrentValue(EntityStatKind.Attack) / defence;
      ReduceHealth(inflicted);
      var ga = new LivingEntityAction(LivingEntityActionKind.GainedPhisicalDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      var desc = "received damage: " + inflicted.Formatted();
      ga.Info = Name.ToString() + " " + desc;
#if UNITY_EDITOR
      ga.Info += "UE , Health = " + Stats.Health.Formatted();
#endif
      AppendAction(ga);
      //if (this is Enemy || this is Hero)// || this is CrackedStone)
      //{
      //  PlayPunchSound();
      //}
      DieIfShould();
      return inflicted;
    }

    public override string ToString()
    {
      var str = base.ToString();
      str += " "+this.State;
      return str;
    }

    protected void AppendAction(GameAction ac)
    {
      if(EventsManager != null)
        EventsManager.AppendAction(ac);
    }

    protected void Assert(bool check, string desc)
    {
      if (EventsManager != null)
        EventsManager.Assert(check, desc);
    }

    private bool DieIfShould()
    {
      if (Alive && HealthZero())
      {
        Alive = false;
        return true;
      }
      return false;
    }

    private bool HealthZero()
    {
      return Stats.GetCurrentValue(EntityStatKind.Health) <= 0;
    }

    public virtual void ReduceHealth(float amount)
    {
      Stats.Stats[EntityStatKind.Health].Subtract(amount);
    }

    private float GetDefence()
    {
      return GetCurrentValue(EntityStatKind.Defence);
    }

    internal void ApplyPhysicalDamage(LivingEntity victim)
    {
      
      victim.OnPhysicalHit(this);
    }

    public float GetCurrentValue(EntityStatKind kind)
    {
      var stat = Stats.Stats[kind];
      var cv = stat.Value.CurrentValue;
      if (stat.IsPercentage && cv > 100)
      {
        cv = 100;
      }
      return cv;
    }

    //public virtual float GetHitAttackValue()//bool withVariation)
    //{
    //  var str = Stats.GetCurrentValue(EntityStatKind.Strength);
    //  //var as1 = Stats.Stats[EntityStatKind.Attack];
    //  var att = Stats.GetCurrentValue(EntityStatKind.Attack);

    //  return str + att;
    //}

    public float GetTotalValue(EntityStatKind esk)
    {
      return Stats.GetTotalValue(esk);
    }

    //public static implicit operator Dungeons.Tiles.Tile(LivingEntity d)  // implicit digit to byte conversion operator
    //{
    //  return d.DungeonTile;  // implicit conversion
    //}
  }
}
