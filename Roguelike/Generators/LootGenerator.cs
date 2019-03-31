﻿using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Generators
{
  public class LootGenerator
  {
    public virtual Weapon GetRandomWeapon()
    {
      var wpn = new Weapon();
      wpn.Name = "Sword";
      wpn.Kind = Weapon.WeaponKind.Sword;
      wpn.EquipmentKind = EquipmentKind.Weapon;
      wpn.PrimaryStat = EntityStatKind.Attack;
      wpn.PrimaryStatValue = 5;
      return wpn;
    }

    public virtual Loot GetRandomLoot()
    {
      var loot = new Loot();
      loot.tag = "mash3";
      return loot;
    }
  }
}
