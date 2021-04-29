using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

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



    public static string[] TinyTrophiesTags = new[]
    {
      Enchanter.Big+ "_claw", Enchanter.Big + "_fang",
      Enchanter.Medium + "_claw",Enchanter.Medium+"_fang",
      Enchanter.Small+"_claw", Enchanter.Small+"_fang"
    };

    static HunterTrophy()
    {
      PopulateProps(enhancmentPropsFang, EntityStatKind.Defense, EntityStatKind.ChanceToBulkAttack, EntityStatKind.ChanceToEvadeMeleeAttack);
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
      SetName(kind.ToDescription());
      SetPrice();

      switch (kind)
      {
        case HunterTrophyKind.Unset:
          EnchantSrc = EnchantSrc.Unset;
          break;
        case HunterTrophyKind.Fang:

          EnchantSrc = EnchantSrc.Fang;
          PrimaryStatDescription = "Sharp, hard, ready to bite. ";
          break;
        case HunterTrophyKind.Tusk:
          EnchantSrc = EnchantSrc.Tusk;
          PrimaryStatDescription = "Big, sharp, ready to tear somebody apart. ";
          break;
        case HunterTrophyKind.Claw:
          EnchantSrc = EnchantSrc.Claw;
          PrimaryStatDescription = "Sharp, hard, ready to claw. ";
          break;
        default:
          break;
      }

      if (PrimaryStatDescription.Any())
        PrimaryStatDescription += Strings.DropOnEnchantable;
    }


    public override LootStatInfo[] GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null)
      {
        var lootStatInfos = new List<LootStatInfo>();
        var props = enhancmentProps[this.TinyTrophyKind];

        var lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Weapons: " + props[EquipmentKind.Weapon].ToDescription() + " " + GetStatIncrease(EquipmentKind.Weapon);
        lootStatInfo.Kind = LootStatKind.Weapon;
        lootStatInfos.Add(lootStatInfo);

        lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Armor: " + props[EquipmentKind.Armor].ToDescription() + " " + GetStatIncrease(EquipmentKind.Weapon);
        lootStatInfo.Kind = LootStatKind.Armor;
        lootStatInfos.Add(lootStatInfo);

        lootStatInfo = new LootStatInfo();
        lootStatInfo.Desc = "Jewellery: " + props[EquipmentKind.Amulet].ToDescription() + " " + GetStatIncrease(EquipmentKind.Ring);
        lootStatInfo.Kind = LootStatKind.Jewellery;
        lootStatInfos.Add(lootStatInfo);

        m_lootStatInfo = lootStatInfos.ToArray();
      }
      return base.GetLootStatInfo(caller);
    }

    public override bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var props = enhancmentProps[this.TinyTrophyKind];
      if (props.ContainsKey(eq.EquipmentKind))
      {
        var esk = props[eq.EquipmentKind];
        int val = GetStatIncrease(eq.EquipmentKind);
        return eq.Enchant(esk, val, this, out error);
      }

      return false;
    }

    public override void SetProps()
    {
      SetKind(this.TinyTrophyKind);//refresh
    }
  }
}
