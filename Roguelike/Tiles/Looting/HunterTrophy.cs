using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum HunterTrophyKind { Unset, Fang, Tusk, Claw }//Fang-Tooth
  
  public class HunterTrophy : Enchanter
  {
    public HunterTrophyKind TinyTrophyKind { get; set; }

    static Dictionary<HunterTrophyKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps = new Dictionary<HunterTrophyKind, Dictionary<EquipmentKind, EntityStatKind>>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsFang = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsTusk = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsClaw = new Dictionary<EquipmentKind, EntityStatKind>();

    

    public static string[] TinyTrophiesTags = new[] { Enchanter.Big+ "_claw", Enchanter.Big + "_fang", Enchanter.Medium + "_claw",
      Enchanter.Medium+"_fang", Enchanter.Small+"_claw", Enchanter.Small+"_fang" };


    public override string PrimaryStatDescription => primaryStatDescription;

    static HunterTrophy()
    {
      PopulateProps(enhancmentPropsFang, EntityStatKind.Defence, EntityStatKind.ChanceToBulkAttack, EntityStatKind.MeleeAttackDamageReduction);
      PopulateProps(enhancmentPropsTusk, EntityStatKind.Health, EntityStatKind.ChanceToCauseBleeding, EntityStatKind.Strength);
      PopulateProps(enhancmentPropsClaw, EntityStatKind.ChanceToHit, EntityStatKind.ChanceToStrikeBack, EntityStatKind.Dexterity);

      enhancmentProps[HunterTrophyKind.Fang] = enhancmentPropsFang;
      enhancmentProps[HunterTrophyKind.Tusk] = enhancmentPropsTusk;
      enhancmentProps[HunterTrophyKind.Claw] = enhancmentPropsClaw;
    }

    public override string GetId()
    {
      return base.GetId() + TinyTrophyKind + " " + EnchanterSize;
    }

    private static void PopulateProps
    (
      Dictionary<EquipmentKind, EntityStatKind> enhancmentProps, 
      EntityStatKind arm,
      EntityStatKind wpn, 
      EntityStatKind Juw
    )
    {
      enhancmentProps[EquipmentKind.Armor] = arm;
      enhancmentProps[EquipmentKind.Helmet] = arm;
      enhancmentProps[EquipmentKind.Shield] = arm;

      enhancmentProps[EquipmentKind.Amulet] = Juw;
      enhancmentProps[EquipmentKind.Ring] = Juw;
      enhancmentProps[EquipmentKind.Trophy] = Juw;

      enhancmentProps[EquipmentKind.Weapon] = wpn;
    }

    public HunterTrophy(HunterTrophyKind kind)
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.HunterTrophy;
      SetKind(kind);
    }

    private void SetKind(HunterTrophyKind kind)
    {
      TinyTrophyKind = kind;
      
      switch (kind)
      {
        case HunterTrophyKind.Unset:
          EnchantSrc = EnchantSrc.Unset;
          break;
        case HunterTrophyKind.Fang:
          Name = "Fang";
          EnchantSrc = EnchantSrc.Fang;
          primaryStatDescription = "Sharp, hard, ready to bite. " + Strings.PartOfCraftingRecipe;
          break;
        case HunterTrophyKind.Tusk:
          EnchantSrc = EnchantSrc.Tusk;
          Name = "Tusk";
          primaryStatDescription = "Big, sharp, ready to tear somebody apart. " + Strings.PartOfCraftingRecipe;
          break;
        case HunterTrophyKind.Claw:
          EnchantSrc = EnchantSrc.Claw;
          Name = "Claw";
          primaryStatDescription = "Sharp, hard, ready to claw. " + Strings.PartOfCraftingRecipe;
          break;
        default:
          break;
      }
    }

    //internal static EnchantSrc EnchantSrcFromTinyTrophyKind(TinyTrophyKind tinyTrophyKind)
    //{
    //  switch (tinyTrophyKind)
    //  {
    //    case TinyTrophyKind.Unset:
    //      return EnchantSrc.Unset;
    //    case TinyTrophyKind.Fang:
    //      return EnchantSrc.Fang;
    //    case TinyTrophyKind.Tusk:
    //      return EnchantSrc.Tusk;
    //    case TinyTrophyKind.Claw:
    //      return EnchantSrc.Claw;
    //    default:
    //      return EnchantSrc.Unset;
    //  }
    //}

    public override LootStatInfo[] GetLootStatInfo()
    {
      if (m_lootStatInfo == null)
      {
        var lootStatInfos = new List<LootStatInfo>();
        var props = enhancmentProps[this.TinyTrophyKind];

        var lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Weapons: "+ props[EquipmentKind.Weapon].ToDescription() + " " + wpnAndArmorValues[this.EnchanterSize];
        lootStatInfo.Kind = LootStatKind.Weapon;
        lootStatInfos.Add(lootStatInfo);

        lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Armor: " + props[EquipmentKind.Armor].ToDescription() + " " + wpnAndArmorValues[this.EnchanterSize];
        lootStatInfo.Kind = LootStatKind.Armor;
        lootStatInfos.Add(lootStatInfo);

        lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Jewellery: " + props[EquipmentKind.Amulet].ToDescription() + " " + otherValues[this.EnchanterSize];
        lootStatInfo.Kind = LootStatKind.Jewellery;
        lootStatInfos.Add(lootStatInfo);

        m_lootStatInfo = lootStatInfos.ToArray();
      }
      return base.GetLootStatInfo();
    }

    public override bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var props = enhancmentProps[this.TinyTrophyKind];
      if (props.ContainsKey(eq.EquipmentKind))
      {
        var esk = props[eq.EquipmentKind];
        int val = 0;
        if (eq.EquipmentKind == EquipmentKind.Amulet || eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        {
          val = otherValues[this.EnchanterSize];
        }
        else
        {
          val = wpnAndArmorValues[this.EnchanterSize];
        }
        return eq.Enchant(esk, val, this, out error);
      }

      return false;
    }

  }
}
