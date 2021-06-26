using Roguelike.Attributes;
using Roguelike.LootFactories;
using Roguelike.Tiles.Looting;

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

    public EntityStat SpecialFeature { get; set; }
    public EntityStat SpecialFeatureAux { get; set; }
    SpellSource spellSource;

    public Weapon()
    {
      this.EquipmentKind = EquipmentKind.Weapon;
      this.PrimaryStatKind = EntityStatKind.Attack;
      this.Price = 5;
    }

    public bool IsMagician
    {
      get
      {
        return Kind == WeaponKind.Scepter || Kind == Weapon.WeaponKind.Wand ||
                Kind == Weapon.WeaponKind.Staff;
      }
    }



    public WeaponKind kind;
    public WeaponKind Kind
    {
      get => kind;
      set
      {
        kind = value;
        if (IsMaterialAware())
        {
          this.collectedSound = "SWORD_Hit_Sword_RR9_mono";
          SetMaterial(EquipmentMaterial.Bronze);
        }
        else
          this.collectedSound = "none_steel_weapon_collected";

        var chanceForEffect = 10;
        switch (value)
        {
          case WeaponKind.Dagger:
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToCauseBleeding, chanceForEffect);
            break;
          case WeaponKind.Sword:
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToHit, chanceForEffect);
            break;
          case WeaponKind.Bashing:
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToCauseStunning, chanceForEffect);
            SpecialFeatureAux = new EntityStat(EntityStatKind.ChanceToHit, -chanceForEffect);

            break;
          case WeaponKind.Axe:
            //Symbol = AxeSymbol;
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToCauseTearApart, chanceForEffect);
            //Name = "Axe";
            break;
          case WeaponKind.Scepter:
          case WeaponKind.Staff:
          case WeaponKind.Wand:
            spellSource = new WeaponSpellSource(Spells.SpellKind.FireBall);
            break;
          default:
            break;
        }
      }
    }

    public override EntityStats GetStats()
    {
      EntityStats stats = base.GetStats();
      var wpn = this;
      if (wpn.SpecialFeature != null)
      {
        stats.Ensure(wpn.SpecialFeature.Kind);
        stats.Stats[wpn.SpecialFeature.Kind].Factor += wpn.SpecialFeature.Value.TotalValue;
      }
      if (wpn.SpecialFeatureAux != null)
      {
        stats.Ensure(wpn.SpecialFeatureAux.Kind);
        stats.Stats[wpn.SpecialFeatureAux.Kind].Factor += wpn.SpecialFeatureAux.Value.TotalValue;
      }
      return stats;
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
      var enh = 1;
      if (material == EquipmentMaterial.Iron)
        enh = MaterialProps.BronzeToIronMult;
      else if (material == EquipmentMaterial.Steel)
        enh = MaterialProps.BronzeToSteelMult;

      Damage *= enh;
    }

    public bool StableDamage { get; set; } = false;
    public SpellSource SpellSource { get => spellSource; set => spellSource = value; }

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
      //return "+["+min + "-" + max+"]";
      return min + "-" + max;
    }

    protected override void SetPrimaryStatDesc()
    {
      PrimaryStatDescription = PrimaryStatKind.ToString() + ": " + GetDamageDescription();
    }
  }
}
