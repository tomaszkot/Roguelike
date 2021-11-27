using Roguelike.Abilities;
using Roguelike.Abstract.Inventory;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Discussions;
using Roguelike.Effects;
using Roguelike.Events;
using Roguelike.Extensions;
using Roguelike.Generators;
using Roguelike.LootContainers;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.Spells;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public enum AllyKind { Unset, Hound, Enemy, Merchant, Paladin }
  public enum EntityProffesionKind { Unset, King, Prince, Knight, Priest, Mercenary, Merchant, Peasant, Bandit, Adventurer, Slave }
  public enum EntityGender { Unset, Male, Female }
  public enum RelationToHeroKind { Unset, Neutral, Like, Dislike, Hate };

  public class RelationToHero
  {
    public RelationToHeroKind Kind { get; set; }
    public int CheatingCounter { get; set; }
  }

  public class AdvancedLivingEntity : LivingEntity, IPersistable, IEquipable, IAdvancedEntity
  {
    public RelationToHero RelationToHero { get; set; } = new RelationToHero();
    public bool HasUrgentTopic { get; set; }
    public Discussion Discussion 
    { 
      get => discussion;
      set => discussion = value;
    }
    public EntityProffesionKind Proffesion { get; set; }
    public event EventHandler ExpChanged;
    public event EventHandler<bool> UrgentTopicChanged;
    public event EventHandler StatsRecalculated;
    public event EventHandler LeveledUp;
    protected CurrentEquipment currentEquipment;
    protected Inventory inventory = null;

    public virtual Inventory Inventory
    {
      get => inventory;
      set
      {
        inventory = value;
        inventory.Owner = this;
      }
    }
    //[JsonIgnoreAttribute]
    public CurrentEquipment CurrentEquipment
    {
      get => currentEquipment;
      set
      {
        currentEquipment = value;
        currentEquipment.Owner = this;
        currentEquipment.EquipmentChanged += OnEquipmentChanged;
      }
    }
    public event EventHandler<EntityStatKind> StatLeveledUp;
    public event EventHandler<int> GoldChanged;

    public double Experience { get; private set; }
    public double NextLevelExperience { get; set; }

    int gold;
    public int Gold
    {
      get { return gold; }
      set
      {
        gold = value;
        if (GoldChanged != null)
          GoldChanged(this, gold);
      }
    }

    //public int AvailableExpPoints { get; set; } = 3;
    protected bool canAdvanceInExp = false;
    int levelUpPoints = 0;

    Dictionary<SpellKind, int> coolingDownSpells = new Dictionary<SpellKind, int>();
    Abilities.AbilitiesSet abilities = new Abilities.AbilitiesSet();

    public int AbilityPoints { get; set; }

    public int LevelUpPoints
    {
      get
      {
        return levelUpPoints;
      }

      set
      {
        levelUpPoints = value;
      }
    }

    public bool InventoryAcceptsItem(Inventory inv, Loot loot, AddItemArg addItemArg)
    {
      if (inv is CurrentEquipment)
      {
        var eq = loot as Equipment;
        if (eq == null)
          return false;

        if (!CanUseEquipment(eq, false))
          return false;

        if (addItemArg == null)
          return false;

        var currArgs = addItemArg as CurrentEquipmentAddItemArg;
        if (currArgs == null)
          return false;

        var slotEK = currArgs.cek.GetEquipmentKind();
        return slotEK == eq.EquipmentKind;
      }
      return true;
    }

    public override Container Container
    {
      get { return base.Container; }
      set
      {
        base.Container = value;
        //this.Inventory.Container = value;
      }
    }

    public static Dictionary<Weapon.WeaponKind, EntityStatKind> MalleeWeapons2Esk = new Dictionary<Weapon.WeaponKind, EntityStatKind>()
    {
      {Weapon.WeaponKind.Axe,  EntityStatKind.AxeExtraDamage},
      { Weapon.WeaponKind.Sword, EntityStatKind.SwordExtraDamage},
      { Weapon.WeaponKind.Bashing, EntityStatKind.BashingExtraDamage},
      { Weapon.WeaponKind.Dagger, EntityStatKind.DaggerExtraDamage},

    };

    public static Dictionary<Weapon.WeaponKind, EntityStatKind> ProjectileWeapons2Esk = new Dictionary<Weapon.WeaponKind, EntityStatKind>()
    {
      { Weapon.WeaponKind.Bow, EntityStatKind.BowExtraDamage},
      { Weapon.WeaponKind.Crossbow, EntityStatKind.CrossbowExtraDamage},
      { Weapon.WeaponKind.Staff, EntityStatKind.StaffExtraElementalProjectileDamage},
      { Weapon.WeaponKind.Scepter, EntityStatKind.ScepterExtraElementalProjectileDamage},
      { Weapon.WeaponKind.Wand, EntityStatKind.WandExtraElementalProjectileDamage},
    };

    public AdvancedLivingEntity(Container cont, Point point, char symbol) : base(point, symbol)
    {
      discussion = new Discussion(cont);
      NextLevelExperience = GenerationInfo.FirstNextLevelExperienceThreshold;
      RelationToHero.Kind = RelationToHeroKind.Neutral;
      Container = cont;
      Inventory = cont.GetInstance<Inventory>();
      CurrentEquipment = cont.GetInstance<CurrentEquipment>();
    }

    public virtual bool IsSellable(Loot loot)
    {
      return loot.Price >= 0;
    }

    public bool IncreaseAbility(AbilityKind kind)
    {
      var ab = abilities.GetAbility(kind);
      return Increase(ab);
    }

    private bool Increase(Ability ab)
    {
      var increased = ab.IncreaseLevel(this);
      if (increased)
      {
        AbilityPoints--;
        RecalculateStatFactors(false);
      }

      return increased;
    }

    public LootAbility GetLootAbility()
    {
      return GetPassiveAbility(AbilityKind.LootingMastering) as LootAbility;
    }

    public PassiveAbility GetPassiveAbility(AbilityKind kind)
    {
      return Abilities.PassiveItems.Where(i => i.Kind == kind).SingleOrDefault();
    }

    public ActiveAbility GetActiveAbility(AbilityKind kind)
    {
      return Abilities.ActiveItems.Where(i => i.Kind == kind).SingleOrDefault();
    }

    public int GetPrice(Loot loot)
    {
      int count = 1;

      var price = (int)(loot.Price * Inventory.PriceFactor) * count;
      return price;
    }

    //public new static AdvancedLivingEntity CreateDummy()
    //{
    //  return new AdvancedLivingEntity(new Point(0, 0), '\0');
    //}
    public double CalcExpScale()
    {
      var currExp = Experience - PrevLevelExperience;
      var scale = currExp / (NextLevelExperience - PrevLevelExperience);
      return scale;
    }

    public bool IncreaseExp(double factor)
    {
      bool leveledUp = false;
      Experience += factor;
      bool thresholdReached = Experience >= NextLevelExperience;
      if (thresholdReached && canAdvanceInExp)
      {
        PrevLevelExperience = NextLevelExperience;
        Level++;
        LevelUpPoints += GenerationInfo.LevelUpPoints;
        AbilityPoints += 2;
        NextLevelExperience = Calculated.FactorCalculator.AddFactor((int)NextLevelExperience, 110);
        //NextLevelExperience = (int)(NextLevelExperience + (NextLevelExperience * GenerationInfo.NextExperienceIncrease));
        //if (Level == 2 || Level == 3)
        //  NextLevelExperience *= 1.5f;

        leveledUp = true;

        this.Stats.GetStat(EntityStatKind.Health).SetSubtraction(0);
        this.Stats.GetStat(EntityStatKind.Mana).SetSubtraction(0);

        if (LeveledUp != null)
          LeveledUp(this, EventArgs.Empty);

        AppendAction(new LivingEntityAction() { Kind = LivingEntityActionKind.LeveledUp, Info = Name + " has gained a new level!", InvolvedEntity = this });
      }
      if (ExpChanged != null)
        ExpChanged(this, EventArgs.Empty);
      return leveledUp;
    }

    public void SetSpellCoolingDown(SpellKind kind)
    {
      if (coolingDownSpells.ContainsKey(kind) && coolingDownSpells[kind] > 0)
      {
        //AppendAction("SpellKind already collingdown!" + kind);
        return;
      }

      coolingDownSpells[kind] = ActiveManaPoweredSpellSource.CreateSpell(this).CoolingDown;
    }

    public void Consume(IConsumable consumable)
    {
      if (inventory.Contains(consumable.Loot))
      {
        if (consumable.StatKind == EntityStatKind.Unset)
        {
          var pot = consumable.Loot as Potion;
          Debug.Assert(pot != null && pot.Kind == PotionKind.Poison);
        }

        var stacked = consumable.Loot as StackedLoot;
        inventory.Remove(stacked);

        if (consumable is SpecialPotion)
        {
          var sp = consumable as SpecialPotion;
          this.Stats[sp.StatKind].Nominal += (float)consumable.StatKindEffective.Value;
        }
        else
        {
          if (consumable is Potion potion)
          {
            Debug.Assert(consumable.ConsumptionSteps == 1);
            if (potion.Kind == PotionKind.Poison)
            {
              var le = GetFirstLastingEffect(EffectType.Poisoned);
              if (le != null)
                RemoveLastingEffect(le);
            }
            else
            {
              var factor = LastingEffectsSet.CalcLastingEffectInfo(EffectType.Unset, consumable);
              DoConsume(consumable.StatKind, factor);
            }
          }
          else
          {
            if (consumable is Hooch hooch)
            {
              LastingEffectsSet.AddPercentageLastingEffect(EffectType.Hooch, consumable, consumable.Loot);
            }
            else
            {
              EffectType et = EffectType.ConsumedRawFood;
              if (consumable.Roasted)
                et = EffectType.ConsumedRoastedFood;
              LastingEffectsSet.AddPercentageLastingEffect(et, consumable, consumable.Loot);
            }
          }
        }

        var info = Name + " consumed " + (consumable as Dungeons.Tiles.Tile).Name + ", Health: " + this.GetCurrentValue(EntityStatKind.Health);
        AppendAction(new LootAction(consumable.Loot, this) { Kind = LootActionKind.Consumed, Info = info });
      }
      else
        Assert(false);
    }

    public void IncreaseStatByLevelUpPoint(EntityStatKind stat)
    {
      if (LevelUpPoints == 0)
        return;
      this.Stats[stat].Nominal += 1;
      LevelUpPoints--;
      RecalculateStatFactors(false);//Attack depends on Str
      EmitStatsLeveledUp(stat);
    }

    public void EmitStatsLeveledUp(EntityStatKind stat)
    {
      if (StatLeveledUp != null)
        StatLeveledUp(this, stat);
    }

    public virtual string GetFormattedStatValue(EntityStatKind kind, bool round)
    {
      float currentValue = GetStatForDisplay(kind, round);
      var stat = Stats.GetStat(kind);
      var value = stat.GetFormattedCurrentValue(currentValue);

      return value;
    }

    public float GetStatForDisplay(EntityStatKind kind, bool round)
    {
      var currentValue = GetCurrentValue(kind);

      return GetForDisplay(round, currentValue);
    }

    public static float GetForDisplay(bool round, float currentValue)
    {
      if (round)
        currentValue = currentValue.Rounded();
      return currentValue;
    }

    public double PrevLevelExperience { get; private set; }
    CurrentEquipment IEquipable.CurrentEquipment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public AbilitiesSet Abilities { get => abilities; set => abilities = value; }

    //protected virtual void CreateInventory(Container container)
    //{
    //  var inv = new Inventory(container);

    //  OnCreateInventory(container, inv);
    //}

    //protected void OnCreateInventory(Container container, Inventory inv)
    //{
    //  this.Inventory = inv;
    //  this.Inventory.Owner = this;
    //  var currentEquipment = new CurrentEquipment(container);
    //  currentEquipment.Owner = this;
    //  CurrentEquipment = currentEquipment;
    //}

    public bool MoveEquipmentCurrent2Inv(Equipment eq, CurrentEquipmentPosition pos)
    {
      return MoveEquipmentCurrent2Inv(eq, eq.EquipmentKind, pos);
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq, EquipmentKind ek, CurrentEquipmentPosition pos)
    {
      var cek = ek.GetCurrentEquipmentKind(pos);
      return MoveEquipmentCurrent2Inv(eq, cek);
    }

    public static CurrentEquipmentPosition DefaultCurrentEquipmentPosition = CurrentEquipmentPosition.Left;
    private Discussion discussion;

    public bool HandleEquipmentFound(Equipment eq)
    {
      if (!eq.IsIdentified)
        return false;

      var activeSet = GetActiveEquipment();//primary or secondary 
      var cep = CurrentEquipmentPosition.Unset;
      if (eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        cep = CurrentEquipmentPosition.Left;

      var cek = Equipment.FromEquipmentKind(eq.EquipmentKind, cep);
      var currentEq = activeSet[cek];
      if (currentEq != null)
      {
        if (eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        {
          cep = CurrentEquipmentPosition.Right;
          cek = Equipment.FromEquipmentKind(eq.EquipmentKind, cep);
          var currentEqRight = activeSet[cek];
          if (currentEqRight == null)
          {
            currentEq = null;
            cep = CurrentEquipmentPosition.Right;
          }
        }
      }

      if (
          CanUseEquipment(eq, true) &&
          (currentEq == null || (eq.IsBetter(currentEq)) && Options.Instance.Mechanics.AutoPutOnBetterEquipment)
        )
      {
        if (currentEq != null)
        {
          if (!MoveEquipmentCurrent2Inv(currentEq, cep))
            return false;
        }
        var destKind = Equipment.FromEquipmentKind(eq.EquipmentKind, cep);
        return MoveEquipmentInv2Current(eq, destKind);
      }

      return false;
    }

    public bool CanUseEquipment(Equipment eq, bool autoPutoOn)
    {
      if (!eq.IsIdentified)
        return false;

      if (Level < eq.RequiredLevel)
        return false;
      foreach (var rs in eq.GetEffectiveRequiredStats())
      {
        if (rs.Value.Nominal > Stats.GetNominal(rs.Kind))
          return false;
      }

      if (eq.EquipmentKind == EquipmentKind.Weapon || eq.EquipmentKind == EquipmentKind.Shield)
      {
        var wpn = GetActiveEquipment()[CurrentEquipmentKind.Weapon] as Weapon;
        var shield = GetActiveEquipment()[CurrentEquipmentKind.Shield];
        if
        (
          eq.EquipmentKind == EquipmentKind.Weapon && shield != null ||
          eq.EquipmentKind == EquipmentKind.Shield && wpn != null
        )
        {
          if (eq.EquipmentKind == EquipmentKind.Shield &&
            (wpn.Kind == Weapon.WeaponKind.Bow || wpn.Kind == Weapon.WeaponKind.Crossbow)
            )
            return false;

          if (!autoPutoOn &&
              eq is Weapon wpnToUSe &&
              (wpnToUSe.Kind == Weapon.WeaponKind.Bow || wpnToUSe.Kind == Weapon.WeaponKind.Crossbow) &&
              shield != null
            )
          {
            var ev = new GameEvent("Can not wield that weapon when using a shield", ActionLevel.Important);
            ev.ShowHint = true;
            AppendAction(ev);
            return false;
          }
        }
      }

      return true;
    }

    public bool MoveEquipmentInv2Current(Equipment eq,
                              CurrentEquipmentKind cek, bool primary = true)
    {
      bool removed = inventory.Remove(eq);
      if (removed)
      {
        var set = SetEquipment(eq, cek, primary);

        return set;
      }
      else
      {
        bool reset = inventory.Add(eq);
        Assert(reset, "from.Add(eq)");
      }

      return removed;
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq, CurrentEquipmentKind cek, bool primary = true)
    {
      bool done = SetEquipment(null, cek, primary);
      if (done)
      {
        //done = inventory.Add(eq);
        //if (!done)
        //{
        //  //revert!
        //  if (primary)
        //    CurrentEquipment.PrimaryEquipment[cek] = eq;
        //  else
        //    CurrentEquipment.SpareEquipment[cek] = eq;
        //}
      }

      return done;
    }

    public Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment()
    {
      return CurrentEquipment.GetActiveEquipment();
    }

    //TODO make it priv. this is a dangerous method!, as not guarating user do not have same eq in the inventory. MoveEquipmentInv2Current shall be used.
    public bool SetEquipment(Equipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset, bool primary = true)
    {
      if (!CurrentEquipment.EnsureCurrEqKind(eq, ref cek))
        return false;
      if (CurrentEquipment.GetActiveEquipment()[cek] != null)
      {
        var prev = this.CurrentEquipment.GetActiveEquipment()[cek];
        if (!inventory.Add(prev))
        {
          Assert(false, "from.Add(prev)");
          return false;
        }
        if (!this.CurrentEquipment.SetEquipment(null, cek, primary))
          return false;
        if (eq == null)
          return true;
      }
      var set = CurrentEquipment.SetEquipment(eq, cek, primary);
      if (!set)
        return false;

      //SetSpellSourceFromWeapon(eq);

      return true;
    }

    public Weapon GetActiveWeapon()
    {
      var currentEquipment = GetActiveEquipment();
      return currentEquipment[CurrentEquipmentKind.Weapon] as Weapon;
    }

    public virtual SpellSource ActiveWeaponSpellSource
    {
      get
      {
        var wpn = GetActiveWeapon();
        return wpn != null ? wpn.SpellSource : null;
      }

    }

    //public IProjectile ActiveProjectile
    //{
    //  get 
    //  {
    //    if (ActiveSpellSource != null)
    //      return ActiveSpellSource.;
    //    if (ActiveFightItem != null)
    //      return ActiveFightItem;

    //    return null;
    //  }
    //}

    public SpellSource ActiveSpellSource
    {
      get
      {
        var spellSrc = ActiveManaPoweredSpellSource;
        if (spellSrc != null)
          return spellSrc;

        var fi = ActiveFightItem;
        if (fi != null)
          return null;

        var wpn = GetActiveWeapon();
        if (wpn != null)
          return wpn.SpellSource;

        return null;
      }
    }

    private void OnEquipmentChanged(object sender, EquipmentChangedArgs args)//)
    {
      Equipment eq = args.Equipment;
      CurrentEquipmentKind cek = args.CurrentEquipmentKind;
      RecalculateStatFactors(false);

      LootAction ac = null;
      CurrentEquipmentPosition pos;
      if (eq != null)
      {
        ac = new LootAction(eq, this)
        {
          Info = Name + " put on " + eq.Name,
          Kind = LootActionKind.PutOn,
          EquipmentKind = eq.EquipmentKind,
          CurrentEquipmentKind = cek
        };
      }
      else
      {
        ac = new LootAction(null, this)
        {
          Info = Name + " took off " + cek,
          Kind = LootActionKind.PutOff,
          EquipmentKind = Equipment.FromCurrentEquipmentKind(cek, out pos),
          CurrentEquipmentKind = cek
        };
      }
      AppendAction(ac);
    }

    public void RecalculateStatFactors(bool fromLoad)
    {
      Stats.ResetStatFactors();
      if (fromLoad)//this shall not be affected by any after load
      {
        //Stats.GetStat(EntityStatKind.ChanceToMeleeHit).SetSubtraction(0);
        //Stats.GetStat(EntityStatKind.ChanceToPhysicalProjectileHit).SetSubtraction(0);
        //Stats.GetStat(EntityStatKind.ChanceToElementalProjectile).SetSubtraction(0);
        //Stats.GetStat(EntityStatKind.Defense).SetSubtraction(0);
        //Stats.GetStat(EntityStatKind.Attack).SetSubtraction(0);
      }

      //accumulate positive factors
      AccumulateEqFactors(true);

      var si = GetStrengthIncrease();
      Stats.AccumulateFactor(EntityStatKind.MeleeAttack, si);
      var abs = Abilities.PassiveItems;
      foreach (var ab in abs)
      {
        if (!ab.BeginTurnApply)
        {
          if (ab.PrimaryStat.Kind != EntityStatKind.Unset)
          {
            if (ab.PrimaryStat.Factor != 0)
              Stats.AccumulateFactor(ab.PrimaryStat.Kind, ab.PrimaryStat.Factor);
            if (ab.AuxStat.Factor != 0)
              AddAuxStat(ab);
          }
        }
      }

      AccumulateEqFactors(false);

      if (StatsRecalculated != null)
        StatsRecalculated(this, EventArgs.Empty);
    }

    private void AddAuxStat(PassiveAbility ab)
    {
      //if (ab.Kind == AbilityKind.we.AuxStat.Kind == EntityStatKind.AxeExtraDamage
      //    || ab.AuxStat.Kind == EntityStatKind.SwordExtraDamage
      //    || ab.AuxStat.Kind == EntityStatKind.BashingExtraDamage
      //    || ab.AuxStat.Kind == EntityStatKind.DaggerExtraDamage

      //                )
      if (ab.AuxStat.Factor != 0)
      {
        Stats.AccumulateFactor(ab.AuxStat.Kind, ab.AuxStat.Factor);
      }
    }

    protected virtual float GetStrengthIncrease()
    {
      return Stats.GetCurrentValue(EntityStatKind.Strength) - StartStrength;
    }

    public bool CanUseEquipment(Equipment eq, EntityStat eqStat)
    {
      return Stats.GetNominal(eqStat.Kind) >= eq.GetReqStatValue(eqStat);
    }

    public override string ToString()
    {
      return base.ToString();
    }

    private void AccumulateEqFactors(bool positive)
    {
      var eqipKinds = Enum.GetValues(typeof(CurrentEquipmentKind)).Cast<CurrentEquipmentKind>();

      foreach (var ek in eqipKinds)
      {
        if (currentEquipment.SpareEquipmentUsed.ContainsKey(ek))//old game save ?
        {
          bool spareUsed = currentEquipment.SpareEquipmentUsed[ek];
          var eq = spareUsed ? currentEquipment.SpareEquipment[ek] : currentEquipment.PrimaryEquipment[ek];
          if (eq != null && eq.IsIdentified)
          {
            var stats = eq.GetStats();
            Stats.AccumulateFactors(stats, positive);
            //Stats.AccumulateFactors(eq.ExtendedInfo.Stats, positive); done in eq.GetStats
          }
        }
      }
    }

    public void SetHasUrgentTopic(bool ut)
    {
      this.HasUrgentTopic = ut;
      if (UrgentTopicChanged != null)
        UrgentTopicChanged(this, HasUrgentTopic);
    }

    public virtual void ApplyAbilities()
    {
      var toApply = Abilities.PassiveItems.Where(i => i.BeginTurnApply && i.Level > 0).ToList();
      foreach (var ab in toApply)
      {
        if (ab.Kind == AbilityKind.RestoreHealth ||
          ab.Kind == AbilityKind.RestoreMana)
        {
          var entityStatKind = EntityStatKind.Unset;

          if (ab.Kind == AbilityKind.RestoreHealth)
          {
            entityStatKind = EntityStatKind.Health;
          }
          else
            entityStatKind = EntityStatKind.Mana;
          var stat = Stats.GetStat(entityStatKind);
          var factor = stat.Value.Subtracted;

          if (factor > 0 && Math.Abs(factor) > 0.001)
          {
            var inc = ab.PrimaryStat.Factor;
            var val = stat.Value.Nominal * inc / 100f;
            Stats.IncreaseStatFactor(entityStatKind, val);
            //GameManager.Instance.AppendDiagnosticsUnityLog("restored " + entityStatKind + " " + val);
          }
        }
      }
    }

    public Equipment GetCurrentEquipment(EquipmentKind ek)
    {
      var cek = Equipment.FromEquipmentKind(ek, DefaultCurrentEquipmentPosition);
      return GetActiveEquipment()[cek] as Weapon;
    }

    private bool CurrentWeaponCausesStunning()
    {
      var wpn = GetCurrentEquipment(EquipmentKind.Weapon) as Weapon;
      return wpn.Kind == Weapon.WeaponKind.Bashing;
    }

    private bool CurrentWeaponCausesBleeding()
    {
      var wpn = GetCurrentEquipment(EquipmentKind.Weapon) as Weapon;
      return wpn.Kind == Weapon.WeaponKind.Axe || wpn.Kind == Weapon.WeaponKind.Dagger || wpn.Kind == Weapon.WeaponKind.Sword;
    }

    protected override LastingEffect EnsurePhysicalHitEffect(float inflicted, LivingEntity victim)
    {
      LastingEffect lastingEffectCalcInfo = null;
      var wpn = this.GetCurrentEquipment(EquipmentKind.Weapon) as Weapon;
      if (wpn != null)
      {
        if (CurrentWeaponCausesBleeding() && CalculateIfStatChanceApplied(EntityStatKind.ChanceToCauseBleeding, victim))
          lastingEffectCalcInfo = victim.LastingEffectsSet.EnsureEffect(EffectType.Bleeding, inflicted / 3, this);
        //if (fi == null)//throwing knife will not cause stunning or tear apart
        {
          if (CurrentWeaponCausesStunning() && CalculateIfStatChanceApplied(EntityStatKind.ChanceToCauseStunning))
            lastingEffectCalcInfo = victim.LastingEffectsSet.EnsureEffect(EffectType.Stunned, 0, this);
          if (victim.Stats.Health < victim.Stats.GetNominal(EntityStatKind.Health) * 2 / 3)
          {
            if (CalculateIfStatChanceApplied(EntityStatKind.ChanceToCauseTearApart))
              lastingEffectCalcInfo = victim.LastingEffectsSet.EnsureEffect(EffectType.TornApart, 0, this);//this is a death          
          }
          //swords does not have any effect by default(beside unique ones), but have high hit %
        }
      }
      return lastingEffectCalcInfo;
    }

    public virtual bool GetGoldWhenSellingTo(IInventoryOwner dest)
    {
      return this != dest;
    }

    public static readonly Dictionary<EnemyPowerKind, double> EnemyDamagingTotalExpAward = new Dictionary<EnemyPowerKind, double>()
    {
      { EnemyPowerKind.Plain, 30 },
      { EnemyPowerKind.Champion, 150 },
      { EnemyPowerKind.Boss, 500 },
    };
    /// <summary>
    /// 
    /// </summary>
    /// <param name="inflicted"></param>
    /// <param name="victim"></param>
    protected override void OnDamageCaused(float inflicted, LivingEntity victim)
    {
      double exp = 1f;
      if (victim is Enemy en)
      {
        var livePercentage = inflicted / en.GetTotalValue(EntityStatKind.Health) * 100;
        var award = EnemyDamagingTotalExpAward[en.PowerKind];
        exp = livePercentage * award / 100;
      }
      var inc = (1 * victim.Level * exp);
      this.IncreaseExp(inc);
    }

    public string GetExpInfo()
    {
      return (int)Experience + "/" + (int)NextLevelExperience;
    }

    public override void RemoveFightItem(FightItem fi)
    {
      Inventory.Remove(fi);
    }

    public override AttackDescription GetAttackValue(AttackKind attackKind)
    {
      var wpn = this.GetActiveWeapon();
      return new AttackDescription(this, wpn != null ? !wpn.StableDamage : true, attackKind);
    }
  }
}
