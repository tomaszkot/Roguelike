using Dungeons.Core;
using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Abstract.Tiles;
using Roguelike.Discussions;
using Roguelike.LootContainers;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
#pragma warning disable 8632
namespace Roguelike.Tiles.LivingEntities
{
  public interface INPC : IAdvancedEntity
  {
    string Name { get; }
    TrainedHound TrainedHound { get; set; }
    LivingEntity LivingEntity { get; }
    Discussion Discussion { get; set; }
    void SetHasUrgentTopic(bool ut);
    bool HasUrgentTopic { get; set; }
    event EventHandler<bool> UrgentTopicChanged;

    DestPointDesc DestPointDesc { get; set; }

    public WalkKind WalkKind { get; }

    public string FollowedTargetName { get; set; }

    public RelationToHero RelationToHero { get; set; }
    public IGodStatue GodStatue { get; set; }
    int PointedByHeroCounter { get; set; }
  }

  public enum DestPointState {
    Unset, TravelingTo, StayingAtTarget, TravelingBack
  }

  public enum WalkKind { Unset, GoToHome, GoToInteractive, FollowingTarget  }

  public enum DestPointActivityKind { Unset, Home, Privy, Grill }

  public class DestPointDesc
  {
    public DestPointDesc()
    {
    }

    public bool IsWalkToTargetInProcess => State != DestPointState.Unset;
    
    private DestPointState state;

    public Dungeons.Tiles.Tile? MoveOnPathTarget { get; set; }

    public Point TargetPoint { get; set; }//can not enter interactive, so its next to interactive one
    public Point ReturnPoint { get; set; }
    public DestPointState State
    {
      get => state;
      set
      {
        state = value;
        StateCounter = 0;
      }
    }

    public int StayingDuration { get; set; } = 4;
    public int StateCounter { get; private set; } = 0;
    internal bool IncreaseStateCounter()
    {
      if (StateCounter < StayingDuration - 1 || State != DestPointState.StayingAtTarget)
      {
        StateCounter++;
        return true;
      }
      return false;
    }

    internal InteractiveTile? UnbusyTarget()
    {
      if (MoveOnPathTarget is InteractiveTile it)
      {
        it.SetBusy(null);
        ActivityKind = DestPointActivityKind.Unset;
        if(MoveOnPathTarget == it)
          MoveOnPathTarget = null;
        return it;
      }

      return null;
    }

    public DestPointActivityKind ActivityKind { get;
      set; }
    //public Point OriginalTargetPoint { get; internal set; }
  }

  /// <summary>
  /// 
  /// </summary>
  public class NPC : 
    LivingEntity, 
    INPC, 
    IApproachableByHero, 
    IAlly
  {
    public TrainedHound TrainedHound { get; set; }
    public bool TakeLevelFromCaster => false;
    public int PointedByHeroCounter { get; set; }

    public NPC(Container cont) : base(new Point().Invalid(), '!', cont)
    {
      
    }

    public LivingEntity LivingEntity => this;
        
    public bool ApproachedByHero { get; set; }
    public string ActivationSound { get; set; }
    
    public WalkKind WalkKind {
      get {
        if (DestPointDesc.State == DestPointState.TravelingBack)
          return WalkKind.GoToHome;
        else if (DestPointDesc.State == DestPointState.TravelingTo)
          return WalkKind.GoToInteractive;
        else if (!string.IsNullOrEmpty(FollowedTargetName))
          return WalkKind.FollowingTarget;

        return WalkKind.Unset;
      } 
    }
    public string FollowedTargetName { get ; set ; }

    public IGodStatue GodStatue { get ; set ; }
    public AllyBehaviour AllyBehaviour { get ; set; }
    public bool Active { get; set; }

    public AllyKind Kind { get; }//TODO remove and ret from method?

    public Point Point
    {
      get => point; set => point = value;
    }

    public bool PendingReturnToCamp { get ; set; }
       

    public Discussion Discussion { get ; set ; }
    public bool HasUrgentTopic { get ; set ; }
    public DestPointDesc DestPointDesc { get; set ; } = new DestPointDesc();
    public RelationToHero RelationToHero { get; set; } = new RelationToHero();
    public int AbilityPoints { get ; set ; }

    public AbilitiesSet Abilities { get; set; } = AbilitiesSet.CreateEmpty();

    public SpellStateSet Spells { get; set; } = SpellStateSet.CreateEmpty();

    public bool IsMecenary => LivingEntity.IsMercenary;

    public virtual Inventory Inventory { get; set; }

    public int Gold { get; set ; }
    public double NextLevelExperience { get; private set; }

    public event EventHandler Activated;
    public event EventHandler<bool> UrgentTopicChanged;
#pragma warning disable 67
    public event EventHandler StatsRecalculated;
    public event EventHandler LeveledUp;
#pragma warning restore 67

    public bool Activate()
    {
      if (!ApproachedByHero)
      {
        ApproachedByHero = true;
        if (Activated != null)
          Activated(this, EventArgs.Empty);
        return true;
      }
      return false;
    }

    public string GetPlaceName()
    {
      return "";
    }

    public void SetNextLevelExp(double exp)
    {
      NextLevelExperience += exp;
    }

    public void SetHasUrgentTopic(bool ut)
    {
      this.HasUrgentTopic = ut;
      if (UrgentTopicChanged != null)
        UrgentTopicChanged(this, HasUrgentTopic);
    }

    Dictionary<CurrentEquipmentKind, IEquipment> activeEquipment = new Dictionary<CurrentEquipmentKind, IEquipment>();
    public Dictionary<CurrentEquipmentKind, IEquipment> GetActiveEquipment()
    {
      return activeEquipment;
    }

    public bool IncreaseAbility(AbilityKind kind)
    {
      return true;
    }

    public bool IncreaseSpell(SpellKind sk)
    {
      return true;
    }

    public string GetExpInfo()
    {
      return "";
    }

    public bool InventoryAcceptsItem(Inventory inventory, Loot loot, AddItemArg addItemArg)
    {
      return true;
    }

    public bool MoveEquipmentCurrent2Inv(IEquipment eq, CurrentEquipmentKind cek)
    {
      return false;
    }

    public int GetPrice(Loot loot)
    {
      return loot.Price*4;
    }

    public bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return this != dest;
    }

    public double Experience { get; set; }
    //bool IAdvancedEntity.IsMercenary { get => LivingEntity.IsMercenary;}

    public bool IncreaseExp(double factor)
    {
      Experience += factor;
      return true;
    }

    public bool IsSellable(Loot loot)
    {
      return loot.IsSellable();
    }
  }
}
