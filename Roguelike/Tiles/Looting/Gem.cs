using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Settings;
using Roguelike.Tiles.LivingEntities;
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
    public int GameLevel { get; set; } = 1;

    public Gem() : this(GemKind.Unset)
    {
    }

    public Gem(int gameLevel) : this(GemKind.Unset, gameLevel)
    {
    }

    public Gem(GemKind kind = GemKind.Unset, int gameLevel = 0)
    {
      Damaged = true;

      collectedSound = "gem_collected";
      LootKind = LootKind.Gem;
      Symbol = '*';
      Name = "Gem";
      GemKind = kind;
      EnchanterSize = EnchanterSize.Small;
      EnchantSrcFromGemKind();

      if (gameLevel >= 0)
        SetRandomKindAndLevelSize(gameLevel, kind == GemKind.Unset);
      else
        SetProps();
            
      //PrimaryStatDescription = desc;
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
      GameLevel = gameLevel;

      if (setKind)
        GemKind = RandHelper.GetRandomEnumValue<GemKind>();
      EnchanterSize = EnchanterSize.Small;
      var smaller = RandHelper.GetRandomDouble() < 0.5f;

      if (gameLevel >= 10)
      {
        EnchanterSize = EnchanterSize.Big;
      }
      else if (gameLevel >= 8)
      {
        EnchanterSize = smaller ? EnchanterSize.Medium : EnchanterSize.Big;
      }
      else if (gameLevel >= 6)
      {
        EnchanterSize = smaller ? EnchanterSize.Small : EnchanterSize.Medium;
      }
      else if (gameLevel >= 4)
      {
        smaller = false;
      }

      if (!smaller && gameLevel < 10)
        Damaged = true;

      if (RandHelper.GetRandomDouble() < 0.3f)
        Damaged = true;

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
        var esk = props[eq.EquipmentKind];
        var propsToSet = new[] { esk }.ToList();
        if (this.GemKind == GemKind.Amber)
        {
          var otherKinds = GetOtherKinds(gemKind);
          foreach (var other in otherKinds)
            propsToSet.Add(enhancmentProps[other][eq.EquipmentKind]);
        }

        var values = new List<int>();
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

    public override int GetStatIncrease(EquipmentKind ek, EntityStatKind esk)
    {
      var val = base.GetStatIncrease(ek);
      if (esk == EntityStatKind.ResistCold || esk == EntityStatKind.ResistFire || esk == EntityStatKind.ResistPoison ||
        esk == EntityStatKind.ResistLighting)
      {
        if (this.EnchanterSize == EnchanterSize.Small)
          val += 1;
        else if (this.EnchanterSize == EnchanterSize.Medium)
          val += 2;
        else if (this.EnchanterSize == EnchanterSize.Big)
          val += 4;

        val *= resistMult;
      }

      if (Damaged)
        val = val / 2;
      return val;
    }

    LootStatInfo GetLootStatInfo(LootStatKind lsk, EquipmentKind ek, Dictionary<EquipmentKind, EntityStatKind> gemKindInfo)
    {
      var lootStatInfo = new LootStatInfo();
      lootStatInfo.Kind = lsk;
      var post = "";
      var desc = "Weapons: ";
      if (lsk == LootStatKind.Armor)
      {
        desc = "Armor: ";
        post += "%";
      }
      else if (lsk == LootStatKind.Jewellery)
        desc = "Jewellery: ";

      lootStatInfo.Desc = desc;
      if (this.GemKind == GemKind.Amber)
      {
        if (lsk == LootStatKind.Weapon)
          lootStatInfo.Desc += "all elemental attacks";
        else if (lsk == LootStatKind.Armor)
          lootStatInfo.Desc += "all elemental resists";
        else
        {
          var gemKind = GemKind.Diamond;//read props from any 
          var otherKinds = GetOtherKinds(gemKind);

          string other = "";
          foreach (var otherKind in otherKinds)
          {
            var gemKindInfo_ = enhancmentProps[otherKind];
            if (other.Any())
              lootStatInfo.Desc += ", ";

            var otDesc = gemKindInfo_[EquipmentKind.Ring].ToDescription();
            lootStatInfo.Desc += otDesc;
            other += otDesc;
          }
        }
      }
      else
        lootStatInfo.Desc += gemKindInfo[ek].ToDescription();

      lootStatInfo.Desc += " +" + GetStatIncrease(ek, gemKindInfo[ek]);

      lootStatInfo.Desc += post;

      return lootStatInfo;
    }

    public override LootStatInfo[] GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null)
      {
        var lootStatsInfo = new List<LootStatInfo>();

        var gemKindInfo = GetCalcKindInfo();

        var lootStatInfo = GetLootStatInfo(LootStatKind.Weapon, EquipmentKind.Weapon, gemKindInfo);
        lootStatsInfo.Add(lootStatInfo);

        lootStatInfo = GetLootStatInfo(LootStatKind.Armor, EquipmentKind.Armor, gemKindInfo);
        lootStatsInfo.Add(lootStatInfo);

        lootStatInfo = GetLootStatInfo(LootStatKind.Jewellery, EquipmentKind.Ring, gemKindInfo);
        lootStatsInfo.Add(lootStatInfo);

        m_lootStatInfo = lootStatsInfo.ToArray();

      }
      return m_lootStatInfo;
    }

    private Dictionary<EquipmentKind, EntityStatKind> GetCalcKindInfo()
    {
      var gemKind = this.GemKind;
      if (this.GemKind == GemKind.Amber)
        gemKind = GemKind.Diamond;//read props from any 

      var gemKindInfo = enhancmentProps[gemKind];
      return gemKindInfo;
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
