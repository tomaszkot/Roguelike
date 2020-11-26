using Dungeons.Core;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum GemKind { Unset, Ruby, Emerald, Diamond, Amber }
  

  public class Gem : Enchanter
  {
    public GemKind GemKind { get; set; }
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsRuby = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsEmer = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsDiam = new Dictionary<EquipmentKind, EntityStatKind>();
    
    static Dictionary<GemKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps = new Dictionary<GemKind, Dictionary<EquipmentKind, EntityStatKind>>();

    public Gem() : this(GemKind.Unset)
    { 
    }

    public Gem(int gameLevel) : this(GemKind.Unset, gameLevel)
    {
    }

    public Gem(GemKind kind = GemKind.Unset, int gameLevel = 0)
    {
      collectedSound = "gem_collected";
      LootKind = LootKind.Gem;
      Symbol = '*';
      Name = "Gem";
      GemKind = kind;
      EnchanterSize = EnchanterSize.Small;
      EnchantSrcFromGemKind();

      if (gameLevel>=0)
        SetRandomKindAndLevelSize(gameLevel, kind == GemKind.Unset);
      else
        SetProps();
    }

    public void EnchantSrcFromGemKind()
    {
      switch (GemKind)
      {
        case GemKind.Unset:
          EnchantSrc = EnchantSrc.Unset;
          break;
        case GemKind.Ruby:
          EnchantSrc = EnchantSrc.Ruby;
          break;
        case GemKind.Emerald:
          EnchantSrc = EnchantSrc.Emerald;
          break;
        case GemKind.Diamond:
          EnchantSrc = EnchantSrc.Diamond;
          break;
        case GemKind.Amber:
          EnchantSrc = EnchantSrc.Amber;
          //src = EnchantSrc.Amber;
          break;
        default:
          break;
      }
    }

    public override string GetId()
    {
      return base.GetId() + "_" + GemKind + "_" + EnchanterSize;
    }

    static Gem()
    {
      PopulateProps(enhancmentPropsRuby, EntityStatKind.ResistFire, EntityStatKind.FireAttack, EntityStatKind.Health);
      PopulateProps(enhancmentPropsEmer, EntityStatKind.ResistPoison, EntityStatKind.PoisonAttack, EntityStatKind.Mana);
      PopulateProps(enhancmentPropsDiam, EntityStatKind.ResistCold, EntityStatKind.ColdAttack, EntityStatKind.ChanceToHit);

      enhancmentProps[GemKind.Ruby] = enhancmentPropsRuby;
      enhancmentProps[GemKind.Emerald] = enhancmentPropsEmer;
      enhancmentProps[GemKind.Diamond] = enhancmentPropsDiam;
    }

    public void SetRandomKindAndLevelSize(int gameLevel, bool setKind)
    {
      if(setKind)
        GemKind = RandHelper.GetRandomEnumValue<GemKind>();
      EnchanterSize = EnchanterSize.Small;
      
      if (gameLevel >= 4)
      {
        EnchanterSize = EnchanterSize.Medium;
      }
      if (gameLevel >= 8)
      {
        EnchanterSize = EnchanterSize.Big;
      }

      SetProps();
    }

    public override void SetProps()
    {
      SetPrice();
      tag1 = CalcTagFrom();
      SetName(GemKind.ToDescription());
    }

    public string CalcTagFrom()
    {
      return CalcTagFrom(GemKind, EnchanterSize);
    }

    public static string CalcTagFrom(GemKind kind, EnchanterSize size)
    {
      return kind.ToString().ToLower() + "_" + size.ToString().ToLower();
    }

    int resistMult = 3;

    public override bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var gemKind = this.GemKind;
      if (gemKind == GemKind.Amber)
        gemKind = GemKind.Diamond;
      var props = enhancmentProps[gemKind];
      if (props.ContainsKey(eq.EquipmentKind))
      {
        var propsGem = props[eq.EquipmentKind];
        var propsToSet = new[] { propsGem }.ToList();
        if (this.GemKind == GemKind.Amber)
        {
          var otherKinds = GetOtherKinds(gemKind);
          foreach (var other in otherKinds)
            propsToSet.Add(enhancmentProps[other][eq.EquipmentKind]);
        }

        var values = new  List<int>();
        foreach (var prop in propsToSet)
        {
          int val = GetStatIncrease(eq.EquipmentKind, prop);
                   
          //if (values.Any(i => i != val)) ??
          //  throw new Exception("Error on crafting");
          values.Add(val);
        }

        var res = eq.Enchant(propsToSet.ToArray(), values.First(), this, out error);
        if (!res)
        {
          return false;
        }
        return true;
      }

      return false;
    }

    private GemKind[] GetOtherKinds(GemKind kind)
    {
      var skip = new[] { GemKind.Amber, kind, GemKind.Unset };
      var values = Enum.GetValues(typeof(GemKind)).Cast<GemKind>().Where(i => !skip.Contains(i)).ToList();
      return values.ToArray();
    }

    public override string PrimaryStatDescription
    {
      get
      {
        string desc = "Enchants equipment. ";
        var allowInPlaceInventoryCrafting = true;
        if (allowInPlaceInventoryCrafting)
          desc += Strings.DropOnEnchantable; 
        else
          desc += "Use it with the Enchant Equipment recipe on the Crafting Panel.";


        return desc;
      }
    }

    int GetResistValue()
    {
      var res = (wpnAndArmorValues[this.EnchanterSize]) * resistMult;
      if (this.EnchanterSize == EnchanterSize.Small)
        res += 1;
      else if (this.EnchanterSize == EnchanterSize.Medium)
        res += 2;
      else if (this.EnchanterSize == EnchanterSize.Big)
        res += 4;
      return res;
    }
    
    LootStatInfo AddLootStatInfo(List<LootStatInfo>  list, LootStatKind lsk)
    {
      var lootStatInfo = new LootStatInfo();
      lootStatInfo.Kind = lsk;
      list.Add(lootStatInfo);
      return lootStatInfo;
    }

    public override int GetStatIncrease(EquipmentKind ek, EntityStatKind esk)
    {
      var val = base.GetStatIncrease(ek);
      if (esk == EntityStatKind.ResistCold || esk == EntityStatKind.ResistFire || esk == EntityStatKind.ResistPoison)
        val = GetResistValue();
      return val;
    }

    public override LootStatInfo[] GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null)
      {
        var lootStatsInfo = new List<LootStatInfo>();

        var gemKind = this.GemKind;
        if (this.GemKind == GemKind.Amber)
        {
          gemKind = GemKind.Diamond;
        }
        var gemKindInfo = enhancmentProps[gemKind];

        var lootStatInfo = AddLootStatInfo(lootStatsInfo, LootStatKind.Weapon);
        lootStatInfo.Desc = "Weapons: ";
        if (this.GemKind == GemKind.Amber)
          lootStatInfo.Desc += "all elemental attacks";
        else
          lootStatInfo.Desc += gemKindInfo[EquipmentKind.Weapon].ToDescription();
                
        lootStatInfo.Desc += " +" + wpnAndArmorValues[this.EnchanterSize];

        //
        lootStatInfo = AddLootStatInfo(lootStatsInfo, LootStatKind.Armor);
        lootStatInfo.Desc = "Armor: ";
        if (this.GemKind == GemKind.Amber)
          lootStatInfo.Desc += "all elemental resists";
        else
          lootStatInfo.Desc += gemKindInfo[EquipmentKind.Armor].ToDescription();

        lootStatInfo.Desc += " +" + GetResistValue() + "%";

        //
        lootStatInfo = AddLootStatInfo(lootStatsInfo, LootStatKind.Jewellery);
        lootStatInfo.Desc = "Jewellery: ";
        lootStatInfo.Desc += gemKindInfo[EquipmentKind.Ring].ToDescription();
        if (this.GemKind == GemKind.Amber)
        {
          var otherKinds = GetOtherKinds(gemKind);
          foreach (var otherKind in otherKinds)
          {
            var gemKindInfo_ = enhancmentProps[otherKind];
            lootStatInfo.Desc += ", " + gemKindInfo_[EquipmentKind.Ring].ToDescription();
          }
        }
        else
        { 
        }

        lootStatInfo.Desc += " +" + otherValues[this.EnchanterSize];

        //if (this.GemKind != GemKind.Amber)
        //{
        //  if (gemKindInfo[EquipmentKind.Ring] == EntityStatKind.ChanceToHit)
        //    lootStatInfo.Desc += "%";
        //}

        m_lootStatInfo = lootStatsInfo.ToArray();

      }
      return m_lootStatInfo;
    }

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        var destItems = new List<string>();
        var items = GetLootStatInfo(null);
        foreach (var item in items)
          destItems.Add(item.Desc);

        extraStatDescription = destItems.ToArray();
      }
      return extraStatDescription;
    }
  }
}
