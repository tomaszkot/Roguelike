using Roguelike.Attributes;
using Roguelike.Tiles;
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
      wpn.PrimaryStatKind = EntityStatKind.Attack;
      wpn.PrimaryStatValue = 5;
      return wpn;
    }

    public virtual Loot GetRandomLoot()
    {
      var loot = new Mushroom();
      loot.SetKind(MushroomKind.Boletus);
      loot.tag = "mash3";
      return loot;
    }
  }
}
