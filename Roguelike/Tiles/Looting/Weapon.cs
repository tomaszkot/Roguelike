﻿using Roguelike.Attributes;

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
      return Kind == WeaponKind.Scepter || Kind == Weapon.WeaponKind.Wand ||
        Kind == Weapon.WeaponKind.Staff;
    }

    public WeaponKind kind;
    public WeaponKind Kind
    {
      get => kind;
      set {
        kind = value;
        if(kind == WeaponKind.Dagger || kind == WeaponKind.Sword || kind == WeaponKind.Axe ||
          kind == WeaponKind.Scepter)
          this.collectedSound = "SWORD_Hit_Sword_RR9_mono";
        else
          this.collectedSound = "none_steel_weapon_collected";
      } 
    }
   // public int MinDropDungeonLevel { get; set; }

    public int Damage
    {
      get { return (int)PrimaryStatValue; }

      set
      {
        PrimaryStatValue = value;
      }
    }

    public bool StableDamage { get; set; } = false;
               
    public float GetPrimaryDamageVariation()
    {
      if (StableDamage)
        return 0;
      if (Damage == 0)
        return 0;
      if (Damage < 10)
        return 1;
      if (Damage < 20)
        return 2;
      return (int)(PrimaryStatValue * .15f);
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
