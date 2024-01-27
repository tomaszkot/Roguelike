using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.LootFactories;
using Roguelike.Tiles.Looting;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public class Weapon : Equipment
  {
    public enum WeaponKind
    {
      Unset = 0, 
      Dagger = 1, 
      Sword =2, 
      Axe = 3, 
      Bashing = 4, 
      Scepter = 5, 
      Wand = 6, 
      Staff = 7,
      Crossbow = 8,
      Bow = 9,
      Other = 50
      //,Bow
    }

    public const int WandChargesCount = 20;
    public const int ScepterChargesCount = 30;
    public const int StaffChargesCount = 40;

    public static Dictionary<WeaponKind, EntityStat> RequiredStartStats = new Dictionary<WeaponKind, EntityStat>() 
    {
      { WeaponKind.Wand, new EntityStat(EntityStatKind.Magic, 10)},
      { WeaponKind.Scepter, new EntityStat(EntityStatKind.Magic, 15)},
      { WeaponKind.Staff, new EntityStat(EntityStatKind.Magic, 20)}
    };

    public EntityStat SpecialFeature { get; set; }
    public EntityStat SpecialFeatureAux { get; set; }
    SpellSource spellSource;

    public Weapon()
    {
      this.EquipmentKind = EquipmentKind.Weapon;
      this.PrimaryStatKind = EntityStatKind.MeleeAttack;
      this.Price = 5;
    }

    static Weapon()
    {
      var kinds = BowAmmoKinds.ToList();
      kinds.AddRange(CrossBowAmmoKinds);
      AllBowLikeAmmoKinds = kinds.ToArray();
  }

    public bool IsMagician
    {
      get
      {
        return Kind == WeaponKind.Scepter || Kind == Weapon.WeaponKind.Wand ||
                Kind == Weapon.WeaponKind.Staff;
      }
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if (base.IsMatchingRecipe(kind))
        return true;
      if (kind == RecipeKind.RechargeMagicalWeapon && IsMagician)
        return true;
      return false;
    }

    public void SetInitChargesCount(int mult)
    {
      if (SpellSource == null)
        return;
      var wss = (SpellSource as WeaponSpellSource);
      switch (Kind)
      {
        case WeaponKind.Unset:
          break;
        case WeaponKind.Dagger:
          break;
        case WeaponKind.Sword:
          break;
        case WeaponKind.Axe:
          break;
        case WeaponKind.Bashing:
          break;
        case WeaponKind.Scepter:
          wss.InitChargesCount = ScepterChargesCount * mult;
          break;
        case WeaponKind.Wand:
          wss.InitChargesCount = WandChargesCount * mult;
          break;
        case WeaponKind.Staff:
          wss.InitChargesCount = StaffChargesCount*mult;
          break;
        case WeaponKind.Other:
          break;
        default:
          break;
      }
    }

    static readonly Dictionary<WeaponKind, int> baseDamages = new Dictionary<WeaponKind, int>()
    {
      { WeaponKind.Axe, MaterialProps.BronzeAxeBaseDamage },
      { WeaponKind.Bashing, MaterialProps.BashingWeaponBaseDamage },
      { WeaponKind.Bow, Props.BowBaseDamage },
      { WeaponKind.Crossbow, Props.CrossbowBaseDamage },
      { WeaponKind.Dagger, MaterialProps.BronzeDaggerBaseDamage },
      { WeaponKind.Scepter, Props.ScepterBaseDamage },
      { WeaponKind.Staff, Props.StaffBaseDamage },
      { WeaponKind.Sword, MaterialProps.BronzeSwordBaseDamage },
      { WeaponKind.Wand, Props.WandBaseDamage }
    };

    public override void SetLevelIndex(int li)
    {
      base.SetLevelIndex(li);
      if (IsMagician)
        SetRequiredStat(li, EntityStatKind.Magic);
      else
        SetRequiredStat(li);

      if (Class != EquipmentClass.Unique)
      {
        if (baseDamages.ContainsKey(Kind))
        {
          CalcDamageFromLevel(baseDamages[Kind]);
        }
        else
        {
          if (!this.Name.Contains("Hound"))
            System.Diagnostics.Debug.WriteLine("!baseDamages.ContainsKey(Kind) " + this);
        }
      }
    }

    private void SetRequiredStat(int levelIndex)
    {
      EntityStatKind esk = EntityStatKind.Unset;
      switch (Kind)
      {
        case WeaponKind.Unset:
          break;
        case WeaponKind.Dagger:
          esk = EntityStatKind.Dexterity;
          break;
        case WeaponKind.Sword:
        case WeaponKind.Axe:
          esk = EntityStatKind.Strength;
          break;
        case WeaponKind.Bashing:
          esk = EntityStatKind.Strength;
          break;
        case WeaponKind.Scepter:
          break;
        case WeaponKind.Wand:
          break;
        case WeaponKind.Staff:
          break;
        case WeaponKind.Bow:
        case WeaponKind.Crossbow:
          esk = EntityStatKind.Dexterity;
          break;
        case WeaponKind.Other:
          break;
        default:
          break;
      }
      if (esk != EntityStatKind.Unset)
        SetRequiredStat(levelIndex, esk);
    }

    public void UpdateMagicWeaponDesc()
    {
      var wss = (SpellSource as WeaponSpellSource);
      PrimaryStatDescription = "Emits " + SpellSource.Kind + " charges\r\n(" + wss.Count + "/" + wss.RestoredChargesCount + " charges available)";
    }

    public bool IsBowLike
    {
      get { return Kind == WeaponKind.Bow || Kind == WeaponKind.Crossbow; }
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
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToMeleeHit, chanceForEffect);
            break;
          case WeaponKind.Bashing:
            
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToCauseStunning, chanceForEffect);
            SpecialFeatureAux = new EntityStat(EntityStatKind.ChanceToMeleeHit, -chanceForEffect);

            break;
          case WeaponKind.Axe:
            
            SpecialFeature = new EntityStat(EntityStatKind.ChanceToCauseTearApart, chanceForEffect);
            
            break;
          case WeaponKind.Scepter:
          case WeaponKind.Staff:
          case WeaponKind.Wand:
            spellSource = new WeaponSpellSource(this, Spells.SpellKind.FireBall);
            break;
          case WeaponKind.Bow:
          case WeaponKind.Crossbow:
            this.PrimaryStatKind = EntityStatKind.PhysicalProjectilesAttack;
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

    float GetDamageIncreasePerc()
    {
      float max = 20;
      float fact = .1f;

      if (Kind == WeaponKind.Bashing)
        return max+ fact*40;
      if (Kind == WeaponKind.Axe)
        return max + fact * 20;
      if (Kind == WeaponKind.Sword)
        return max - fact;
      if (Kind == WeaponKind.Dagger)
        return max - fact;
      if (IsMagician)
        return max - 4*fact;

      //bowlike
      return max - 62*fact;
    }

    public int CalcDamageFromLevel(int startDmg)
    {
      Damage = FactorCalculator.CalcFromLevel3(LevelIndex, startDmg, GetDamageIncreasePerc());
      return Damage;
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
    public SpellSource SpellSource 
    { 
      get => spellSource;
      set
      {
        spellSource = value;
        if (spellSource is WeaponSpellSource wss)
          wss.Weapon = this;
      }
    }

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
      if (IsMagician)
        UpdateMagicWeaponDesc();
      else
        PrimaryStatDescription = PrimaryStatKind.ToDescription() + ": " + GetDamageDescription();
    }

    public override string ToString()
    {
      return base.ToString() + " " + Damage;
    }

    public static readonly FightItemKind[] BowAmmoKinds = new[] 
    {
      FightItemKind.PlainArrow, FightItemKind.IronArrow, FightItemKind.SteelArrow ,
       FightItemKind.PoisonArrow,  FightItemKind.IceArrow,  FightItemKind.FireArrow
    };
    public static readonly FightItemKind[] CrossBowAmmoKinds = new[] 
    {
      FightItemKind.PlainBolt, FightItemKind.IronBolt, FightItemKind.SteelBolt ,
      FightItemKind.PoisonBolt, FightItemKind.IceBolt, FightItemKind.FireBolt ,
    };
    public static readonly FightItemKind[] AllBowLikeAmmoKinds = null; //see ctor

    public static bool IsBowAmmoKind(FightItemKind kind)
    {
      return BowAmmoKinds.Contains(kind);
    }

    public static bool IsBowLikeAmmoKind(FightItemKind kind)
    {
      return AllBowLikeAmmoKinds.Contains(kind);
    }

    public static bool IsCrossBowAmmoKind(FightItemKind kind)
    {
      return CrossBowAmmoKinds.Contains(kind);
    }

    public FightItemKind[] GetFightItemKindAmmo()
    {
      if (IsBowLike)
      {
        if (this.Kind == WeaponKind.Bow)
          return BowAmmoKinds;
        else if (this.Kind == WeaponKind.Crossbow)
          return CrossBowAmmoKinds;
      }

      return new []{FightItemKind.Unset}; 
    }
  }
}
