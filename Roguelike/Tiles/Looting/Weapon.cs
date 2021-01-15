using Roguelike.Attributes;
using Roguelike.LootFactories;

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
        if (IsMaterialAware())
        {
          this.collectedSound = "SWORD_Hit_Sword_RR9_mono";
          SetMaterial(EquipmentMaterial.Bronze);
        }
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

    protected override void EnhanceStatsDueToMaterial(EquipmentMaterial material)
    {
      if (Material != material)//shall be already set
        return;
      var enh = 0;
      if (material == EquipmentMaterial.Iron)
        enh = MaterialProps.BronzeToIronMult;
      else if (material == EquipmentMaterial.Steel)
        enh = MaterialProps.BronzeToSteelMult;

      Damage *= enh;
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
      PrimaryStatDescription = PrimaryStatKind.ToString() + ": " + GetDamageDescription();
    }
  }
}
