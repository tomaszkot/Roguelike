﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Jewellery : Equipment
  {
    public const char AmuletSymbol = '"';
    public const char RingSymbol = '=';
    public const char PendantSymbol = AmuletSymbol;

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
            //IncludeDebugDetailsInToString = false;
            break;
          case EquipmentKind.Amulet:
            Symbol = AmuletSymbol;
            name = IsPendant ? "Pendant" : "Amulet";
            //IncludeDebugDetailsInToString = false;
            break;
          default:
            break;
        }

        if (Name != name)//do not override real name!
          Name = name;
      }
    }

    bool isPendant;
    public bool IsPendant 
    {
      get { return isPendant; }
      set 
      {
        if (EquipmentKind == EquipmentKind.Amulet)
        {
          isPendant = value;
          if (isPendant)
          {
            tag1 = "pendant";
            Name = "Pendant";
            MakeEnchantable();
          }
        }
        else
          Debug.Assert(false);
      } 
    }

    public Jewellery() : base(EquipmentKind.Ring)
    {

    }
  }
}
