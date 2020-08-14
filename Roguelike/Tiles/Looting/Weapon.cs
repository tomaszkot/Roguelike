using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Weapon : Equipment
  {
    public enum WeaponKind
    {
      Unset, Dagger, Sword, Axe, Bashing, Scepter, Wand, Staff,
      Other
      //,Bow
    }

    public Weapon()
    {
      this.EquipmentKind = EquipmentKind.Weapon;
      this.PrimaryStatKind = EntityStatKind.Attack;
      this.Price = 5;
    }

    public bool IsMagician()
    {
      return Kind == Roguelike.Tiles.Weapon.WeaponKind.Scepter || Kind == Roguelike.Tiles.Weapon.WeaponKind.Wand ||
        Kind == Roguelike.Tiles.Weapon.WeaponKind.Staff;
    }

    public WeaponKind Kind { get; set; }
   // public int MinDropDungeonLevel { get; set; }

    public int Damage
    {
      get { return (int)PrimaryStatValue; }

      set
      {
        PrimaryStatValue = value;
      }
    }
  }
}
