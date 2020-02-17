using Dungeons.Tiles;
using Roguelike.LootContainers;
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
      public enum ActionType { Load, Save, NextLevel, PrevLevel, GameFinished, DemoFinished, EnteredLevel, ContextSwitched, Assert }
      public ActionType Type { get; set; }
      public GameNode InvolvedNode { get => involvedNode; set => involvedNode = value; }

      GameNode involvedNode;
    }

    public class DamageAppliedAction : GameAction
    {

    }

    public enum InventoryActionKind { ItemAdded, ItemRemoved, ShorcutsBarChanged}
    public class InventoryAction : GameAction
    {
      public Loot Item { get; set; }
      public InventoryActionKind Kind { get; set; }
      public Inventory Inv { get; set ; }

      //Inventory inv;

      public InventoryAction(Inventory inv)
      {
        Inv = inv;
      }
    }

    public class ResourceNeededAction : GameAction
    {

    }

    public class InteractiveTileAction : GameAction
    {
      public enum Kind { Unset, DoorsUnlocked, DoorsLocked, Destroyed }
      public Tiles.InteractiveTile Tile { get; set; }
      public Kind KindValue { get; set; }
      public InteractiveTileAction(Tiles.InteractiveTile tile) { Tile = tile; }
      //public Tiles.Door Door { get; set; }

      //public override string GetSound
      //{
      //  get
      //  {
      //    if (KindValue == Kind.DoorsLocked)
      //      return "door_locked";
      //    else if (KindValue == Kind.DoorsUnlocked)
      //      return "door_locked";
      //    return "";
      //  }
      //}
    }
    public enum LootActionKind { Generated, Collected, PutOn, TookOff, Crafted, SpecialDrunk, Enchanted, Consumed }
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

      public LootAction(Loot loot) { Loot = loot; }
    }

    public class HeroAction : GameAction
    {
      public enum Kind { LeveledUp, ChangedLevel };

      public Kind KindValue
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
      public List<Tile> Revealed { get; set; }
      public bool Value { get; set; }//revealed or hidden?
    }

    //public class EnemyAction : GameAction
    //{
    //  public enum Kind { Moved, Died, AttackingHero, ChasingPlayer, AppendedToLevel, Teleported, RaiseCall, SpecialAction };
    //  public Kind KindValue;

    //  public Enemy Enemy
    //  {
    //    get;
    //    set;
    //  }
    //}

    public class LivingEntityAction : GameAction
    {
      public LivingEntityAction(Kind kind)
      {
        this.KindValue = kind;
      }

      public LivingEntity InvolvedEntity { get; set; }
      public enum Kind
      {
        Moved, Died, GainedDamage, ExperiencedEffect, Trapped, Interacted, Missed, UsedSpell,
        FailedToCastSpell, GodsTurn, GodsPowerReleased, StrikedBack, BulkAttack
      }
      public Kind KindValue { get; set; }
      //public TileData TileData { get; set; }
      public double InvolvedValue { get; set; }

      public override string GetSound()
      {
        var sound = "";
        //if (TileData != null)
        //  sound = TileData.GetSound(KindValue);
        if (sound.Any())
          return sound;
        //var KindVa = KindValue.ToString();
        //if (KindValue == Kind.Moved)
        //  return "living_ent_moved";
        return GetType().Name.Replace("Action", "") + KindValue;
      }
    }
  }
}
