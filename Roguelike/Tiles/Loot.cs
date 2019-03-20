#define ASCII_BUILD  
using Dungeons.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using Roguelike.Abstract;

namespace Roguelike.Tiles
{
  public enum EquipmentKind { Weapon, Armor, Helmet, Shield, Ring, Amulet, TrophyLeft, TrophyRight }
  public enum LootKind { Gold, Potion, Scroll, Weapon, Armor, Jewellery, Other, Gem, Recipe, Trophy, Seal, SealPart }
  public enum EquipmentClass { Plain, Magic, Unique }

  public class Loot : Tile//, IDescriptable
  {
    //public static EntityStatKind[] AttackingExtendedStats = new[] { EntityStatKind.Attack, EntityStatKind.FireAttack, EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack };
    //public static EntityStatKind[] AttackingNonPhysicalStats = new[] { EntityStatKind.FireAttack, EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack, EntityStatKind.LightingAttack };
    public const char GoldSymbol = '$';
    public const char PotionSymbol = '!';
#if ASCII_BUILD
    public const ConsoleColor GoldColor = ConsoleColor.Yellow;
    public const ConsoleColor CyanColor = ConsoleColor.Cyan;
    public const ConsoleColor HealthPotionColor = ConsoleColor.Red;
    public const ConsoleColor ManaPotionColor = ConsoleColor.Blue;
#endif
    public enum PotionKind { Health, Mana }

    int price;
    public int Price
    {
      get { return price; }
      set
      {
        price = value;
        //if (AssetName == "shark")
        //{
        //  int k = 0;
        //}
      }
    }

    public int PositionInPage { get; set; }
    public int PageIndex { get; set; }
    public bool Collected { get; set; }
    public Guid StackedInventoryId { get; set; }

    public virtual bool Positionable
    {
      get { return false; }
    }

    public bool StackedInInventory { get { return StackedInventoryId != Guid.Empty; } }

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
    }

    public virtual Loot CreateCrafted(Loot other)
    {
      return null;
    }

    public override bool Equals(object obj)
    {

      Loot other = obj as Loot;
      if ((object)other == null)
      {
        return false;
      }
      if (this.StackedInInventory != other.StackedInInventory)
        return false;
      if (this.StackedInInventory)
        return this.StackedInventoryId == other.StackedInventoryId;
      return other.Id == Id;
    }

    bool placedInCraftSlot;

    public bool PlacedInCraftSlot
    {
      get { return placedInCraftSlot; }
      set { placedInCraftSlot = value; }
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
      res += " " + this.Point.ToString() + " " + Id + ", tag ="+tag;
      return res;
    }

    public virtual bool IsSameKind(Loot other)
    {
      return this.GetType() == other.GetType();
    }

    public virtual string GetPrimaryStatDescription()
    {
      return "?";
    }

    protected string[] extraStatDescription;
    public virtual string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }


    public virtual Loot Clone()
    {
      return this.MemberwiseClone() as Loot;
    }
  }
}
