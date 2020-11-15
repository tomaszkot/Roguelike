#define ASCII_BUILD  
using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using System;
using System.Linq;

namespace Roguelike.Tiles
{
  class Strings
  {
    public const string PartOfCraftingRecipe = "Part of the crafting recipe.";
    public const string ConsumeDescPart = "";// Press Right Mouse Button to consume.";
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

  public enum EquipmentKind { Unset, Weapon, Armor, Helmet, Shield, Ring, Amulet,
                              Trophy, Glove, God }

  public enum CurrentEquipmentKind
  {
    Unset, Weapon, Armor, Helmet, Shield, RingLeft, Amulet,
    TrophyLeft, Glove, God, RingRight, TrophyRight
  }

  public enum CurrentEquipmentPosition { Unset, Left, Right}

  public enum LootKind { Unset, 
                         Other, //MagicDust...
                         Gold, Potion, Scroll, Equipment, Gem,
                         Recipe, Seal, SealPart, Food, Plant, HunterTrophy}

  public enum LootSourceKind { Enemy, PlainChest, GoldChest, DeluxeGoldChest, Barrel }
  public enum EquipmentClass { Unset, Plain, Magic, MagicSecLevel, Unique }

  public abstract class Loot : Tile//, IDescriptable
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

    protected string primaryStatDesc = "?";
        
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
      var other = obj as Loot;
      if (other == null)
        return false;
      if (this.StackedInInventory != other.StackedInInventory)
        return false;
      if (!this.StackedInInventory)
        return this.GetHashCode() == other.GetHashCode();

      return (this as Looting.StackedLoot).GetId() == (other as Looting.StackedLoot).GetId();
    }

    public virtual bool IsConsumable()
    {
      //return LootKind == LootKind.Food //|| this is Mushroom //TODO
      //  || LootKind == LootKind.Potion;
      return this is IConsumable;
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

    public abstract string PrimaryStatDescription
    {
      get;
    }

    protected string[] extraStatDescription;
    
    public virtual string[] GetExtraStatDescription()
    {
      return extraStatDescription;
    }

    protected LootStatInfo[] m_lootStatInfo;
    public virtual LootStatInfo[] GetLootStatInfo(LivingEntity caller)
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

    protected string PartOfCraftingRecipe
    {
      get { return "Part of a crafting recipe."; }
    }
  }
}
