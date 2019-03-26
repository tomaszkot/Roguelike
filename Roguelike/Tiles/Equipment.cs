using Dungeons.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class Equipment : Loot
  {
    readonly static LootKind[] possibleLootKinds = new LootKind[] { LootKind.Armor, LootKind.Weapon, LootKind.Jewellery };

    EquipmentKind type;
    private EntityStatKind primaryStat = EntityStatKind.Unknown;
    private float primaryStatValue;
    EquipmentClass _class;

    public Equipment() : this(EquipmentKind.Weapon)
    {

    }

    public Equipment(EquipmentKind kind = EquipmentKind.Weapon)//default arg for serialization
    {
      this.EquipmentKind = kind;
      Class = EquipmentClass.Plain;
      ExtendedInfo = new LootExtendedInfo();
    }

    public EquipmentClass Class
    {
      get
      {
        return _class;
      }

      set
      {
        _class = value;
      }
    }

    public LootExtendedInfo ExtendedInfo { get; private set; }

    public virtual EquipmentKind EquipmentKind
    {
      get
      {
        return type;
      }

      set
      {
        type = value;
      }
    }
    
    public static LootKind[] GetPossibleLootKindsForCrafting()
    {
      return possibleLootKinds;
    }

    internal LootKind GetLootKind()
    {
      if (EquipmentKind == EquipmentKind.Weapon)
        return LootKind.Weapon;
      else if (EquipmentKind == EquipmentKind.Amulet || EquipmentKind == EquipmentKind.Ring)
        return LootKind.Jewellery;
      else if (EquipmentKind == EquipmentKind.Armor || EquipmentKind == EquipmentKind.Shield || EquipmentKind == EquipmentKind.Helmet)
        return LootKind.Armor;
      else if (EquipmentKind == EquipmentKind.TrophyLeft)//|| Type == EquipmentKind.TrophyRight)
        return LootKind.Trophy;
      return LootKind.Other;
    }

    public EntityStatKind PrimaryStat
    {
      get
      {
        return primaryStat;
      }
      set
      {
        primaryStat = value;//needed for serial...

      }
    }

    public float PrimaryStatValue
    {
      get
      {
        return primaryStatValue;
      }

      set
      {
        primaryStatValue = value;
      }
    }


    public void SetPrimaryStat(EntityStatKind primaryStat, float value)
    {
      this.primaryStat = primaryStat;
      this.PrimaryStatValue = value;
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetMagicStats()
    {
      return ExtendedInfo.Stats.Stats.Where(i => i.Value.Factor > 0).ToList();
    }

    public void SetMagicStat(EntityStatKind statKind, EntityStat stat)
    {
      ExtendedInfo.Stats.Stats[statKind] = stat;
    }

    public override string GetPrimaryStatDescription()
    {
      return PrimaryStat + ": " + PrimaryStatValue;
    }

    public static List<EntityStatKind> possibleChoicesWeapon = new List<EntityStatKind>() { EntityStatKind.Attack, EntityStatKind.ChanceToHit, EntityStatKind.ColdAttack,
      EntityStatKind.FireAttack,  EntityStatKind.PoisonAttack};

    public static List<EntityStatKind> possibleChoicesWeaponMagician = new List<EntityStatKind>()
    {
      EntityStatKind.Magic, EntityStatKind.Mana, /*EntityStatKind.ManaStealing, uniq*/
      EntityStatKind.ChanceToCastSpell
      //,  EntityStatKind.ChanceToEvadeMeleeAttack, EntityStatKind.ChanceToEvadeMeleeAttack //TODO
    };

    public static List<EntityStatKind> possibleChoicesArmor = new List<EntityStatKind>() { EntityStatKind.Defence, EntityStatKind.ChanceToHit, EntityStatKind.Health,
      EntityStatKind.Magic, EntityStatKind.Mana, EntityStatKind.ResistCold, EntityStatKind.ResistFire,
      EntityStatKind.ResistPoison, EntityStatKind.LightPower, EntityStatKind.ChanceToCastSpell};

    public static List<EntityStatKind> possibleChoicesJewelery = new List<EntityStatKind>() { EntityStatKind.Mana, EntityStatKind.Magic, EntityStatKind.Health,
    EntityStatKind.ResistCold, EntityStatKind.ResistFire, EntityStatKind.ResistPoison, EntityStatKind.ChanceToCastSpell};


    EntityStatKind GetRandomStatForMagicItem(EntityStatKind[] skip)
    {
      EntityStatKind esk = EntityStatKind.Unknown;
      List<EntityStatKind> possibleChoices = null;
      switch (this.type)
      {
        case EquipmentKind.Weapon:
          var wpn = this as Weapon;
          possibleChoices = possibleChoicesWeapon.ToList();//make copy

          if (wpn != null)
          {
            //TODO
            //if (wpn.Kind == Weapon.WeaponKind.Bashing)
            //{
            //  possibleChoices.Add(EntityStatKind.ChanceToCauseStunning);
            //}
            //else if (wpn.Kind == Weapon.WeaponKind.Axe)
            //{
            //  possibleChoices.Add(EntityStatKind.ChanceToCauseTearApart);
            //}
            //else if (wpn.Kind == Weapon.WeaponKind.Dagger)
            //{
            //  possibleChoices.Add(EntityStatKind.ChanceToCauseBleeding);
            //}
          }
          break;
        case EquipmentKind.Armor:
        case EquipmentKind.Helmet:
        case EquipmentKind.Shield:
          possibleChoices = possibleChoicesArmor;
          break;
        case EquipmentKind.Ring:
        case EquipmentKind.Amulet:
          possibleChoices = possibleChoicesJewelery;
          break;
        default:
          break;
      }
      esk = RandHelper.GetRandomElem<EntityStatKind>(possibleChoices, skip);
      return esk;
    }


    

    void MakeMagic(bool magicOfSecondLevel = false)
    {
      var stat = AddMagicStat(new[] { EntityStatKind.Unknown, this.PrimaryStat }, false);
      if (magicOfSecondLevel)
      {
        AddMagicStat(new[] { EntityStatKind.Unknown, this.PrimaryStat, stat }, true);
      }
    }

    public EntityStatKind AddMagicStat(EntityStatKind[] skip, bool secMagicLevel)
    {
      var stat = GetRandomStatForMagicItem(skip);
      MakeMagic(stat, secMagicLevel);
      return stat;
    }

    public void AddMagicStat(EntityStatKind esk)
    {
      MakeMagic(esk, GetMagicStats().Any());
    }

    public EntityStatKind AddRandomMagicStat()
    {
      var skip = GetMagicStats().Select(i => i.Key).ToList();
      skip.Add(PrimaryStat);
      var stat = GetRandomStatForMagicItem(skip.ToArray());
      MakeMagic(stat, GetMagicStats().Any());
      return stat;
    }

    public bool IsSecondMagicLevel { get; set; }

    void MakeMagic(EntityStatKind stat, bool secLevel)
    {
      var value = levelIndex + 1;
      if (RandHelper.Random.NextDouble() > 0.5f)
      {
        value++;
      }
      if (RandHelper.Random.NextDouble() > 0.5f)
      {
        value++;
      }
      if (stat == EntityStatKind.ResistCold || stat == EntityStatKind.ResistFire || stat == EntityStatKind.ResistPoison)
        value *= 2;
      
      if (stat == EntityStatKind.FireAttack || stat == EntityStatKind.PoisonAttack || stat == EntityStatKind.ColdAttack)
      {
        value /= 2;
        value++;
      }
      if (value == 1)
        value = 2;
      MakeMagic(stat, secLevel, value, false);
    }

    private void MakeMagic(EntityStatKind stat, bool secLevel, int value, bool incrementFactor = false)
    {
      var factorBefore = ExtendedInfo.Stats.GetFactor(stat);
      if (factorBefore > 0 && incrementFactor)
        value += value;
      Class = EquipmentClass.Magic;//we shall not lost that info

      ExtendedInfo.Stats.SetFactor(stat, value);
      IsSecondMagicLevel = secLevel;
    }

    public bool priceAlrIncreased;
    public void IncreasePriceBasedOnExtInfo()
    {
      if (priceAlrIncreased)
      {
        //Debug.Assert(false, "priceAlrIncreased");
        return;
      }
      //var priceInc = 0;
      foreach (var st in ExtendedInfo.Stats.Stats)
      {
        var prInc = GetPriceForFactor(st.Key, (int)st.Value.Factor);
        if (prInc > 0)
          Price += prInc;
      }

      //Price += priceInc;
      priceAlrIncreased = true;
    }

    public int GetPriceForFactor(EntityStatKind esk, int factor)
    {
      var price = 0;
      var priceFactor = factor;
      if (priceFactor == 0)
        return 0;
      var inc = (int)priceFactor;
      if (esk == EntityStatKind.LightPower)
        inc /= 2;
      price += inc;
      if (esk != EntityStatKind.LightPower)
      {
        if (priceFactor >= 15)
          price += 20;
        else if (priceFactor >= 10)
          price += 15;
        else
          price += 10;
      }

      if (esk == EntityStatKind.ManaStealing || esk == EntityStatKind.LifeStealing)//||
      {
        price += (int)(priceFactor * 1.5f);
      }
      else if (esk == EntityStatKind.FireAttack || esk == EntityStatKind.PoisonAttack || esk == EntityStatKind.ColdAttack)
      {
        price += (int)(priceFactor * 3f);
      }
      return price;
    }

    int levelIndex = -1;

    public void SetUnique(EntityStats lootStats)
    {
      SetClass(EquipmentClass.Unique, -1, lootStats);
    }

    public int GetLevelIndex() { return levelIndex; }
    public void SetLevelIndex(int li) { levelIndex = li; }

    public virtual void SetClass(EquipmentClass _class, int levelIndex, EntityStats lootStats = null, bool magicOfSecondLevel = false)
    {
      this.levelIndex = levelIndex;
      Class = _class;
      if (lootStats == null)
      {
        //TODO assert
        //Assert(ExtendedInfo == null || ExtendedInfo.Stats.Stats.All(i => i.Value.NominalValue == 0));
        //Assert(levelIndex >= 0);
        //Assert(_class == EquipmentClass.Magic);
        if (_class == EquipmentClass.Magic)
          MakeMagic(magicOfSecondLevel);
      }
      else
      {
        ExtendedInfo.Stats = lootStats;
        IsSecondMagicLevel = magicOfSecondLevel;
      }
      IncreasePriceBasedOnExtInfo();
    }

    public bool IsPlain()
    {
      return Class == EquipmentClass.Plain;
    }

    protected bool includeTypeInToString = true;

    public override string ToString()
    {
      var res = "";
      if (includeTypeInToString)
        res += this.EquipmentKind + " ";
      res += base.ToString();
      return res;
    }

    public override Loot Clone()
    {
      var clone = base.Clone();
      var eq = clone as Equipment;
      eq.ExtendedInfo = this.ExtendedInfo.Clone() as LootExtendedInfo;
      return clone;
    }
  }
}
