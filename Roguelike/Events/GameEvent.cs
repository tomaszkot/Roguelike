using Dungeons.Tiles;
using Roguelike.Abstract.Tiles;
using Roguelike.Effects;
using Roguelike.Extensions;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;


namespace Roguelike.Events
{
  public enum ActionLevel
  {
    Normal,
    Important,
  }

  public class GameEvent
  {
    public string Info { get; set; } = "";
    public ActionLevel Level { get; set; }
    public int Index { get; set; }
    public virtual string GetSound() { return ""; }

    public GameEvent() : this("", ActionLevel.Normal)
    {
    }

    public GameEvent(string info, ActionLevel lvl)
    {
      Info = info;
      Level = lvl;
    }

    public override string ToString()
    {
      return GetType().Name + " " + Info;
    }
  }

  public class GameStateAction : GameEvent
  {
    public enum ActionType { Load, Save, NextLevel, PrevLevel, GameFinished, DemoFinished, EnteredLevel, ContextSwitched, HitGameOneEntry, Assert }
    public ActionType Type { get; set; }
    public AbstractGameLevel InvolvedNode { get => involvedNode; set => involvedNode = value; }

    AbstractGameLevel involvedNode;
  }

  public class DamageAppliedAction : GameEvent
  {

  }

  public enum QuestActionKind { Unset, Accepted, AwaitingReward }

  public class QuestAction : GameEvent
  {
    public QuestActionKind QuestActionKind { get; set; }
    public int QuestID { get; set; }
  }

  public enum ShorcutsBarActionKind { ShorcutsBarChanged }
  public class ShorcutsBarAction : GameEvent
  {
    public int Digit { get; set; } = -1;
    public ShorcutsBarActionKind Kind { get; set; }
  }

  public enum InventoryActionKind { ItemAdded, ItemRemoved, DragDropDone }
  public enum InventoryActionDetailedKind { Unset, Collected, TradedDragDrop }

  public class InventoryAction : GameEvent
  {
    public Loot Loot { get; set; }
    public InventoryActionKind Kind { get; set; }
    public Inventory Inv { get; set; }
    public InventoryActionDetailedKind DetailedKind { get; set; }

    public InventoryAction(Inventory inv)
    {
      Inv = inv;
    }
  }

  public class ResourceNeededAction : GameEvent
  {

  }

  /// <summary>
  /// /////////////////////////////////////////////
  /// </summary>
  public enum AllyActionKind { Unset, Engaged, Created, Died }
  public class AllyAction : GameEvent
  {
    public IAlly InvolvedTile { get; set; }
    public AllyActionKind AllyActionKind { get; set; }
  }

  /// <summary>
  /// /////////////////////////////////////////////
  /// </summary>
  public enum NPCActionKind { Unset, Engaged, Died }
  public class NPCAction : GameEvent
  {
    public INPC InvolvedTile { get; set; }
    public NPCActionKind NPCActionKind { get; set; }
  }

  /// <summary>
  /// 
  /// </summary>
  public enum InteractiveActionKind
  {
    Unset, DoorOpened, DoorClosed, DoorUnlocked, DoorLocked, Destroyed, ChestOpened, AppendedToLevel,
    HitPortal, HitGroundPortal, GroundPortalApproached, HitClosedStairs
  }
  public class InteractiveTileAction : GameEvent
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
  public class LootAction : GameEvent
  {

    public Loot Loot
    {
      get;
      set;
    }

    public LootActionKind Kind { get; set; }
    public EquipmentKind EquipmentKind { get; set; }
    public CurrentEquipmentKind CurrentEquipmentKind { get; set; }
    public bool CollectedFromDistance { get; set; }
    public bool GenerationAnimated { get; set; }
    public Tile Source { get; set; }
    public LivingEntity LootOwner { get; set; }

    public LootAction(Loot loot, LivingEntity lootOwner) { Loot = loot; LootOwner = lootOwner; }
    public LootAction() { }
  }

  public enum HeroActionKind { ChangedLevel, Moved, HitWall, HitPrivateChest, HitLockedChest };
  public class HeroAction : GameEvent
  {
    public Tile InvolvedTile { get; set; }

    public HeroActionKind Kind
    {
      get; set;
    }
  }

  public class GameInstructionAction : GameEvent
  {
    public GameInstructionAction()
    {
      Level = ActionLevel.Important;
    }
  }

  public class TilesRevealedAction : GameEvent
  {
    public IList<Tile> Revealed { get; set; }
    public bool Value { get; set; }//revealed or hidden?
  }

  public enum EnemyActionKind { Moved, /*Died,*/ AttackingHero, ChasingPlayer, RaiseCall, SpecialAction, Teleported };

  public class EnemyAction : GameEvent
  {

    public EnemyActionKind Kind;

    public Enemy Enemy
    {
      get;
      set;
    }
  }

  public class SoundRequestAction : GameEvent
  {
    public string SoundName { get; set; }
  }

  public enum LivingEntityActionKind
  {
    LeveledUp, Moved, Died, GainedDamage, ExperiencedEffect, EffectFinished, Trapped, Interacted, Missed, UsedSpell,
    FailedToCastSpell, GodsTurn, GodsPowerReleased, StrikedBack, BulkAttack, UsedPortal, Teleported, AppendedToLevel,
    StateChanged
  }

  public class PolicyAppliedAction : GameEvent
  {
    public Policy Policy { get; set; }
  }

  public class LivingEntityStateChangedEvent : LivingEntityAction
  {
    public EntityState Old { get; set; }
    public EntityState New { get; set; }

    public LivingEntityStateChangedEvent(EntityState oldState, EntityState newState, LivingEntity involvedEntity)
    {
      Old = oldState;
      New = newState;
      this.Kind = LivingEntityActionKind.StateChanged;
      this.InvolvedEntity = involvedEntity;
      Info = InvolvedEntity.Name + " state changed to "+New.ToDescription();
    }
  }

  public class LivingEntityAction : GameEvent
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
    public InteractionResult InteractionResult { get; set; }

  }
}
