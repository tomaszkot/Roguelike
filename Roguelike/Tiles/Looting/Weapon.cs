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
    
    const float DamageVariation = .15f;
    public float GetPrimaryDamageVariation()
    {
      if (Damage == 0)
        return 0;
      if (Damage < 10)
        return 1;
      if (Damage < 20)
        return 2;
      return (int)(PrimaryStatValue * DamageVariation);
    }

    public string GetDamageDescription()
    {
      float min = PrimaryStatValue - GetPrimaryDamageVariation();
      float max = PrimaryStatValue + GetPrimaryDamageVariation();
      return min + "-" + max;
    }

    protected override void SetPrimaryStatDesc()
    {
      primaryStatDesc = PrimaryStatKind.ToString() + ": " + GetDamageDescription();
    }
  }
}
