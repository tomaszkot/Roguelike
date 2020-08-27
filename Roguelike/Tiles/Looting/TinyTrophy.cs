using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum TinyTrophyKind { Unset, Fang, Tusk, Claw }//Fang-Tooth
  
  public class TinyTrophy : Enchanter
  {
    public TinyTrophyKind TinyTrophyKind { get; set; }

    static Dictionary<TinyTrophyKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps = new Dictionary<TinyTrophyKind, Dictionary<EquipmentKind, EntityStatKind>>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsFang = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsTusk = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsClaw = new Dictionary<EquipmentKind, EntityStatKind>();


    public override string PrimaryStatDescription => primaryStatDescription;

    static TinyTrophy()
    {
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
          EnchantSrc = EnchantSrc.Unset;
          break;
        case TinyTrophyKind.Fang:
          Name = "Fang";
          EnchantSrc = EnchantSrc.Fang;
          primaryStatDescription = "Sharp, hard, ready to bite. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Tusk:
          EnchantSrc = EnchantSrc.Tusk;
          Name = "Tusk";
          primaryStatDescription = "Big, sharp, ready to tear somebody apart. " + Strings.PartOfCraftingRecipe;
          break;
        case TinyTrophyKind.Claw:
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

    public override bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var props = enhancmentProps[this.TinyTrophyKind];
      if (props.ContainsKey(eq.EquipmentKind))
      {
        var propsGem = props[eq.EquipmentKind];
        int val = 0;
        if (eq.EquipmentKind == EquipmentKind.Amulet || eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        {
          val = otherValues[this.EnchanterSize];
        }
        else
        {
          val = wpnAndArmorValues[this.EnchanterSize];
        }
        return eq.Enchant(propsGem, val, this, out error);
      }

      return false;
    }

  }
}
