using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum HunterTrophyKind { Unset, Fang, /*Tusk,*/ Claw }//Fang-Tooth

  public class HunterTrophy : Enchanter
  {
    HunterTrophyKind tinyTrophyKind;
    public HunterTrophyKind TinyTrophyKind
    {
      get => tinyTrophyKind;
      set {
        tinyTrophyKind = value;
        SetProps();
      }
    }

    static Dictionary<HunterTrophyKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps =
      new Dictionary<HunterTrophyKind, Dictionary<EquipmentKind, EntityStatKind>>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsFang = new Dictionary<EquipmentKind, EntityStatKind>();
    //static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsTusk = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsClaw = new Dictionary<EquipmentKind, EntityStatKind>();

    public HunterTrophy():this(HunterTrophyKind.Claw)
    { 
    
    }

    public HunterTrophy(HunterTrophyKind kind)
    {
      Price = 5;
      Symbol = '&';
      LootKind = LootKind.HunterTrophy;
      this.TinyTrophyKind = kind;
      SetProps();
    }

    public static string[] TinyTrophiesTags = new[]
    {
      Enchanter.Big+ "_claw", Enchanter.Big + "_fang",//, Enchanter.Big + "_tusk",
      Enchanter.Medium + "_claw", Enchanter.Medium+"_fang",//, Enchanter.Medium+ "_tusk",
      Enchanter.Small+"_claw", Enchanter.Small+"_fang",//, Enchanter.Small+"_tusk"
    };

    static HunterTrophy()
    {
      PopulateProps(enhancmentPropsFang, EntityStatKind.Defense, EntityStatKind.ChanceToBulkAttack, EntityStatKind.ChanceToEvadeMeleeAttack);
      PopulateProps(enhancmentPropsClaw, EntityStatKind.Health, EntityStatKind.ChanceToCauseBleeding, EntityStatKind.Strength);
      //PopulateProps(enhancmentPropsTusk, EntityStatKind.ChanceToMeleeHit, EntityStatKind.ChanceToStrikeBack, EntityStatKind.Dexterity);

      enhancmentProps[HunterTrophyKind.Fang] = enhancmentPropsFang;
      //enhancmentProps[HunterTrophyKind.Tusk] = enhancmentPropsTusk;
      enhancmentProps[HunterTrophyKind.Claw] = enhancmentPropsClaw;
    }

    public override string GetId()
    {
      return base.GetId() + TinyTrophyKind + " " + EnchanterSize;
    }

    

    private void SetKind(HunterTrophyKind kind)
    {
      tinyTrophyKind = kind;
      SetName(kind.ToDescription());

      switch (kind)
      {
        case HunterTrophyKind.Unset:
          EnchantSrc = EnchantSrc.Unset;
          break;
        case HunterTrophyKind.Fang:

          EnchantSrc = EnchantSrc.Fang;
          PrimaryStatDescription = "Sharp, hard, ready to bite. ";
          break;
        //case HunterTrophyKind.Tusk:
        //  EnchantSrc = EnchantSrc.Tusk;
        //  PrimaryStatDescription = "Big, sharp, ready to tear somebody apart. ";
        //  break;
        case HunterTrophyKind.Claw:
          EnchantSrc = EnchantSrc.Claw;
          PrimaryStatDescription = "Sharp, hard, ready to claw. ";
          break;
        default:
          break;
      }
    }


    public override List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null || !m_lootStatInfo.Any())
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

        m_lootStatInfo = lootStatInfos;
      }
      return base.GetLootStatInfo(caller);
    }

    public override bool ApplyTo(Equipment eq, Func<EquipmentKind> ekProvider, out string error)
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
      base.SetPropsCommon();
    }

    protected override string CalcTagFromProps()
    {
      var tags = TinyTrophiesTags
        .Where(i => i.Contains(TinyTrophyKind.ToString().ToLower()) && i.Contains(EnchanterSize.ToString().ToLower()))
        .SingleOrDefault();
      return tags??"";
    }
  }
}
