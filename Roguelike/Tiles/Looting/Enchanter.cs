using Roguelike.Attributes;
using System.Collections.Generic;

namespace Roguelike.Tiles.Looting
{
  public enum EnchantSrc { Unset, Ruby, Emerald, Diamond, Amber, Fang, Tusk, Claw }
  public enum EnchanterSize { Small, Medium, Big }

  public abstract class Enchanter : StackedLoot
  {
    public static string Small = "small";
    public static string Medium = "medium";
    public static string Big = "big";
    public bool Damaged { get; set; }

    EnchanterSize enchanterSize = EnchanterSize.Small;
    public EnchanterSize EnchanterSize
    {
      get { return enchanterSize; }
      set
      {
        enchanterSize = value;
        SetProps();
      }
    }

    public EnchantSrc EnchantSrc { get; set; }
    //public EnchanterKind EnchanterKind { get; set; }

    static Dictionary<EnchanterSize, int> wpnAndArmorEqKindValues = new Dictionary<EnchanterSize, int>();
    static Dictionary<EnchanterSize, int> otherEqKindValues = new Dictionary<EnchanterSize, int>();

    static Enchanter()
    {
      wpnAndArmorEqKindValues[EnchanterSize.Big] = 6;
      wpnAndArmorEqKindValues[EnchanterSize.Medium] = 4;
      wpnAndArmorEqKindValues[EnchanterSize.Small] = 2;

      otherEqKindValues[EnchanterSize.Big] = 15;
      otherEqKindValues[EnchanterSize.Medium] = 10;
      otherEqKindValues[EnchanterSize.Small] = 5;
    }

    protected static void PopulateProps
    (
      Dictionary<EquipmentKind, EntityStatKind> enhancmentProps,
      EntityStatKind arm,
      EntityStatKind wpn,
      EntityStatKind juw
    )
    {
      enhancmentProps[EquipmentKind.Armor] = arm;
      enhancmentProps[EquipmentKind.Helmet] = arm;
      enhancmentProps[EquipmentKind.Shield] = arm;
      enhancmentProps[EquipmentKind.Glove] = arm;

      enhancmentProps[EquipmentKind.Amulet] = juw;
      enhancmentProps[EquipmentKind.Ring] = juw;
      enhancmentProps[EquipmentKind.Trophy] = juw;

      enhancmentProps[EquipmentKind.Weapon] = wpn;
    }


    public virtual int GetStatIncrease(EquipmentKind ek, EntityStatKind esk = EntityStatKind.Unset)
    {
      int val = 0;
      if (ek == EquipmentKind.Amulet || ek == EquipmentKind.Ring || ek == EquipmentKind.Trophy)
      {
        val = otherEqKindValues[this.EnchanterSize];
      }
      else if (ek != EquipmentKind.Unset)
      {
        val = wpnAndArmorEqKindValues[this.EnchanterSize];
      }
      return val;
    }

    public abstract void SetProps();

    public abstract bool ApplyTo(Equipment eq, out string error);

    const int baseEnchPrice = 15;
    protected void SetPrice()
    {
      var price = baseEnchPrice;
      if (EnchanterSize == EnchanterSize.Medium)
        price *= 2;
      else if (EnchanterSize == EnchanterSize.Big)
        price *= 4;

      Price = price;
    }

    protected void SetName(string typeName)
    {
      Name = EnchanterSize + " " + typeName;
    }
  }
}
