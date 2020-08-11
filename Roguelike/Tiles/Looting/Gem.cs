using Dungeons.Core;
using Roguelike.Attributes;
using System.Collections.Generic;


namespace Roguelike.Tiles.Looting
{
  public enum GemKind { Ruby, Emerald, Diamond, Amber }
  public enum GemSize { Big, Medium, Small }

  public class Gem : StackedLoot
  {
    public GemKind GemKindValue;
    public GemSize GemSizeValue;
    static Dictionary<GemSize, int> wpnAndArmorValues = new Dictionary<GemSize, int>();
    static Dictionary<GemSize, int> otherValues = new Dictionary<GemSize, int>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsRuby = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsEmer = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<EquipmentKind, EntityStatKind> enhancmentPropsDiam = new Dictionary<EquipmentKind, EntityStatKind>();
    static Dictionary<GemKind, Dictionary<EquipmentKind, EntityStatKind>> enhancmentProps = new Dictionary<GemKind, Dictionary<EquipmentKind, EntityStatKind>>();

    public Gem(int gameLevel) : this(null, gameLevel)
    {
    }

    public Gem() : this(null, 0)
    {
    }

    public Gem(GemKind? kind, int gameLevel = -1)
    {
      Symbol = '*';
      Name = "Gem";
      if(kind.HasValue)
        GemKindValue = kind.Value;
      GemSizeValue = GemSize.Small;

      if(gameLevel>=0)
        SetRandomKindAndLevelSize(gameLevel, !kind.HasValue);
      else
        SetProps();
    }

    public static EnchantSrc EnchantSrcFromGemKind(GemKind gk)
    {
      EnchantSrc src = EnchantSrc.Unset;
      switch (gk)
      {
        case GemKind.Ruby:
          src = EnchantSrc.Ruby;
          break;
        case GemKind.Emerald:
          src = EnchantSrc.Emerald;
          break;
        case GemKind.Diamond:
          src = EnchantSrc.Diamond;
          break;
        case GemKind.Amber:
          src = EnchantSrc.Amber;
          break;
        default:
          break;
      }

      return src;
    }

    public override string GetId()
    {
      return base.GetId() + "_" + GemKindValue + "_" + GemSizeValue;
    }

    static Gem()
    {
      wpnAndArmorValues[GemSize.Big] = 6;
      wpnAndArmorValues[GemSize.Medium] = 4;
      wpnAndArmorValues[GemSize.Small] = 2;

      otherValues[GemSize.Big] = 15;
      otherValues[GemSize.Medium] = 10;
      otherValues[GemSize.Small] = 5;
      
      PopulateProps(enhancmentPropsRuby, EntityStatKind.ResistFire, EntityStatKind.FireAttack, EntityStatKind.Health);
      PopulateProps(enhancmentPropsEmer, EntityStatKind.ResistPoison, EntityStatKind.PoisonAttack, EntityStatKind.Mana);
      PopulateProps(enhancmentPropsDiam, EntityStatKind.ResistCold, EntityStatKind.ColdAttack, EntityStatKind.ChanceToHit);

      enhancmentProps[GemKind.Ruby] = enhancmentPropsRuby;
      enhancmentProps[GemKind.Emerald] = enhancmentPropsEmer;
      enhancmentProps[GemKind.Diamond] = enhancmentPropsDiam;
    }

    private static void PopulateProps(Dictionary<EquipmentKind, EntityStatKind> enhancmentProps, EntityStatKind arm, EntityStatKind wpn, EntityStatKind Juw)
    {
      enhancmentProps[EquipmentKind.Armor] = arm;
      enhancmentProps[EquipmentKind.Helmet] = enhancmentProps[EquipmentKind.Armor];
      enhancmentProps[EquipmentKind.Shield] = enhancmentProps[EquipmentKind.Armor];

      enhancmentProps[EquipmentKind.Amulet] = Juw;
      enhancmentProps[EquipmentKind.Ring] = Juw;
      enhancmentProps[EquipmentKind.Trophy] = Juw;

      enhancmentProps[EquipmentKind.Weapon] = wpn;
    }

    public void SetRandomKindAndLevelSize(int gameLevel, bool setKind)
    {
      if(setKind)
        GemKindValue = RandHelper.GetRandomEnumValue<GemKind>();
      GemSizeValue = GemSize.Small;
      
      if (gameLevel >= 4)
      {
        GemSizeValue = GemSize.Medium;
       
      }
      if (gameLevel >= 8)
      {
        GemSizeValue = GemSize.Big;
        
      }

      SetProps();
    }

    public void SetProps()
    {
      string gemSize = GemSizeValue.ToString();
      string gemName = GemKindValue.ToString();
      int price = 15;
      if(GemSizeValue == GemSize.Medium)
        price *= 2;
      else if (GemSizeValue == GemSize.Big)
        price *= 4;

      tag1 = gemName.ToLower() + "_" + gemSize.ToLower();
      Name = gemSize + " " + gemName;
      Price = price;
    }

    int resistMult = 3;

    public bool ApplyTo(Equipment eq, out string error)
    {
      error = "";
      var props = enhancmentProps[this.GemKindValue];
      if (props.ContainsKey(eq.EquipmentKind))//IsCraftableWith(eq))
      {
        var propsGem = props[eq.EquipmentKind];
        int val = 0;
        if (eq.EquipmentKind == EquipmentKind.Amulet || eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        {
          val = otherValues[this.GemSizeValue];
        }
        else
        {
          val = wpnAndArmorValues[this.GemSizeValue];
          if (propsGem == EntityStatKind.ResistCold || propsGem == EntityStatKind.ResistFire || propsGem == EntityStatKind.ResistPoison)
            val = GetResistValue();
        }
        return eq.Enchant(propsGem, val, EnchantSrcFromGemKind(GemKindValue), out error);
      }

      return false;
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
      var res = (wpnAndArmorValues[this.GemSizeValue]) * resistMult;
      if (this.GemSizeValue == GemSize.Small)
        res += 1;
      else if (this.GemSizeValue == GemSize.Medium)
        res += 2;
      else if (this.GemSizeValue == GemSize.Big)
        res += 4;
      return res;
    }

    public override string[] GetExtraStatDescription()
    {
      if (extraStatDescription == null)
      {
        extraStatDescription = new string[3];
        string desc = "Weapons: " + enhancmentProps[this.GemKindValue][EquipmentKind.Weapon] + " " + wpnAndArmorValues[this.GemSizeValue];
        extraStatDescription[0] = desc;
        desc = "Armor: " + enhancmentProps[this.GemKindValue][EquipmentKind.Armor] + " " + GetResistValue() + "%";
        extraStatDescription[1] = desc;
        desc = "Jewellery: " + enhancmentProps[this.GemKindValue][EquipmentKind.Ring] + " " + otherValues[this.GemSizeValue];
        
        if (enhancmentProps[this.GemKindValue][EquipmentKind.Ring] == EntityStatKind.ChanceToHit)
          desc += "%";
        extraStatDescription[2] = desc;

      }
      return extraStatDescription;
    }
  }
}
