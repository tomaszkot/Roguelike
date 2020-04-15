#define ASCII_BUILD  
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Roguelike.Abstract;
using System.Diagnostics;

namespace Roguelike.Tiles
{
  public enum EquipmentKind { Unset, Weapon, Armor, Helmet, Shield, RingLeft, RingRight, Amulet, TrophyLeft, TrophyRight, Gloves }
  public enum LootKind { Unset, Gold, Potion, Scroll, Equipment, Other, Gem,
                         Recipe, Seal, SealPart, Food, Plant }
  public enum LootSourceKind { Enemy, PlainChest, GoldChest, DeluxeGoldChest, Barrel }
  public enum EquipmentClass { Unset, Plain, Magic, MagicSecLevel, Unique }

  public class Loot : Tile//, IDescriptable
  {
    //public static EntityStatKind[] AttackingExtendedStats = new[] { EntityStatKind.Attack, EntityStatKind.FireAttack, 
    //EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack };
  //public static EntityStatKind[] AttackingNonPhysicalStats = new[] { EntityStatKind.FireAttack, EntityStatKind.PoisonAttack, 
 // EntityStatKind.ColdAttack, EntityStatKind.LightingAttack };
    public const char GoldSymbol = '$';
    public const char PotionSymbol = '!';
#if ASCII_BUILD
    public const ConsoleColor GoldColor = ConsoleColor.Yellow;
    public const ConsoleColor CyanColor = ConsoleColor.Cyan;
    public const ConsoleColor HealthPotionColor = ConsoleColor.Red;
    public const ConsoleColor ManaPotionColor = ConsoleColor.Blue;
#endif

    public LootKind LootKind { get; set; }
    public LootExtendedInfo ExtendedInfo { get; protected set; }
    int price;
    protected int basePrice = -1;

    public int Price
    {
      get { return price; }
      set
      {
        price = value;
        if (basePrice == -1)
          basePrice = price;
        //if (AssetName == "shark")
        //{
        //  int k = 0;
        //}
      }
    }

    public int PositionInPage { get; set; }
    public int PageIndex { get; set; }
    public bool Collected { get; set; }
    //public Guid StackedInventoryId { get; set; }

    public virtual bool Positionable
    {
      get { return false; }
    }

    protected string primaryStatDesc = "?";

    //public bool StackedInInventory { get { return StackedInventoryId != Guid.Empty; } }
    public bool StackedInInventory { get; set; }
    //public int StackedInInventoryCount { get; set; }

    public Guid Id
    {
      get
      {
        return id;
      }

      set
      {
        id = value;
      }
    }

    Guid id;


    public Loot() : base('?')
    {
      Price = 1;
      id = Guid.NewGuid();
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif

      ExtendedInfo = new LootExtendedInfo();
    }

    public virtual Loot CreateCrafted(Loot other)
    {
      return null;
    }

    public override bool Equals(object obj)
    {
      var other = obj as Loot;
      if (other == null)
        return false;
      if (this.StackedInInventory != other.StackedInInventory)
        return false;
      if (!this.StackedInInventory)
        return this.GetHashCode() == other.GetHashCode();

      return this.GetType() == other.GetType();
      
    }

    //bool placedInCraftSlot;

    //public bool PlacedInCraftSlot
    //{
    //  get { return placedInCraftSlot; }
    //  set { placedInCraftSlot = value; }
    //}

    public virtual bool IsConsumable()
    {
      return LootKind == LootKind.Food || this is Mushroom;//TODO
    }

    public override int GetHashCode()
    {
      return id.GetHashCode();
    }

    public virtual bool IsCraftableWith(Loot other)
    {
      return false;
    }
    public virtual bool IsCraftable()
    {
      return false;
    }

    static public bool operator ==(Loot a, Loot b)
    {
      if (Object.ReferenceEquals(a, b))
      {
        return true;
      }
      // If one is null, but not both, return false.
      if (((object)a == null) || ((object)b == null))
      {
        return false;
      }
      return a.Equals(b);
    }

    static public bool operator !=(Loot a, Loot b)
    {
      return !(a == b);
    }

    public override string ToString()
    {
      var res = Name;// + " " + Price + GoldSymbol;
      res += " " + this.Point.ToString();
      //res += " " + Id;
      res += ", tag ="+tag1;
      return res;
    }

    public virtual bool IsSameKind(Loot other)
    {
      return this.GetType() == other.GetType();
    }

    public virtual string PrimaryStatDescription
    {
      get { return primaryStatDesc; }
    }

    protected string[] extraStatDescription;
    public virtual string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }

    public virtual void HandleGenerationDone()
    {
      
    }


    //public virtual Loot Clone()
    //{
    //  var clone =  this.MemberwiseClone() as Loot;
    //  clone.ExtendedInfo = this.ExtendedInfo.Clone() as LootExtendedInfo;
    //  return clone;
    //}
  }
}
