using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Jewellery : Equipment
  {
    public const char AmulerSymbol = '"';
    public const char RingSymbol = '=';


    public override EquipmentKind EquipmentKind
    {
      get
      {
        return base.EquipmentKind;
      }

      set
      {
        base.EquipmentKind = value;
        var name = "";
        switch (EquipmentKind)
        {
          case EquipmentKind.Ring:
          //case EquipmentKind.RingRight:
            Symbol = RingSymbol;
            name = "Ring";
            includeTypeInToString = false;
            break;
          case EquipmentKind.Amulet:
            Symbol = AmulerSymbol;
            name = "Amulet";
            includeTypeInToString = false;
            break;
          default:
            break;
        }

        if (Name != name)//do not override real name!
          Name = name;
      }
    }

    public Jewellery() : base(EquipmentKind.Ring)
    {

    }
  }
}
