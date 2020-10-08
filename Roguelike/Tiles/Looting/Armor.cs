using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Armor : Equipment
  {
    public const char ArmorSymbol = '[';
    public const char ShieldSymbol = ')';

    public Armor() : base(EquipmentKind.Armor)
    {
      Price = 10;
      SetPrimaryStat(EntityStatKind.Defense, 1);
    }

    public int Defence
    {
      get
      {
        return (int)PrimaryStatValue;
      }

      set
      {
        PrimaryStatValue = value;
      }
    }

    public override bool IsSameKind(Loot other)
    {
      return (other as Armor).EquipmentKind == this.EquipmentKind;
    }

    public override EquipmentKind EquipmentKind
    {
      get
      {
        return base.EquipmentKind;
      }

      set
      {
        switch (value)
        {
          case EquipmentKind.Shield:
            Symbol = ShieldSymbol;
            break;
          case EquipmentKind.Armor:
            Symbol = ArmorSymbol;
            break;
          case EquipmentKind.Helmet:
            Symbol = ArmorSymbol;
            break;
          case EquipmentKind.Glove:
            Symbol = ArmorSymbol;
            break;
          default:
            throw new Exception("EquipmentKind invalid!"); ;

        }
        base.EquipmentKind = value;
      }
    }
  }
}
