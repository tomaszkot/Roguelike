#define ASCII_BUILD  
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles
{
  class Strings
  {
    public const string PartOfCraftingRecipe = "Part of the crafting recipe.";
    public const string ConsumeDescPart = "";// Press Right Mouse Button to consume.";
    public const string DropOnEnchantable = "Drop it on an enchantable item in the Inventory.";
  }

  [Flags]
  public enum LootStatKind
  {
    Unset = 0,
    Weapon = 1,
    Armor = 2,
    Jewellery = 4,
    //Health = 8,
    //Mana = 16
  }

  public class LootStatInfo
  {
    public string Desc { get; set; }
    public LootStatKind Kind { get; set; }
    public EntityStatKind EntityStatKind { get; set; }

    public override string ToString()
    {
      return Kind + " " + EntityStatKind + " " + Desc;
    }
  }

  public enum EquipmentKind
  {
    Unset, Weapon, Armor, Helmet, Shield, Ring, Amulet,
    Trophy, Glove, God
  }

  public enum CurrentEquipmentKind
  {
    Unset, Weapon, Armor, Helmet, Shield, RingLeft, Amulet,
    TrophyLeft, Glove, God, RingRight, TrophyRight
  }

  public enum CurrentEquipmentPosition { Unset, Left, Right }

  public enum LootKind
  {
    Unset,
    Other, //MagicDust...
    Gold, Potion, Scroll, Equipment, Gem,
    Recipe, Seal, SealPart, Food, Plant, HunterTrophy, Book
  }

  public enum LootSourceKind { Enemy, PlainChest, GoldChest, DeluxeGoldChest, Barrel }
  public enum EquipmentClass { Unset, Plain, Magic, MagicSecLevel, Unique }

  public abstract class Loot : Tile//, IDescriptable
  {
    //public static EntityStatKind[] AttackingExtendedStats = new[] { EntityStatKind.Attack, EntityStatKind.FireAttack, 
    //EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack };
    public static EntityStatKind[] AttackingNonPhysicalStats = new[] { EntityStatKind.FireAttack, EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack, EntityStatKind.LightingAttack };
    public const char GoldSymbol = '$';
    public const char PotionSymbol = '!';
#if ASCII_BUILD
    public const ConsoleColor GoldColor = ConsoleColor.Yellow;
    public const ConsoleColor CyanColor = ConsoleColor.Cyan;
    public const ConsoleColor HealthPotionColor = ConsoleColor.Red;
    public const ConsoleColor ManaPotionColor = ConsoleColor.Blue;
#endif

    public LootKind LootKind { get; set; }
    public string SourceQuestKind { get; set; }

    int price;
    protected int basePrice = -1;
    protected string collectedSound = "cloth";
    string droppedSound = "";

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

    public virtual bool Positionable
    {
      get { return false; }
    }

    //protected string primaryStatDesc = "?";

    public bool StackedInInventory
    {
      get { return this is Roguelike.Tiles.Looting.StackedLoot; }
    }

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

    /// <summary>
    /// ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    public Loot() : base('?')
    {
      Price = 1;
      Id = Guid.NewGuid();
#if ASCII_BUILD
      color = ConsoleColor.Green;
#endif
    }

    //public virtual Loot CreateCrafted(Loot other)
    //{
    //  return null;
    //}

    public override bool Equals(object obj)
    {
      var other = obj as Loot;
      if (other == null)
        return false;
      
      return this.Id == other.Id;
    }

    public virtual bool IsConsumable()
    {
      return this is IConsumable;
    }

    public override int GetHashCode()
    {
      return id.GetHashCode();
    }

    //public virtual bool IsCraftableWith(Loot other)
    //{
    //  return false;
    //}

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
      var res = base.ToString();
      if (IncludeDebugDetailsInToString)
      {
        res += ", " + LootKind;
        res += ", tag =" + tag1;
      }
      return res;
    }

    public virtual bool IsSameKind(Loot other)
    {
      return this.GetType() == other.GetType();
    }

    public virtual string PrimaryStatDescription
    {
      get;
      set;
    } = "";

    protected string[] extraStatDescription;

    public virtual string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }

    protected List<LootStatInfo> m_lootStatInfo = new List<LootStatInfo>();
    public virtual List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      return m_lootStatInfo;
    }

    public virtual void HandleGenerationDone()
    {

    }

    public static T DiscoverKindFromName<T>(string name)
    {
      var kinds = Enum.GetValues(typeof(T)).Cast<T>().ToList();
      return kinds.FirstOrDefault(i => name.Contains(i.ToString().ToLower()));
    }

    public string CollectedSound
    {
      get { return collectedSound; }
    }

    public string DroppedSound
    {
      get
      {
        return droppedSound.Any() ? droppedSound : collectedSound;
      }
    }

    protected static string PartOfCraftingRecipe
    {
      get { return "Part of a crafting recipe."; }
    }

    public ILootSource source;
    [JsonIgnore]
    public ILootSource Source
    {
      get { return source; }
      internal set
      {
        if (this is Equipment eq && eq.IsPlain() && eq.EnchantSlots < 2)
        {
          int k = 0;
          k++;
        }
        source = value;
      }
    }
  }
}
