using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.TileParts;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.Tiles
{
  public class Enchant
  {
    public Enchanter Enchanter { get; set; }
    public List<EntityStatKind> StatKinds { get; set; } = new List<EntityStatKind>(); 
    public int StatValue { get; set; }
  }

  public enum EquipmentMaterial { Unset, Bronze, Iron, Steel }

  public class Equipment : Loot
  {
    public EquipmentMaterial Material { get; set; }
    EquipmentKind kind;
    EntityStat primaryStat;
    EquipmentClass _class;
    public int RequiredLevel { get; set; } = 1;
    EntityStats requiredStats = new EntityStats();
    public bool IsIdentified { get; set; } = true;
    public event EventHandler<Loot> Identified;

    public bool Enchantable { get { return maxEnchants > 0; } }
    List<Enchant> enchants = new List<Enchant>();
    int enchantSlots = 0;
    int maxEnchants = 0;
    public LootExtendedInfo ExtendedInfo { get; set; }

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

    public void SetMaterial(EquipmentMaterial material)
    {
      if (material == EquipmentMaterial.Iron ||
         material == EquipmentMaterial.Steel)
      {
        if (this.Material != EquipmentMaterial.Bronze)
        {
          return;//TODO Assert
        }
      }

      var eskToEnhance = EntityStatKind.Unset;
      switch (EquipmentKind)
      {
        case EquipmentKind.Weapon:
          eskToEnhance = EntityStatKind.Attack;
          break;
        //case EquipmentKind.Armor:
        //case EquipmentKind.Helmet:
        //case EquipmentKind.Shield:
        //case EquipmentKind.Glove:
        //  eskToEnhance = EntityStatKind.Defense;
        //  break;
        default:
          break;
      }

      if (eskToEnhance != EntityStatKind.Unset)
      {
        this.Material = material;
        EnhanceStatsDueToMaterial(material);
      }
    }

    protected virtual void EnhanceStatsDueToMaterial(EquipmentMaterial material)
    {
      throw new Exception("EnhanceMaterial!");
    }

    public bool MakeEnchantable(int enchantSlotsToMake = 1)
    {
      if (!IsIdentified)
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
      if (unidentifiedStats != null)//TODO assert
      {
        ExtendedInfo.Stats.Accumulate(unidentifiedStats);
        IncreasePriceBasedOnExtInfo();
      }
      if (Identified != null)
        Identified(this, this);

      return true;
    }

    public EntityStats GetStats()
    {
      EntityStats stats = new EntityStats();
      stats[this.PrimaryStatKind].Accumulate(primaryStat.Value);
      //if (!IsPlain())
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
      PrimaryStatDescription = primaryStat.Kind.ToString() + ": " + PrimaryStatValue;
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
        SetPrimaryStatDesc();
      }
    }

    public void SetPrimaryStat(EntityStatKind primaryStat, float value)
    {
      this.primaryStat = new EntityStat(primaryStat, 0);
      PrimaryStatValue = value;
      this.Name += " of " + primaryStat.ToString();
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetPossibleMagicStats()
    {
      return ExtendedInfo.Stats.GetStats().Where(i => i.Value.Factor == 0).ToList();
    }

    public List<KeyValuePair<EntityStatKind, EntityStat>> GetMagicStats()
    {
      return ExtendedInfo.Stats.GetStats().Where(i => i.Value.Factor > 0).ToList();
    }

    public void SetMagicStat(EntityStatKind statKind, EntityStat stat)
    {
      ExtendedInfo.Stats.SetStat(statKind, stat);
    }

    internal bool IsBetter(Equipment currentEq)
    {
      return Price > currentEq.Price;
    }

    public static List<EntityStatKind> possibleChoicesWeapon = new List<EntityStatKind>() { EntityStatKind.Attack, EntityStatKind.ChanceToHit, EntityStatKind.ColdAttack,
      EntityStatKind.FireAttack,  EntityStatKind.PoisonAttack};

    public static List<EntityStatKind> possibleChoicesWeaponMagician = new List<EntityStatKind>()
    {
      EntityStatKind.Magic, EntityStatKind.Mana, /*EntityStatKind.ManaStealing, uniq*/
      EntityStatKind.ChanceToCastSpell
      //,  EntityStatKind.ChanceToEvadeMeleeAttack, EntityStatKind.ChanceToEvadeMeleeAttack //TODO
    };

    public static List<EntityStatKind> possibleChoicesArmor = new List<EntityStatKind>() { EntityStatKind.Defense, EntityStatKind.ChanceToHit, EntityStatKind.Health,
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

    public void MakeMagic(EntityStatKind stat, int statValue, AddMagicStatReason reason = AddMagicStatReason.Unset)
    {
      AddMagicStat(stat, IsSecondMagicLevel, statValue, false, reason);
    }

    public void MakeMagic(bool magicOfSecondLevel = false)
    {
      //Debug.Assert(levelIndex >= 0);
      var toSkip = GetMagicStats().Select(i => i.Key).ToList();
      toSkip.AddRange(new[] { EntityStatKind.Unset, this.primaryStat.Kind });

      var stat = AddMagicStat(toSkip.ToArray(), false);
      if (magicOfSecondLevel)
      {
        priceAlrIncreased = false;
        toSkip.Add(stat);
        AddMagicStat(toSkip.ToArray(), true);
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

    public bool IsSecondMagicLevel { get { return Class == EquipmentClass.MagicSecLevel; } }
    public EntityStat PrimaryStat { get => primaryStat; set => primaryStat = value; }
    public EntityStats RequiredStats { get => requiredStats; set => requiredStats = value; }
    
    //setter needed from serialize
    public int EnchantSlots { get => enchantSlots; set { enchantSlots = value; } }

    public List<EntityStat> GetEffectiveRequiredStats()
    {
      //TODO too heavy?
      return RequiredStats.GetStats().Where(i => GetReqStatValue(i.Value) > 0).Select(i=> i.Value).ToList();
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
        
    public void MakeMagicSecLevel(EntityStatKind stat, int statValue)
    {
      AddMagicStat(stat, true, statValue, false);
    }

    EntityStats unidentifiedStats;
    public enum AddMagicStatReason { Unset, Enchant};

    void AddMagicStat(EntityStatKind stat, bool secLevel, int statValue, bool incrementFactor = false, AddMagicStatReason reason = AddMagicStatReason.Unset)
    {
      var factorBefore = ExtendedInfo.Stats.GetFactor(stat);
      if (factorBefore > 0 && incrementFactor)
        statValue += statValue;

      //TODO price based on amount of Magic Stat Value
      if (reason != AddMagicStatReason.Enchant)
      {
        SetClass(EquipmentClass.Magic);//we shall not lost that info

        if (unidentifiedStats == null)
        {
          unidentifiedStats = new EntityStats();
          Price = (int)(Price * 1.5f);
        }
        unidentifiedStats.SetFactor(stat, statValue);
        if (Class == EquipmentClass.Magic && secLevel)
          Class = EquipmentClass.MagicSecLevel;
      }
      else
      {
        Price = (int)(Price * 1.5f);
        ExtendedInfo.Stats.SetFactor(stat, statValue);
      }
      IncreasePriceBasedOnExtInfo();
    }

    public virtual void PrepareForSave()
    {
      ExtendedInfo.Stats.PrepareForSave();
      
    }

    public override void HandleGenerationDone()
    {
      //Debug.Assert(this.MinDropDungeonLevel >= 0);
      //if (this.MinDropDungeonLevel >= 0) //set in SetPriceFromLevel
      //{
      //  if(this.MinDropDungeonLevel > 0)
      //    Price = Price * this.MinDropDungeonLevel;
      //}
    }

    public bool priceAlrIncreased;
    
    public void IncreasePriceBasedOnExtInfo()
    {
      if(!IsIdentified)
        return;
      if (priceAlrIncreased)
      {
        //Debug.Assert(false, "priceAlrIncreased");
        return;
      }
      basePrice = Price;
      foreach (var st in ExtendedInfo.Stats.GetStats())
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

    //set based of the Dungeon Level it was dropped on, or if not applicable the enemy or chest level
    int levelIndex = -1;

    public void SetUnique(EntityStats lootStats, int lootLevel)
    {
      SetClass(EquipmentClass.Unique, lootLevel, lootStats);
      Material = EquipmentMaterial.Unset;
    }

    public int MinDropDungeonLevel { get { return levelIndex; } }

    public int LevelIndex
    {
      get { return levelIndex; }
    }

    public EntityStats UnidentifiedStats { get => unidentifiedStats; set => unidentifiedStats = value; }
    public int MaxEnchants { get => maxEnchants; set => maxEnchants = value; }

    public void SetLevelIndex(int li) 
    {
      if (li <= 0)
        throw new Exception("Eq SetLevelIndex = 0!");
      levelIndex = li;
      if (RequiredLevel < li)
        RequiredLevel = li;

      if(Class != EquipmentClass.Unique)
        SetPriceFromLevel();
    }

    public virtual void SetClass(EquipmentClass _class, int levelIndex, EntityStats lootStats = null, bool magicOfSecondLevel = false)
    {
      SetLevelIndex(levelIndex);
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
      }
      IncreasePriceBasedOnExtInfo();
    }

    private void SetClass(EquipmentClass _class)
    {
      Class = _class;
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
        res += " Kind:"+this.EquipmentKind + " Lvl:" + levelIndex;

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

    public bool Enchant(EntityStatKind[] kinds, int val, Enchanter enchantSrc, out string error)
    {
      error = "";
      if (Class == EquipmentClass.Unique && !(this is Trophy))
      {
        error = "Unique item can not be enchanted";
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
        Price += (int)(GetPriceForFactor(kind, val));// *.9f);//crafted loot - price too hight comp to uniq.
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
      juwell.SetLevelIndex(1);//TODO
      juwell.Price = 10;
      juwell.IsPendant = true;
      return juwell;
    }

    public override bool IsCraftable()
    {
      return base.IsCraftable() || Enchantable;
    }

    public void SetPriceFromLevel()
    {
      Price = (LevelIndex + 1) * 15;//TODO
    }
  }
}
