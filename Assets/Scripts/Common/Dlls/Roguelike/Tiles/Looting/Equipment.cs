﻿using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.TileParts;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public class Enchant
  {
    public Enchanter Enchanter { get; set; }
    public List<EntityStatKind> StatKinds { get; set; } = new List<EntityStatKind>();
    public int StatValue { get; set; }
  }

  public enum EquipmentMaterial { Unset, Bronze, Iron, Steel }

  public interface IEquipment
  {
    EquipmentKind EquipmentKind { get; set; }
    AnimalKind MatchingAnimalKind { get; set; }
    string Name { get; set; }
    int Price { get; set; }
    bool IsIdentified { get; set; }
    EntityStats GetStats();
    void PrepareForSave();
    int RequiredLevel { get; set; }

    List<EntityStat> GetEffectiveRequiredStats();

    int Count { get; set; }

    string Tag1 { get; }

    bool IsBetter(IEquipment currentEq);

    float GetReqStatValue(EntityStat es);
  }

  public class Equipment : Loot, IEquipment
  {
    public AnimalKind MatchingAnimalKind { get; set; }
    public EquipmentMaterial Material { get; set; }
    EquipmentKind kind;
    EntityStat primaryStat;
    EquipmentClass _class;
    public int RequiredLevel { get; set; } = 1;
    EntityStats requiredStats = new EntityStats();
    public bool IsIdentified { 
      get; 
      set; } = true;
    public event EventHandler<Loot> Identified;

    public bool Enchantable { get { return maxEnchants > 0; } }
    List<Enchant> enchants = new List<Enchant>();
    int enchantSlots = 0;
    int maxEnchants = 0;
    public LootExtendedInfo ExtendedInfo { get; set; }
    public int Count { get; set; } = 1;

    public Equipment() : this(EquipmentKind.Unset)
    {
    }

    public Equipment(EquipmentKind kind = EquipmentKind.Unset)//default arg for serialization
    {
      ExtendedInfo = new LootExtendedInfo();
      primaryStat = new EntityStat();
      EquipmentKind = kind;
      Class = EquipmentClass.Plain;
      LootKind = LootKind.Equipment;
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if(Class == EquipmentClass.Unique)
        return false;
      if (kind == RecipeKind.OneEq || kind == RecipeKind.TwoEq ||
        kind == RecipeKind.EnchantEquipment || kind == RecipeKind.UnEnchantEquipment)
        return true;
      return false;
    }

    public void SetMaterial(EquipmentMaterial material)
    {
      if (material == EquipmentMaterial.Unset)
        return;
      if (Class == EquipmentClass.Unique)
        return;

      if (this.Material != EquipmentMaterial.Unset &&
      this.Material != EquipmentMaterial.Bronze)
      {
        Dungeons.DebugHelper.Assert(false);//ups already set
        return;
      }

      var eskToEnhance = EntityStatKind.Unset;
      switch (EquipmentKind)
      {
        case EquipmentKind.Weapon:
          eskToEnhance = EntityStatKind.MeleeAttack;
          break;
        default:
          break;
      }
      if (material == EquipmentMaterial.Bronze && this is Weapon)
      {
        if (LevelIndex > 7)
        {
          int k = 0;
          k++;
        }
      }
      this.Material = material;
     

      if (eskToEnhance != EntityStatKind.Unset)
      {
        if (Class != EquipmentClass.Unique)
        {
          this.DisplayedName = "";
          SetDisplayedName();
          EnhanceStatsDueToMaterial(material);
        }
      }
    }

    public void SetDisplayedName()
    {
      if (string.IsNullOrEmpty(displayedName) || displayedName == "Unset")
        this.DisplayedName = GetDefaultName();
    }

    protected override string GetDefaultName()
    {
      return GetNameWithMaterial(Name);
    }

    private string GetNameWithMaterial(string name)
    {
      string defaultName = "";
      if (Material != EquipmentMaterial.Unset)
      {
        defaultName = this.Material.ToDescription() + " ";
        name = name.ToLower();
      }
      else
        name = name.ToUpperFirstLetter();

      return defaultName + name;
    }

    public override string Name
    {
      get => base.Name;
      set
      {
        //if (Name.Contains("Armor") && !DisplayedName.Contains("Armor"))
        //{
        //  int k = 0;
        //  k++;
        //}
        //if (Name.Contains("Shield") && !DisplayedName.Contains("Shield"))
        //{
        //  int k = 0;
        //  k++;
        //}
        var val = value;
        
        base.Name = val;
       

        EnsureDisplayedName();

      }
    }

   

    protected override bool DisplayedNameNeedsToBeSet()
    {
      if (!name.Any())
        return false;
      if (Class == EquipmentClass.Unique)
      {
        if (StringStartsWithMaterial(DisplayedName, EquipmentMaterial.Bronze) ||
            StringStartsWithMaterial(DisplayedName, EquipmentMaterial.Iron) ||
            StringStartsWithMaterial(DisplayedName, EquipmentMaterial.Steel))
          return true;
      }

      if (name.Any())
      {
        var dn = DisplayedName;
        if (!Char.IsUpper(name[0]) && (!dn.Any() || !Char.IsUpper(dn[0])))
          return true;
      }
      var assetNoLevel = EnsureLevelNotInAssetName(tag1);

      if (assetNoLevel.Any(char.IsDigit))//wand1
        return false;

      return base.DisplayedNameNeedsToBeSet();
    }


    private void EnsureDisplayedName()
    {
      if (DisplayedNameNeedsToBeSet())
      {
        if (Class == EquipmentClass.Unique)
        {
          DisplayedName = Name.ToUpperFirstLetter();
        }
        else if (Name.Any())
        {
          DisplayedName = Name.ToUpperFirstLetter();
        }
      }
    }

    bool StringStartsWithMaterial(string str)
    {
      return StringStartsWithMaterial(str, Material);
    }
    bool StringStartsWithMaterial(string str, EquipmentMaterial mat)
    {
      return str.StartsWith(mat.ToDescription());
    }


    public override string DisplayedName
    {
      set
      {
        var val = value;
        if (value.Any())
        {
          if (IsMaterialAware() && Class != EquipmentClass.Unique && !StringStartsWithMaterial(val))
          {
            val = GetNameWithMaterial(val);
          }
        }
        base.DisplayedName = val;
      }
    }

    //public void SetNameFromAsset(string asset)
    //{
    //  DisplayedName = "";
    //  Name = asset.Replace("_", " ").ToUpperFirstLetter();
    //}

    protected virtual void EnhanceStatsDueToMaterial(EquipmentMaterial material)
    {
      throw new Exception("EnhanceMaterial!");
    }

    public bool MakeEnchantable(int enchantSlotsToMake = 1)
    {
      if ( Class != EquipmentClass.Plain && !IsIdentified)
        return false;
      if (maxEnchants > 0)
        return false; //already done

      maxEnchants = GetMaxEnchants();
      enchantSlots = enchantSlotsToMake;

      var priceInc = ((float)Price) / 5;
      if (priceInc == 0)
        priceInc = 1;
      priceInc *= EnchantSlots;
      Price += (int)priceInc;
      return true;
    }

    public bool MaxEnchantsReached()
    {
      return Enchants.Count >= EnchantSlots;
    }

    public bool IncreaseEnchantSlots()
    {
      if (EnchantSlots < maxEnchants)
      {
        enchantSlots++;
        return true;
      }
      return false;
    }

    public List<Enchant> Enchants
    {
      get
      {
        return enchants;
      }

      set
      {
        enchants = value;
      }
    }

    int GetMaxEnchants()
    {
      if (Class == EquipmentClass.Unique)
      {
        if (this is Trophy)
        {
          return 2;
        }
        else
          return 0;
      }
      if (Class == EquipmentClass.Magic)
        return IsSecondMagicLevel ? 1 : 2;

      return 3;
    }

    public bool Identify()
    {
      if (IsIdentified)
        return false;
      this.IsIdentified = true;
      if (unidentifiedStats != null)
      {
        foreach (var stat in unidentifiedStats.GetStats())
        {
          var sign = stat.Value.Factor > 0 ? 1 : -1;
          var pf = sign*GetPriceForFactor(stat.Key, (int)stat.Value.Factor);
          Price += pf;
        }

        ExtendedInfo.Stats.Accumulate(unidentifiedStats);
        if (Identified != null)
          Identified(this, this);

        return true;
      }
      Dungeons.DebugHelper.Assert(false);
      return false;
    }

    public virtual EntityStats GetStats()
    {
      EntityStats stats = new EntityStats();
      stats[this.PrimaryStatKind].Accumulate(primaryStat.Value);
      stats.Accumulate(ExtendedInfo.Stats);

      return stats;
    }

    internal void RemoveMagicStat(EntityStatKind stat, int statValue)
    {
      var es = ExtendedInfo.Stats.GetStat(stat);
      es.Value.Factor -= statValue;
    }

    public EquipmentClass Class
    {
      get {return _class;}

      set
      {
        _class = value;
      }
    }

    public virtual EquipmentKind EquipmentKind
    {
      get{return kind;}
      set
      {
        kind = value;
        if (kind == EquipmentKind.Armor && !name.Contains("Armor"))
        {
          int k = 0;
          k++;
        }
      }
    }

    readonly static EquipmentKind[] possibleLootKinds = new EquipmentKind[] { EquipmentKind.Armor, EquipmentKind.Weapon, EquipmentKind.Amulet,
      EquipmentKind.Ring, EquipmentKind.Helmet, EquipmentKind.Glove, EquipmentKind.Shield};

    public static EquipmentKind[] GetPossibleLootKindsForCrafting()
    {
      return possibleLootKinds;
    }

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

    protected virtual void SetPrimaryStatDesc()
    {
      PrimaryStatDescription = primaryStat.Kind.ToDescription() + ": " + PrimaryStatValue;
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
        //if (value == 0 && kind == EquipmentKind.Weapon && 
        //    this is Weapon wpn 
        //    && !string.IsNullOrEmpty(this.tag1)
        //  //&& wpn.Kind == Weapon.WeaponKind.Bow
        //  )
        //{
        //  int k = 0;
        //  k++;
        //}
        SetPrimaryStatDesc();
      }
    }

    public void SetPrimaryStat(EntityStatKind primaryStat, float value)
    {
      this.primaryStat = new EntityStat(primaryStat, 0);
      PrimaryStatValue = value;
      //if (string.IsNullOrEmpty(this.Name))
      //  Name = EquipmentKind.ToString();
      //this.Name += " of " + primaryStat.ToDescription();
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetPossibleMagicStats()
    {
      return ExtendedInfo.Stats.GetStats().Where(i => i.Value.Factor == 0).ToList();
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetMagicStats()
    {
      return ExtendedInfo.Stats.GetStats().Where(i => i.Value.Factor != 0).ToList();
    }

    public void SetMagicStat(EntityStatKind statKind, EntityStat stat)
    {
      ExtendedInfo.Stats.SetStat(statKind, stat);
    }

    public bool IsBetter(IEquipment currentEq)
    {
      return Price > currentEq.Price;
    }

    public static List<EntityStatKind> possibleChoicesWeapon = new List<EntityStatKind>() 
    { 
      //EntityStatKind.Attack, TODO es
      //EntityStatKind.ChanceToHit, TODO es
      EntityStatKind.ColdAttack,
      EntityStatKind.FireAttack, 
      EntityStatKind.PoisonAttack
    };

    public static List<EntityStatKind> possibleChoicesWeaponMagician = new List<EntityStatKind>()
    {
      EntityStatKind.Magic, EntityStatKind.Mana, /*EntityStatKind.ManaStealing, uniq*/
      EntityStatKind.ChanceToCastSpell
      ,EntityStatKind.ChanceToEvadeMeleeAttack, EntityStatKind.ChanceToEvadeMeleeAttack
    };

    public static List<EntityStatKind> possibleChoicesArmor = new List<EntityStatKind>() 
    {
      EntityStatKind.Defense, 
      //EntityStatKind.ChanceToHit, TODO es
      EntityStatKind.Health,
      EntityStatKind.Magic, EntityStatKind.Mana, EntityStatKind.ResistCold, EntityStatKind.ResistFire,
      EntityStatKind.ResistPoison, EntityStatKind.LightPower, EntityStatKind.ChanceToCastSpell
    };

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
            if (wpn.Kind == Weapon.WeaponKind.Bashing)
            {
              possibleChoices.Add(EntityStatKind.ChanceToCauseStunning);
            }
            else if (wpn.Kind == Weapon.WeaponKind.Axe)
            {
              possibleChoices.Add(EntityStatKind.ChanceToCauseTearApart);
            }
            else if (wpn.Kind == Weapon.WeaponKind.Dagger)
            {
              possibleChoices.Add(EntityStatKind.ChanceToCauseBleeding);
            }
          }
          break;
        case EquipmentKind.Armor:
        case EquipmentKind.Helmet:
        case EquipmentKind.Shield:
        case EquipmentKind.Glove:
          possibleChoices = possibleChoicesArmor;
          break;
        case EquipmentKind.Ring:
        //case EquipmentKind.RingRight:
        case EquipmentKind.Amulet:
          possibleChoices = possibleChoicesJewelery;
          break;
        default:
          break;
      }
      esk = RandHelper.GetRandomElem<EntityStatKind>(possibleChoices, skip);
      return esk;
    }

    public virtual bool IsMaterialAware()
    {
      if (Class == EquipmentClass.Unique)
        return false;
      if (this is Weapon wpn)
      {
        var typical = wpn.Kind == Weapon.WeaponKind.Axe || wpn.Kind == Weapon.WeaponKind.Sword || wpn.Kind == Weapon.WeaponKind.Dagger;
        if (typical)
          return true;
        if (wpn.Kind == Weapon.WeaponKind.Bashing)
        {
          return wpn.tag1 == "hammer" || wpn.tag1 == "solid_hammer" || wpn.tag1 == "war_hammer";
        }
      }

      //TODO
      //else if (this is Armor arm && 
      //  (arm.EquipmentKind == EquipmentKind.Helmet || arm.EquipmentKind == EquipmentKind.Shield))
      //{
      //  if (arm.EquipmentKind == EquipmentKind.Helmet)
      //  {
      //    if (arm.tag1 == "helm" ||
      //      arm.tag1 == "full_helm" ||
      //      arm.tag1 == "holly_helm" 
      //      )
      //    {
      //      return true;
      //    }
      //  }
      //  if (arm.EquipmentKind == EquipmentKind.Shield)
      //  {
      //    if (arm.tag1 == "enhanced_buckler" ||
      //      arm.tag1 == "long_shield" ||
      //      arm.tag1 == "war_shield" ||
      //      arm.tag1 == "king's_buckler"
      //      )
      //    {
      //      return true;
      //    }
      //  }
      //}

      return false;
    }

    public void MakeMagic(EntityStatKind stat, int statValue, AddMagicStatReason reason = AddMagicStatReason.Unset)
    {
      AddMagicStat(stat, IsSecondMagicLevel, statValue, reason == AddMagicStatReason.Enchant, reason);
    }

    public bool HasMagicStat(EntityStatKind esk)
    {
      return GetMagicStats().Any(i => i.Key == esk);
    }

    public void MakeMagic(bool magicOfSecondLevel = false)
    {
      //DebugHelper.Assert(levelIndex >= 0);
      var toSkip = GetMagicStats().Select(i => i.Key).ToList();
      toSkip.AddRange(new[] { EntityStatKind.Unset, this.primaryStat.Kind });

      var stat = AddMagicStat(toSkip.ToArray(), false);
      if (magicOfSecondLevel)
      {
        //priceAlrIncreased = false;
        toSkip.Add(stat);
        AddMagicStat(toSkip.ToArray(), true);
      }
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

    public bool IsSecondMagicLevel { get { return Class == EquipmentClass.MagicSecLevel; } }
    public EntityStat PrimaryStat { get => primaryStat; set => primaryStat = value; }
    public EntityStats RequiredStats { get => requiredStats; set => requiredStats = value; }

    //setter needed from serialize
    public int EnchantSlots { get => enchantSlots; set { enchantSlots = value; } }

    public void SetRequiredStat(EntityStatKind esk, int value)
    {
      var es = new EntityStat(esk, value);
      this.RequiredStats.SetStat(es.Kind, es);
    }

    protected void SetRequiredStat(int levelIndex, EntityStatKind esk)
    {
      var baseVal = (int) Hero.GetStrengthStartStat();
      var fac = FactorCalculator.CalcFromLevel3(levelIndex, baseVal, 20);
      var es = new EntityStat(esk, fac);
      this.RequiredStats.SetStat(es.Kind, es);
    }

    public List<EntityStat> GetEffectiveRequiredStats()
    {
      //TODO too heavy, maybe do it once?
      return RequiredStats.GetStats().Where(i => GetReqStatValue(i.Value) > 0).Select(i => i.Value).ToList();
    }

    public EntityStat GetReqStat(EntityStatKind esk)
    {
      return RequiredStats.GetStats()[esk];
    }

    public float GetReqStatValue(EntityStatKind esk)
    {
      return GetReqStatValue(GetReqStat(esk));
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

    public void PromoteToSecondMagicClass()
    {
      if (!IsSecondMagicLevel)
      {
        var stats = GetPossibleMagicStats();
        var stat = RandHelper.GetRandomElem(stats);
        AddMagicStat(stat.Key, true);

      }
    }

    EntityStats unidentifiedStats;
    public enum AddMagicStatReason { Unset, Enchant };

    void AddMagicStat(EntityStatKind stat, bool secLevel, int statValue, bool incrementFactor = false, AddMagicStatReason reason = AddMagicStatReason.Unset)
    {
      var factorBefore = ExtendedInfo.Stats.GetFactor(stat);
      if (factorBefore > 0 && incrementFactor)
        statValue += statValue;

      if (reason != AddMagicStatReason.Enchant)
      {
        SetClass(EquipmentClass.Magic);//we shall not lost that info

        if (unidentifiedStats == null)
        {
          unidentifiedStats = new EntityStats();
        }
        unidentifiedStats.SetFactor(stat, statValue);
        if (Class == EquipmentClass.Magic && secLevel)
          Class = EquipmentClass.MagicSecLevel;

        if (!priceAlrIncreased)
        {
          Price += Price / 5;
          priceAlrIncreased = true;
        }
      }
      else
      {
        ExtendedInfo.Stats.SetFactor(stat, statValue);
        Price += GetPriceForFactor(stat, statValue);
      }
    }

    public virtual void PrepareForSave()
    {
      ExtendedInfo.Stats.PrepareForFullSave();

    }

    public override void HandleGenerationDone()
    {
      //DebugHelper.Assert(this.MinDropDungeonLevel >= 0);
      //if (this.MinDropDungeonLevel >= 0) 
      //{
      //  if(this.MinDropDungeonLevel > 0)
      //    Price = Price * this.MinDropDungeonLevel;
      //}
    }

    public bool priceAlrIncreased;

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

    //set based of the Dungeon Level it was dropped on, or if not applicable the enemy or chest level
    int levelIndex = -1;

    public void SetUnique(EntityStats lootStats, int lootLevel)
    {
      SetClass(EquipmentClass.Unique, lootLevel, lootStats);
      Material = EquipmentMaterial.Unset;
      EnsureDisplayedName();
    }

    public int MinDropDungeonLevel { get { return levelIndex; } }

    public int LevelIndex
    {
      get { return levelIndex; }
      set { levelIndex = value; }//for serializ
    }

    public EntityStats UnidentifiedStats { get => unidentifiedStats; set => unidentifiedStats = value; }
    public int MaxEnchants { get => maxEnchants; set => maxEnchants = value; }

    public virtual void SetLevelIndex(int li)
    {
      if (li <= 0)
        throw new Exception("Eq SetLevelIndex = 0!");
      if (li > 15)
      {
        int k = 0;
        k++;
      }
      
      levelIndex = li;
      if (RequiredLevel < li)
        RequiredLevel = li;

      //if(Class != EquipmentClass.Unique)
      SetPriceFromLevel();
    }

    public virtual void SetClass(EquipmentClass _class, int levelIndex, EntityStats lootStats = null, bool magicOfSecondLevel = false)
    {
      SetClass(_class);
      SetLevelIndex(levelIndex);
      
      if (lootStats == null)
      {
        if (_class == EquipmentClass.Magic)
          MakeMagic(magicOfSecondLevel);
      }
      else
      {
        unidentifiedStats = lootStats;
      }
    }

    private void SetClass(EquipmentClass _class)
    {
      Class = _class;
      IsIdentified = true;
      if (_class != EquipmentClass.Plain)
        IsIdentified = false;
    }

    public bool IsPlain()
    {
      return Class == EquipmentClass.Plain;
    }

    public override string ToString()
    {
      var res = base.ToString();
      if (IncludeDebugDetailsInToString)
        res += " Kind:" + this.EquipmentKind + " Lvl:" + levelIndex;

      return res;
    }

    public static EquipmentKind FromCurrentEquipmentKind(CurrentEquipmentKind currentEquipmentKind, out CurrentEquipmentPosition pos)
    {
      pos = CurrentEquipmentPosition.Left;
      if (currentEquipmentKind == CurrentEquipmentKind.RingRight)
      {
        pos = CurrentEquipmentPosition.Right;
        return EquipmentKind.Ring;
      }
      if (currentEquipmentKind == CurrentEquipmentKind.TrophyRight)
      {
        pos = CurrentEquipmentPosition.Right;
        return EquipmentKind.Trophy;
      }
      var res = (EquipmentKind)currentEquipmentKind;
      return res;
    }

    public static CurrentEquipmentKind FromEquipmentKind(EquipmentKind equipmentKind, CurrentEquipmentPosition pos)
    {
      //ring or trophy can be also right
      //if (pos == CurrentEquipmentPosition.Right)
      {
        if (equipmentKind == EquipmentKind.Ring || equipmentKind == EquipmentKind.Trophy)
        {
          //Assert(pos != CurrentEquipmentPosition.Unset, "FromEquipmentKind " + equipmentKind + " " + pos);
          if (pos == CurrentEquipmentPosition.Unset)
            return CurrentEquipmentKind.Unset;
          if (pos == CurrentEquipmentPosition.Right)
          {
            return equipmentKind == EquipmentKind.Ring ? CurrentEquipmentKind.RingRight : CurrentEquipmentKind.TrophyRight;
          }
          return equipmentKind == EquipmentKind.Ring ? CurrentEquipmentKind.RingLeft : CurrentEquipmentKind.TrophyLeft;
        }
      }
      var res = (CurrentEquipmentKind)equipmentKind;
      return res;
    }

    public bool WasCrafted;
    public RecipeKind CraftingRecipe;

    public bool WasCraftedBy(RecipeKind rec)
    {
      return WasCrafted && CraftingRecipe == rec;
    }

    public bool Enchant(EntityStatKind kind, int val, Enchanter enchantSrc, out string error)
    {
      return Enchant(new EntityStatKind[] { kind }, val, enchantSrc, out error);
    }

    public virtual bool CanBeEnchantedDueToClass(out string error)
    {
      error = "";
      if (Class == EquipmentClass.Unique)// && !(this is Trophy) && !(this is GodStatue))
      {
        error = "Unique item can not be enchanted";
        return false;
      }
      return true;

    }

    public bool Enchant(EntityStatKind[] kinds, int val, Enchanter enchantSrc, out string error)
    {
      error = "";
      if (!CanBeEnchantedDueToClass(out error))
      {
        return false;
      }

      if (EnchantSlots == 0)
      {
        error = "Not possible to enchant " + Name + ", this item is not enchantable";
        return false;
      }
      if (MaxEnchantsReached())
      {
        error = "Max enchanting level reached";
        return false;
      }
      var enchant = new Enchant();
      enchant.StatValue = val;
      foreach (var kind in kinds)
      {
        MakeMagic(kind, val, AddMagicStatReason.Enchant);
        //Price += (int)(GetPriceForFactor(kind, val));// *.9f);//crafted loot - price too hight comp to uniq.
        enchant.StatKinds.Add(kind);
      }

      enchant.Enchanter = enchantSrc;
      Enchants.Add(enchant);//only one slot is occupied
      return true;
    }

    public static Jewellery CreatePendant()
    {
      var juwell = new Jewellery();
      juwell.EquipmentKind = EquipmentKind.Amulet;
      juwell.SetLevelIndex(1);
      juwell.Price = 10;
      juwell.SetIsPendant(true);
      return juwell;
    }

    public override bool IsCraftable()
    {
      return base.IsCraftable() || Enchantable;
    }
    public void SetPriceFromLevel()
    {
      Price = (LevelIndex + 1) * 15;//TODO !
    }

    public string Tag1 { get { return tag1; } }

    public override RecipeKind GetMatchingRecipe(Loot other)
    {
      if (other is Gem || other is HunterTrophy)
        return RecipeKind.EnchantEquipment;

      if (other is Equipment)
        return RecipeKind.TwoEq;

      return RecipeKind.Unset;
    }
  }
}
