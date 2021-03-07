using Dungeons.Tiles;
using Roguelike.Abstract.Tiles;
using Roguelike.Effects;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Roguelike.Events
{
  public enum ActionLevel
  {
    Normal,
    Important,
  }

  //TODO rename to Event
  public class GameAction
  {
    public string Info { get; set; } = "";
    public ActionLevel Level { get; set; }
    public int Index { get; set; }
    public virtual string GetSound() { return ""; }

    public GameAction() : this("", ActionLevel.Normal)
    {
    }

    public GameAction(string info, ActionLevel lvl)
    {
      Info = info;
      Level = lvl;
    }

    public override string ToString()
    {
      return GetType().Name + " " + Info;
    }
  }

  public class GameStateAction : GameAction
  {
    public enum ActionType { Load, Save, NextLevel, PrevLevel, GameFinished, DemoFinished, EnteredLevel, ContextSwitched, HitGameOneEntry, Assert }
    public ActionType Type { get; set; }
    public AbstractGameLevel InvolvedNode { get => involvedNode; set => involvedNode = value; }

    AbstractGameLevel involvedNode;
  }

  public class DamageAppliedAction : GameAction
  {

  }

  public enum QuestActionKind { Unset, Accepted, AwaitingReward }

  public class QuestAction : GameAction
  {
    public QuestActionKind QuestActionKind { get; set; }
    public int QuestID { get; set; }
  }

  public enum ShorcutsBarActionKind { ShorcutsBarChanged }
  public class ShorcutsBarAction : GameAction
  {
    public int Digit { get; set; } = -1;
    public ShorcutsBarActionKind Kind { get; set; }
  }

  public enum InventoryActionKind { ItemAdded, ItemRemoved, DragDropDone }
  public enum InventoryActionDetailedKind { Unset, Collected, TradedDragDrop }

  public class InventoryAction : GameAction
  {
    public Loot Loot { get; set; }
    public InventoryActionKind Kind { get; set; }
    public InventoryBase Inv { get; set; }
    public InventoryActionDetailedKind DetailedKind { get; set; }

    public InventoryAction(InventoryBase inv)
    {
      Inv = inv;
    }
  }

  public class ResourceNeededAction : GameAction
  {

  }

  /// <summary>
  /// /////////////////////////////////////////////
  /// </summary>
  //public enum MerchantActionKind { Unset, Engaged }
  //public class MerchantAction : GameAction
  //{
  //  public Tiles.Merchant InvolvedTile { get; set; }
  //  public MerchantActionKind MerchantActionKind { get; set; }
  //}

  /// <summary>
  /// /////////////////////////////////////////////
  /// </summary>
  public enum AllyActionKind { Unset, Engaged }
  public class AllyAction : GameAction
  {
    public IAlly InvolvedTile { get; set; }
    public AllyActionKind AllyActionKind { get; set; }
  }

  /// <summary>
  /// 
  /// </summary>
  public enum InteractiveActionKind
  {
    Unset, DoorOpened, DoorClosed, DoorUnlocked, DoorLocked, Destroyed, ChestOpened, AppendedToLevel,
    HitPortal, HitGroundPortal, GroundPortalApproached, HitClosedStairs
  }
  public class InteractiveTileAction : GameAction
  {

    public Tiles.Interactive.InteractiveTile InvolvedTile { get; set; }
    public InteractiveActionKind InteractiveKind { get; set; }
    public InteractiveTileAction(Tiles.Interactive.InteractiveTile tile) { InvolvedTile = tile; }
    public InteractiveTileAction() { }
  }

  /// <summary>
  /// ///////////////
  /// </summary>
  public enum LootActionKind { Generated, Collected, PutOn, PutOff, Crafted, SpecialDrunk, Enchanted, Consumed, Identified }
  public class LootAction : GameAction
  {

    public Loot Loot
    {
      get;
      set;
    }

    //TODO can be named Kind ?
    public LootActionKind LootActionKind { get; set; }
    public EquipmentKind EquipmentKind { get; set; }
    public CurrentEquipmentKind CurrentEquipmentKind { get; set; }
    public bool CollectedFromDistance { get; set; }
    public bool GenerationAnimated { get; set; }
    public Tile Source { get; set; }

    public LootAction(Loot loot) { Loot = loot; }
    public LootAction() { }
  }

  public enum HeroActionKind { LeveledUp, ChangedLevel, Moved, HitWall, HitPrivateChest, HitLockedChest };
  public class HeroAction : GameAction
  {
    public Tile InvolvedTile { get; set; }

    public HeroActionKind Kind
    {
      get; set;
    }
  }

  public class GameInstructionAction : GameAction
  {
    public GameInstructionAction()
    {
      Level = ActionLevel.Important;
    }
  }

  public class TilesRevealedAction : GameAction
  {
    public IList<Tile> Revealed { get; set; }
    public bool Value { get; set; }//revealed or hidden?
  }

  public enum EnemyActionKind { Moved, /*Died,*/ AttackingHero, ChasingPlayer, RaiseCall, SpecialAction };

  public class EnemyAction : GameAction
  {

    public EnemyActionKind Kind;

    public Enemy Enemy
    {
      get;
      set;
    }
  }

  public class SoundRequestAction : GameAction
  {
    public string SoundName { get; set; }
  }

  public enum LivingEntityActionKind
  {
    Moved, Died, GainedDamage, ExperiencedEffect, EffectFinished, Trapped, Interacted, Missed, UsedSpell,
    FailedToCastSpell, GodsTurn, GodsPowerReleased, StrikedBack, BulkAttack, UsedPortal, Teleported, AppendedToLevel
  }

  public class PolicyAppliedAction : GameAction
  {
    public Policy Policy { get; set; }
  }

  public class LivingEntityAction : GameAction
  {
    public LivingEntityAction() { }
    public LivingEntityAction(LivingEntityActionKind kind)
    {
      this.Kind = kind;
    }

    public EffectType EffectType { get; set; }
    public MovePolicy MovePolicy { get; set; }

    public LivingEntity InvolvedEntity { get; set; }

    public LivingEntityActionKind Kind { get; set; }
    //public TileData TileData { get; set; }
    public double InvolvedValue { get; set; }

  }
}
