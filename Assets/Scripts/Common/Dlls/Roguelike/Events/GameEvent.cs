﻿using Dungeons.Tiles;
using Roguelike.Abilities;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Extensions;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.Spells;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
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
    Guid guid;

    public string TurnHint { get; set; } = "";
    public string Info { get; set; } = "";
    public ActionLevel Level { get; set; }
    public int Index { get; set; }
    public Guid Guid { get => guid; set => guid = value; }
    public bool ShowHint { get; set; }
    public bool Silent { get; internal set; }

    public virtual string GetSound() { return ""; }

    public GameEvent() : this("", ActionLevel.Normal)
    {
    }

    public GameEvent(string info, ActionLevel lvl)
    {
      Info = info;
      Level = lvl;
      guid = Guid.NewGuid();
    }

    public override string ToString()
    {
      return GetType().Name + " " + Info;
    }
  }

 

  public class GameStateAction : GameEvent
  {
    public enum ActionType { Load, Save, NextLevel, PrevLevel, GameFinished, DemoFinished, EnteredLevel, ContextSwitched, HitGameOneEntry, Assert,
      AutoQuickSaveStarting, AutoQuickSaveFinished, SaveFinished }
    public ActionType Type { get; set; }
    public AbstractGameLevel InvolvedNode { get => involvedNode; set => involvedNode = value; }
    public Tile InvolvedTile { get; internal set; }

    AbstractGameLevel involvedNode;
  }

  public class DamageAppliedAction : GameEvent
  {

  }

  public enum QuestActionKind { Unset, Accepted, AwaitingReward , Done}

  public class QuestAction : GameEvent
  {
    public QuestActionKind QuestActionKind { get; set; }
    public int QuestID { get; set; }
  }

  public enum ShortcutsBarActionKind { ShortcutsBarChanged, ActiveItemDigitChanged, ContainerIndexChanged, RightMouseButton }
  public class ShortcutsBarAction : GameEvent
  {
    public int Digit { get; set; } = -1;
    public ShortcutsBarActionKind Kind { get; set; }
  }

  public enum InventoryActionKind { ItemAdded, ItemRemoved, DragDropDone, NotEnoughRoom, ItemTooExpensive, NeedsRebind }
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
  public enum AllyActionKind { Unset, Engaged, Created, Died, Released }
  public class AllyAction : GameEvent
  {
    public IAlly InvolvedTile { get; set; }
    public AllyActionKind AllyActionKind { get; set; }
  }

  /// <summary>
  /// /////////////////////////////////////////////
  /// </summary>
  public enum NPCActionKind { Unset, Engaged, Died, NotToLooseVigor }
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
    HitPortal, HitGroundPortal, GroundPortalApproached, HitClosedStairs, DeadBodyLooted, HitWhenLooted, LeverSwitched, Hit
  }
  public class InteractiveTileAction : GameEvent
  {

    public Tiles.Interactive.InteractiveTile InvolvedTile { get; set; }
    public InteractiveActionKind InteractiveKind { get; set; }
    public bool PlaySound { get; internal set; } = true;

    public InteractiveTileAction(Tiles.Interactive.InteractiveTile tile) { InvolvedTile = tile; }
    public InteractiveTileAction() { }
  }

  public class NpcAction : GameEvent
  {
    public DestPointDesc DestPointDesc { get; set; }
    public INPC InvolvedTile { get; set; }

    public Tile MoveOnPathTarget { get; set; }

    public NpcAction(INPC involvedTile, DestPointDesc destPointDesc, Tile moveOnPathTarget)
    {
      InvolvedTile = involvedTile;
      DestPointDesc = destPointDesc;
      MoveOnPathTarget = moveOnPathTarget;
    }
  }

  /// <summary>
  /// ///////////////
  /// </summary>
  public enum LootActionKind { Generated, Collected, PutOn, PutOff, Crafted, SpecialDrunk, Enchanted, Consumed, Identified, 
    Deactivated, Destroyed }
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

  public enum SurfaceActionKind
  {
    Created, Destroyed
  }
  public class SurfaceAction : GameEvent
  {
    public SurfaceActionKind Kind { get; set; }
    public Surface Surface { get; set; }

    public SurfaceAction(Surface sur, SurfaceActionKind kind) 
    {
      Surface = sur;
      Kind = kind;
    }

  }

  public enum HeroActionKind { ChangedLevel, Moved, HitWall, HitPrivateChest, HitLockedChest, EnteredInteractive ,
    LeftInteractive
  };
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

  public enum EntityCommandKind
  {
    Unset, Resurrect, SorroundHim, SenseVictimWeakResist, MakeFakeClones, TeleportAway,
    TeleportCloser
  }

  public enum EnemyActionKind { Moved, /*Died,*/ AttackingHero, ChasingPlayer, SendComand };

  public class EnemyAction : GameEvent
  {
    public EnemyActionKind Kind;
    public CommandUseInfo Command { get; set; }
    //public EntityCommandKind CommandKind;

    public Enemy Enemy
    {
      get;
      set;
    }

    public EnemyAction(CommandUseInfo command) 
    {
      Command = command;
    }
  }

  public class AbilityStateChangedEvent : GameEvent
  {
    public AbilityKind AbilityKind { get; set; }
    public AbilityState AbilityState { get; set; }

    public LivingEntity AbilityUser { get; set; }

    //public AbilityVisualState AbilityVisualState { get; set; }

    public override string ToString()
    {
      return base.ToString() + " " + AbilityKind + " " + AbilityState;// + " " + AbilityVisualState;
    }
  }

  public class SoundRequestAction : GameEvent
  {
    public string SoundName { get; set; }
  }

  public enum LivingEntityActionKind
  {
    LeveledUp, Moved, Died, GainedDamage, ExperiencedEffect, EffectFinished, Trapped, Interacted, Missed, UsedSpell,
    FailedToCastSpell, GodsTurn, GodsPowerReleased, UsedPortal, Teleported, AppendedToLevel,
    StateChanged, 
    /*UsedAbility, */
    MadeBattleOrder, 
    SummonedBuddy, 
    SwappedPos, 
    SendCommand
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
    public EntityCommandKind CommandKind { get; set; }
    public EffectType EffectType { get; set; }
    public MovePolicy MovePolicy { get; set; }

    public string Sound { get; set; }

    public LivingEntity InvolvedEntity { get; set; }
    public LivingEntity AttackerEntity { get; set; }

    public LivingEntityActionKind Kind { get; set; }
    public double InvolvedValue { get; set; }
    public InteractionResult InteractionResult { get; set; }

    public SpellKind UsedSpellKind { get; set; }
    public Dungeons.Tiles.Abstract.IHitable Missed { get; internal set; }
    public AttackKind AttackKind { get; internal set; }
    public CommandUseInfo Cmd { get; internal set; }
   
  }

  public class TilesAppendedEvent : GameEvent
  {
    public List<Tile> Tiles { get; set; }
  }
}
