using Dungeons.Core;
using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Tiles
{
  public class Equipment : Loot
  {
    public int MinDropDungeonLevel = 100;
    EquipmentKind kind;
    EntityStat primaryStat;
    EquipmentClass _class;
    public int RequiredLevel { get; set; }
    EntityStats requiredStats = new EntityStats();
    public bool Identified { get; set; } = true;

    public Equipment() : this(EquipmentKind.Unset)
    {
      
    }

    public Equipment(EquipmentKind kind = EquipmentKind.Unset)//default arg for serialization
    {
      primaryStat = new EntityStat();
      EquipmentKind = kind;
      Class = EquipmentClass.Plain;
      LootKind = LootKind.Equipment;
    }

    public void Identify()
    {
      if (Identified)
        return;
      this.Identified = true;
      if (unidentifiedStats != null)//TODO assert
      {
        ExtendedInfo.Stats.Accumulate(unidentifiedStats);
        IncreasePriceBasedOnExtInfo();
      }
    }

    public EntityStats GetStats()
    {
      EntityStats stats = new EntityStats();
      stats.Stats[this.PrimaryStatKind].Value.Accumulate(primaryStat.Value);
      //if (eq is Weapon)
      //{
      //  stats.Stats[EntityStatKind.Attack].Factor += eq.PrimaryStatValue;
      //}
      //else if (eq is Armor)
      //{
      //  stats.Stats[EntityStatKind.Defence].Factor += eq.PrimaryStatValue;
      //}
      //else if (eq is Jewellery)
      //{
      //  var juw = eq as Jewellery;
      //  stats.Stats[juw.PrimaryStat].Factor += juw.PrimaryStatValue;
      //}
      if (!IsPlain())
      {
        stats.Accumulate(ExtendedInfo.Stats);
      }
      return stats;
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

    public virtual EquipmentKind EquipmentKind
    {
      get
      {
        return kind;
      }

      set
      {
        kind = value;
      }
    }
    
    //public static LootKind[] GetPossibleLootKindsForCrafting()
    //{
    //  return possibleLootKinds;
    //}

    //internal LootKind GetLootKind()
    //{
    //  //if (EquipmentKind == EquipmentKind.Weapon)
    //  //  return LootKind.Weapon;
    //  //else if (EquipmentKind == EquipmentKind.Amulet || EquipmentKind == EquipmentKind.RingLeft || EquipmentKind == EquipmentKind.RingRight)
    //  //  return LootKind.Jewellery;
    //  //else if (EquipmentKind == EquipmentKind.Armor || EquipmentKind == EquipmentKind.Shield || EquipmentKind == EquipmentKind.Helmet)
    //  //  return LootKind.Armor;
    //  //else if (EquipmentKind == EquipmentKind.TrophyLeft)//|| Type == EquipmentKind.TrophyRight)
    //  //  return LootKind.Trophy;
    //  //return LootKind.Other;
    //  return LootKind.Equipment;
    //}

    
    public EntityStatKind PrimaryStatKind
    {
      get
      {
        return primaryStat.Kind;
      }
      set
      {
        primaryStat.Kind = value;//needed for serial...
        SetPrimaryStatDesc();
      }
    }

    void SetPrimaryStatDesc()
    {
      primaryStatDesc = primaryStat.Kind.ToString() + ": " + primaryStat.Value.Nominal;
    }

    public float PrimaryStatValue
    {
      get
      {
        return primaryStat.Value.Factor;
      }

      set
      {
        primaryStat.Value.Factor = value;
      }
    }

    public void SetPrimaryStat(EntityStatKind primaryStat, float value)
    {
      this.primaryStat = new EntityStat(primaryStat, 0);
      PrimaryStatValue = value; 
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetMagicStats()
    {
      return ExtendedInfo.Stats.Stats.Where(i => i.Value.Factor > 0).ToList();
    }

    public void SetMagicStat(EntityStatKind statKind, EntityStat stat)
    {
      ExtendedInfo.Stats.Stats[statKind] = stat;
    }

    internal bool IsBetter(Equipment currentEq)
    {
      return Price > currentEq.Price;
    }

    public override string PrimaryStatDescription
    {
      get { return primaryStatDesc; }
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
      EntityStatKind esk = EntityStatKind.Unset;
      List<EntityStatKind> possibleChoices = null;
      switch (this.kind)
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
        case EquipmentKind.RingLeft:
        case EquipmentKind.RingRight:
        case EquipmentKind.Amulet:
          possibleChoices = possibleChoicesJewelery;
          break;
        default:
          break;
      }
      esk = RandHelper.GetRandomElem<EntityStatKind>(possibleChoices, skip);
      return esk;
    }

    public void MakeMagic(bool magicOfSecondLevel = false)
    {
      Debug.Assert(levelIndex >= 0);
      var stat = AddMagicStat(new[] { EntityStatKind.Unset, this.primaryStat.Kind }, false);
      if (magicOfSecondLevel)
      {
        priceAlrIncreased = false;
        AddMagicStat(new[] { EntityStatKind.Unset, this.primaryStat.Kind, stat }, true);
      }

      IncreasePriceBasedOnExtInfo();
    }

    public EntityStatKind AddMagicStat(EntityStatKind[] skip, bool secMagicLevel)
    {
      var stat = GetRandomStatForMagicItem(skip);
      AddMagicStat(stat, secMagicLevel);
      return stat;
    }

    public void AddMagicStat(EntityStatKind esk)
    {
      AddMagicStat(esk, GetMagicStats().Any());
    }

    public EntityStatKind AddRandomMagicStat()
    {
      var skip = GetMagicStats().Select(i => i.Key).ToList();
      skip.Add(primaryStat.Kind);
      var stat = GetRandomStatForMagicItem(skip.ToArray());
      AddMagicStat(stat, GetMagicStats().Any());
      return stat;
    }

    public bool IsSecondMagicLevel { get; set; }
    public EntityStat PrimaryStat { get => primaryStat; set => primaryStat = value; }
    public EntityStats RequiredStats { get => requiredStats; set => requiredStats = value; }

    public List<EntityStat> GetEffectiveRequiredStats()
    {
      return RequiredStats.Stats.Values.Where(i => GetReqStatValue(i) > 0).Select(i=> i).ToList();
    }

    public float GetReqStatValue(EntityStat es)
    {
      return es.Value.TotalValue;
    }

    void AddMagicStat(EntityStatKind stat, bool secLevel)
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
      AddMagicStat(stat, secLevel, value, false);
    }

    public void MakeMagic(EntityStatKind stat, int statValue)
    {
      AddMagicStat(stat, false, statValue, false);
    }

    public void MakeMagicSecLevel(EntityStatKind stat, int statValue)
    {
      AddMagicStat(stat, true, statValue, false);
    }

    EntityStats unidentifiedStats;

    void AddMagicStat(EntityStatKind stat, bool secLevel, int statValue, bool incrementFactor = false)
    {
      var factorBefore = ExtendedInfo.Stats.GetFactor(stat);
      if (factorBefore > 0 && incrementFactor)
        statValue += statValue;
      SetClass(EquipmentClass.Magic);//we shall not lost that info

      if (unidentifiedStats == null)
      {
        unidentifiedStats = new EntityStats();
        Price *= 2;
      }

      //ExtendedInfo.Stats.SetFactor(stat, statValue);
      unidentifiedStats.SetFactor(stat, statValue);
      IsSecondMagicLevel = secLevel;
      IncreasePriceBasedOnExtInfo();
    }

    public override void HandleGenerationDone()
    {
      Debug.Assert(this.MinDropDungeonLevel >= 0);
      if (this.MinDropDungeonLevel >= 0)
      {
        Price += Price*this.MinDropDungeonLevel;
      }
    }

    public bool priceAlrIncreased;
    
    public void IncreasePriceBasedOnExtInfo()
    {
      if(!Identified)
        return;
      if (priceAlrIncreased)
      {
        //Debug.Assert(false, "priceAlrIncreased");
        return;
      }
      basePrice = Price;
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

    //set based of the Dungeon Level it was dropped on.
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
      SetClass(_class);
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
        //ExtendedInfo.Stats = lootStats;
        unidentifiedStats = lootStats;
        IsSecondMagicLevel = magicOfSecondLevel;
      }
      IncreasePriceBasedOnExtInfo();
    }

    private void SetClass(EquipmentClass _class)
    {
      Class = _class;
      if (_class != EquipmentClass.Plain)
        Identified = false;
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

    //public override Loot Clone()
    //{
    //  var clone = base.Clone();
    //  //var eq = clone as Equipment;
      
    //  return clone;
    //}
  }
}
