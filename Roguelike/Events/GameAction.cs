using Dungeons.Tiles;
using Roguelike.LootContainers;
using Roguelike.Managers;
using Roguelike.Policies;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Roguelike
{
  namespace Events
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

    public enum ShorcutsBarActionKind { ShorcutsBarChanged }
    public class ShorcutsBarAction : GameAction
    {
      public int Digit { get; set; } = -1;
      public ShorcutsBarActionKind Kind { get; set; }
    }

    public enum InventoryActionKind { ItemAdded, ItemRemoved}
    public class InventoryAction : GameAction
    {
      public Loot Item { get; set; }
      public InventoryActionKind Kind { get; set; }
      public Inventory Inv { get; set ; }

      public InventoryAction(Inventory inv)
      {
        Inv = inv;
      }
    }

    public class ResourceNeededAction : GameAction
    {

    }

    public enum InteractiveActionKind { Unset, DoorsUnlocked, DoorsLocked, Destroyed, ChestOpened, AppendedToLevel,
      HitPortal, HitGroundPortal, GroundPortalApproached }
    public class InteractiveTileAction : GameAction
    {
      
      public Tiles.InteractiveTile InvolvedTile { get; set; }
      public InteractiveActionKind InteractiveKind { get; set; }
      public InteractiveTileAction(Tiles.InteractiveTile tile) { InvolvedTile = tile; }
      public InteractiveTileAction() { }
    }
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

    public enum HeroActionKind { LeveledUp, ChangedLevel, Moved };
    public class HeroAction : GameAction
    {
      

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

    public enum EnemyActionKind { Moved, Died, AttackingHero, ChasingPlayer, AppendedToLevel, Teleported, RaiseCall, SpecialAction };

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
      Moved, Died, GainedDamage, ExperiencedEffect, Trapped, Interacted, Missed, UsedSpell,
      FailedToCastSpell, GodsTurn, GodsPowerReleased, StrikedBack, BulkAttack
    }

    public class PolicyAppliedAction : GameAction
    {
      public Policy Policy { get; set; }
    }

    public class LivingEntityAction : GameAction
    {
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
}
