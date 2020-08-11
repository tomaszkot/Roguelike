using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum TinyTrophyKind { Unset, Fang, Tusk, Claw }//Fang-Tooth
  public enum TinyTrophySize { Small, Medium, Big }

  public class TinyTrophy : StackedLoot
  {
    public TinyTrophyKind TinyTrophyKind { get; set; }
    public TinyTrophySize TinyTrophySize { get; set; } = TinyTrophySize.Small;
    public string primaryStatDescription;

    static Dictionary<TinyTrophySize, int> wpnAndArmorValues = new Dictionary<TinyTrophySize, int>();
    static Dictionary<TinyTrophySize, int> otherValues = new Dictionary<TinyTrophySize, int>();
    static Dictionary<TinyTrophyKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps = new Dictionary<TinyTrophyKind, Dictionary<EquipmentKind, EntityStatKind>>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsFang = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsTusk = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsClaw = new Dictionary<EquipmentKind, EntityStatKind>();


    public override string PrimaryStatDescription => primaryStatDescription;

    static TinyTrophy()
    {
      wpnAndArmorValues[TinyTrophySize.Big] = 6;
      wpnAndArmorValues[TinyTrophySize.Medium] = 4;
      wpnAndArmorValues[TinyTrophySize.Small] = 2;

      otherValues[TinyTrophySize.Big] = 15;
      otherValues[TinyTrophySize.Medium] = 10;
      otherValues[TinyTrophySize.Small] = 5;

      PopulateProps(enhancmentPropsFang, EntityStatKind.Defence, EntityStatKind.ChanceToBulkAttack, EntityStatKind.MeleeAttackDamageReduction);
      PopulateProps(enhancmentPropsTusk, EntityStatKind.Health, EntityStatKind.ChanceToCauseBleeding, EntityStatKind.Strength);
      PopulateProps(enhancmentPropsClaw, EntityStatKind.ChanceToHit, EntityStatKind.ChanceToStrikeBack, EntityStatKind.Dexterity);

      enhancmentProps[TinyTrophyKind.Fang] = enhancmentPropsFang;
      enhancmentProps[TinyTrophyKind.Tusk] = enhancmentPropsTusk;
      enhancmentProps[TinyTrophyKind.Claw] = enhancmentPropsClaw;
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

    public TinyTrophy(TinyTrophyKind kind)
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.TinyTrophy;
      SetKind(kind);
    }

    private void SetKind(TinyTrophyKind kind)
    {
      TinyTrophyKind = kind;
      
      switch (kind)
      {
        case TinyTrophyKind.Unset:
          break;
        case TinyTrophyKind.Fang:
          Name = "Fang";
          primaryStatDescription = "Sharp, hard, ready to bite. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Tusk:
          Name = "Tusk";
          primaryStatDescription = "Big, sharp, ready to tear somebody apart. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Claw:
          Name = "Claw";
          primaryStatDescription = "Sharp, hard, ready to claw. " + Strings.PartOfCraftingRecipe;
          break;
        default:
          break;
      }
    }

    internal static EnchantSrc EnchantSrcFromTinyTrophyKind(TinyTrophyKind tinyTrophyKind)
    {
      switch (tinyTrophyKind)
      {
        case TinyTrophyKind.Unset:
          return EnchantSrc.Unset;
        case TinyTrophyKind.Fang:
          return EnchantSrc.Fang;
        case TinyTrophyKind.Tusk:
          return EnchantSrc.Tusk;
        case TinyTrophyKind.Claw:
          return EnchantSrc.Claw;
        default:
          return EnchantSrc.Unset;
      }
    }

    public bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var props = enhancmentProps[this.TinyTrophyKind];
      if (props.ContainsKey(eq.EquipmentKind))
      {
        var propsGem = props[eq.EquipmentKind];
        int val = 0;
        if (eq.EquipmentKind == EquipmentKind.Amulet || eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        {
          val = otherValues[this.TinyTrophySize];
        }
        else
        {
          val = wpnAndArmorValues[this.TinyTrophySize];
        }
        return eq.Enchant(propsGem, val, EnchantSrcFromTinyTrophyKind(TinyTrophyKind), out error);
      }

      return false;
    }

  }
}
