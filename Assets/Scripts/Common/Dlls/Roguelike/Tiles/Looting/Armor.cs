﻿using Roguelike.Attributes;
using System;

namespace Roguelike.Tiles.Looting
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

    public int Defense
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

    public override void SetLevelIndex(int levelIndex)
    {
      base.SetLevelIndex(levelIndex);
      SetRequiredStat(levelIndex, EntityStatKind.Strength);
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
