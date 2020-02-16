using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
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
    //TileData data;
    //public event EventHandler SmoothMovement;
    //public event EventHandler<GenericEventArgs<LivingEntity>> Died;
    public Point PrevPoint;
    EntityStats stats = new EntityStats();
    public EntityState State { get; set; }

    //[JsonIgnoreAttribute]
    public EntityStats Stats { get => stats; set => stats = value; }

    public LivingEntity(Point point, char symbol) : base(point, symbol)
    {
      //this.EventsManager = eventsManager;
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
    List<Algorithms.PathFinderNode> pathToTarget;
    //internal void EmitSmoothMovement()
    //{
    //  if (SmoothMovement != null)
    //    SmoothMovement(this, EventArgs.Empty);
    //}

    bool alive = true;
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
            AppendAction(new LivingEntityAction(LivingEntityAction.Kind.Died) { InvolvedEntity = this, Level = ActionLevel.Important, Info = Name +" Died" });
          }
        }
      }
    }

   // public TileData Data { get => data; set => data = value; }
    public EventsManager EventsManager { get; set; }

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
      var inflicted = (attacker.GetHitAttackValue(true) / defence);
      ReduceHealth(inflicted);
      var ga = new LivingEntityAction(LivingEntityAction.Kind.GainedDamage) { InvolvedValue = inflicted, InvolvedEntity = this };
      var desc = "received damage: " + inflicted.Formatted();
      ga.Info = Name.ToString() + " " + desc;
#if UNITY_EDITOR
      ga.Info += "UE , Health = " + Stats.Health.Formatted();
#endif
      AppendAction(ga);
      DieIfShould();
      return inflicted;
    }

    protected void AppendAction(GameAction ac)
    {
      if(EventsManager != null)
        EventsManager.AppendAction(ac);
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
      var cv = stat.CurrentValue;
      if (stat.IsPercentage && cv > 100)
      {
        cv = 100;
      }
      return cv;
    }

    public virtual float GetHitAttackValue(bool withVariation)
    {
      var str = Stats.GetCurrentValue(EntityStatKind.Strength);
      //var as1 = Stats.Stats[EntityStatKind.Attack];
      var att = Stats.GetCurrentValue(EntityStatKind.Attack);

      return str + att;
    }

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
