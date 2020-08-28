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
      LootKind = LootKind.Gem;
      Symbol = '*';
      Name = "Gem";
      GemKind = kind;
      EnchanterSize = EnchanterSize.Small;

      if(gameLevel>=0)
        SetRandomKindAndLevelSize(gameLevel, kind == GemKind.Unset);
      else
        SetProps();

      EnchantSrcFromGemKind();
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

    public void SetProps()
    {
      string gemName = GemKind.ToString();
      int price = 15;
      if(EnchanterSize == EnchanterSize.Medium)
        price *= 2;
      else if (EnchanterSize == EnchanterSize.Big)
        price *= 4;

      string enchanterSize = EnchanterSize.ToString();
      tag1 = gemName.ToLower() + "_" + enchanterSize.ToLower();
      Name = enchanterSize + " " + gemName;
      Price = price;
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
          int val = 0;
          if (eq.EquipmentKind == EquipmentKind.Amulet || eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
          {
            val = otherValues[this.EnchanterSize];
          }
          else
          {
            val = wpnAndArmorValues[this.EnchanterSize];
            if (prop == EntityStatKind.ResistCold || prop == EntityStatKind.ResistFire || prop == EntityStatKind.ResistPoison)
              val = GetResistValue();
          }
          if (values.Any(i => i != val))
            throw new Exception("Error on crafting");
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
          desc += "Drop it on the item in the Inventory.";
        else
          desc += "Use it with Custom Recipe on Crafting Panel.";


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

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        extraStatDescription = new string[3];
        string desc = "Weapons: " + enhancmentProps[this.GemKind][EquipmentKind.Weapon] + " " + wpnAndArmorValues[this.EnchanterSize];
        extraStatDescription[0] = desc;
        desc = "Armor: " + enhancmentProps[this.GemKind][EquipmentKind.Armor] + " " + GetResistValue() + "%";
        extraStatDescription[1] = desc;
        desc = "Jewellery: " + enhancmentProps[this.GemKind][EquipmentKind.Ring] + " " + otherValues[this.EnchanterSize];
        
        if (enhancmentProps[this.GemKind][EquipmentKind.Ring] == EntityStatKind.ChanceToHit)
          desc += "%";
        extraStatDescription[2] = desc;

      }
      return extraStatDescription;
    }
  }
}
