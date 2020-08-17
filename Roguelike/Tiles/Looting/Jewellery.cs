﻿using System;
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
    public const char PendantSymbol = AmulerSymbol;

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
            Symbol = RingSymbol;
            name = "Ring";
            includeTypeInToString = false;
            break;
          case EquipmentKind.Amulet:
            Symbol = AmulerSymbol;
            name = IsPendant ? "Pendant" : "Amulet";
            includeTypeInToString = false;
            break;
          default:
            break;
        }

        if (Name != name)//do not override real name!
          Name = name;
      }
    }

    public bool IsPendant { get; set; }

    public Jewellery() : base(EquipmentKind.Ring)
    {

    }
  }
}
