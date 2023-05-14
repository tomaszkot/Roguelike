﻿using Roguelike.Abilities;
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
using Roguelike.Tiles.Looting;
using Roguelike.Tiles.Abstract;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Roguelike.Abstract.Projectiles;
using Roguelike.Managers;

namespace Roguelike.Tiles.LivingEntities
{
  public enum AllyKind { Unset, Hound, Enemy, Merchant, Paladin }
  public enum EntityProffesionKind { Unset, King, Prince, Knight, Priest, Mercenary, Merchant, Peasant, Bandit, Adventurer, Slave }
  public enum EntityGender { Unset, Male, Female }
  public enum RelationToHeroKind { Unset, Neutral, Friendly, Antagonistic, Hostile };
  public enum EntityKind { Unset, Human, Animal, Undead, Daemon }
  public enum AnimalKind { Unset, Hound, Pig, Hen, Rooster, Deer }

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
        public ProjectileFightItem ActiveProjectileFightItem => ActiveFightItem as ProjectileFightItem;

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

    public double Experience { get; set; }
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

    Abilities.AbilitiesSet abilities = new Abilities.AbilitiesSet();
    SpellStateSet spellStatesSet = new SpellStateSet();

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

    public bool IncreaseSpell(SpellKind sk)
    {
      var state = this.Spells.GetState(sk);
      if (state.CanIncLevel(this))
      {
        state.Level++;
        AbilityPoints--;
        return true;
      }

      return false;
    }

    public bool InventoryAcceptsItem(Inventory inv, Loot loot, AddItemArg addItemArg)
    {
      if (inv is CurrentEquipment)
      {
        var eq = loot as IEquipment;
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

    public AdvancedLivingEntity(Container cont, Point point, char symbol) : base(point, symbol, cont)
    {
      discussion = cont.GetInstance<Discussion>();
      NextLevelExperience = GenerationInfo.FirstNextLevelExperienceThreshold;
      RelationToHero.Kind = RelationToHeroKind.Neutral;
      Container = cont;
      Inventory = cont.GetInstance<Inventory>();
      CurrentEquipment = cont.GetInstance<CurrentEquipment>();
    }

    public virtual bool IsSellable(Loot loot)
    {
      return loot.IsSellable();
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

    public override ActiveAbility GetActiveAbility(AbilityKind kind)
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
        NextLevelExperience = Calculated.FactorCalculator.AddFactor((int)NextLevelExperience, 110);
        Level++;
        if (Level == 2)
        {
          NextLevelExperience = NextLevelExperience * 1.3f;
        }
        LevelUpPoints += GenerationInfo.LevelUpPoints;
        AbilityPoints += 2;

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
      var spell = spellStatesSet.GetState(kind);
      if (spell.IsCoolingDown())//coolingDownSpells.ContainsKey(kind) && coolingDownSpells[kind] > 0)
      {
        //AppendAction("SpellKind already collingdown!" + kind);
        return;
      }

      spell.CoolDownCounter = ActiveManaPoweredSpellSource.CreateSpell(this).CoolingDown;
    }

    public override void Consume(IConsumable consumable)
    {
      if (inventory.Contains(consumable.Loot))
      {
        var stacked = consumable.Loot as StackedLoot;
        var rem = inventory.Remove(stacked);
        if (rem == null)
        {
          Assert(false);
          return;
        }
      }
      base.Consume(consumable);
    }

    public void IncreaseStatByLevelUpPoint(EntityStatKind stat)
    {
      if (LevelUpPoints == 0)
        return;
      this.Stats[stat].Nominal += 1;
      if (stat == EntityStatKind.Magic)
        this.Stats[EntityStatKind.Mana].Nominal += 1;
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

    public bool MoveEquipmentCurrent2Inv(IEquipment eq, CurrentEquipmentPosition pos)
    {
      return MoveEquipmentCurrent2Inv(eq, eq.EquipmentKind, pos);
    }

    public bool MoveEquipmentCurrent2Inv(IEquipment eq, EquipmentKind ek, CurrentEquipmentPosition pos)
    {
      var cek = ek.GetCurrentEquipmentKind(pos);
      return MoveEquipmentCurrent2Inv(eq, cek);
    }
        
    public static CurrentEquipmentPosition DefaultCurrentEquipmentPosition = CurrentEquipmentPosition.Left;
    private Discussion discussion;

    public bool HandleEquipmentFound(IEquipment eq)
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

    virtual protected bool CanUseAnimalKindEq(IEquipment eq)
    {
      return false;
    }

    public virtual bool CanUseEquipment(IEquipment eq, bool autoPutoOn)
    {
      Func<string, bool> report = (string message) =>
      {
        var ev = new GameEvent(message, ActionLevel.Important);
        ev.ShowHint = !autoPutoOn;
        AppendAction(ev);
        return false;
      };

      if (!eq.IsIdentified)
        return report("Item is not identified");

      if (Level < eq.RequiredLevel)
        return report("Required Level too high");

      if (eq is ProjectileFightItem proj && proj.FightItemKind != FightItemKind.ThrowingTorch)
        return false;

      if (eq.MatchingAnimalKind != AnimalKind.Unset && !CanUseAnimalKindEq(eq))
      {
        return report("Can not use Animal's equipment");
      }

      foreach (var rs in eq.GetEffectiveRequiredStats())
      {
        if (rs.Value.Nominal > Stats.GetNominal(rs.Kind))
          return report("Required statistic " + rs.Kind.ToDescription() + " not met.");
      }

      if (autoPutoOn && eq is Weapon wpnBowLike && wpnBowLike.IsBowLike)
      {
        var pfi = GetFightItemKindAmmo(wpnBowLike);
        if (pfi == null || pfi.Count == 0)
          return false;
      }

      if (eq.EquipmentKind == EquipmentKind.Weapon || eq.EquipmentKind == EquipmentKind.Shield)
      {
        var wpn = GetActiveEquipment(CurrentEquipmentKind.Weapon) as Weapon;
        var shield = GetActiveEquipment(CurrentEquipmentKind.Shield);
        if
        (
          eq.EquipmentKind == EquipmentKind.Weapon && shield != null ||
          eq.EquipmentKind == EquipmentKind.Shield && wpn != null
        )
        {
          if (eq.EquipmentKind == EquipmentKind.Shield &&
            (wpn.Kind == Weapon.WeaponKind.Bow || wpn.Kind == Weapon.WeaponKind.Crossbow)
            )
            return report("Can not wield a shield when using a " + wpn.Kind.ToDescription());

          if (
              eq is Weapon wpnToUSe &&
              (wpnToUSe.Kind == Weapon.WeaponKind.Bow || wpnToUSe.Kind == Weapon.WeaponKind.Crossbow) &&
              shield != null
            )
          {
            if (!autoPutoOn)
            {
              return report("Can not wield that weapon when using a shield");
            }
            return false;
          }
        }
      }

      return true;
    }


    public bool MoveEquipmentInv2Current
    (
      IEquipment eq,
      CurrentEquipmentKind cek
    )
    {
      var loot = eq as Loot;
      var sl = loot as StackedLoot;
      RemoveItemArg arg = null;
      if (sl != null)
      {
        arg = new RemoveItemArg();
        arg.StackedCount = sl.Count;
      }
      var removed = inventory.Remove(loot, arg) != null;
      if (removed)
      {
        if (sl != null)
          eq.Count = arg.StackedCount;
        var set = SetEquipment(eq, cek);
        //SyncShortcutsBarStackedLoot();
        return set;
      }
      else
      {
        bool reset = inventory.Add(loot);
        Assert(reset, "from.Add(eq)");
      }

      return removed;
    }

    public bool MoveEquipmentCurrent2Inv(IEquipment eq, CurrentEquipmentKind cek)
    {
      bool primary = CurrentEquipment.SpareEquipmentUsed[cek] ? false : true;
      if (primary && CurrentEquipment.PrimaryEquipment[cek] != eq)
        return false;
      else if (!primary && CurrentEquipment.SpareEquipment[cek] != eq)
        return false;

      bool done = SetEquipment(null, cek);
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
        //SyncShortcutsBarStackedLoot();
      }

      return done;
    }

    public IEquipment GetActiveEquipment(CurrentEquipmentKind cek)
    {
      return GetActiveEquipment()[cek];
    }

    public Dictionary<CurrentEquipmentKind, IEquipment> GetActiveEquipment()
    {
      return CurrentEquipment.GetActiveEquipment();
    }

    public bool RemoveEquipment(Loot eq, CurrentEquipmentKind cek)
    {
      var removed = SetEquipment(null, cek);//shall move it to inv
      if (removed)
      {
        if (Inventory.Contains(eq))
          Inventory.Remove(eq);
      }

      return removed;
    }

    //Pivate as this is a dangerous method!, not guarating user have this eq in the inventory.
    bool SetEquipment(IEquipment eq, CurrentEquipmentKind cek = CurrentEquipmentKind.Unset)
    {
      if (!CurrentEquipment.EnsureCurrEqKind(eq, ref cek))
        return false;

      var primary = CurrentEquipment.SpareEquipmentUsed[cek] ? false : true;

      if (CurrentEquipment.GetActiveEquipment(cek) != null)
      {
        var prev = CurrentEquipment.GetActiveEquipment(cek);
        if (!inventory.Add(prev as Loot))
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

    public override Weapon GetActiveWeapon()
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

    public SpellSource ActiveSpellSource
    {
      get
      {
        var spellSrc = ActiveManaPoweredSpellSource;
        if (spellSrc != null)
          return spellSrc;

        var fi = ActiveFightItem;
        if (fi != null && GetStackedCountForHotBar(fi) > 0)
        {
          return null;
        }

        var wpn = GetActiveWeapon();
        if (wpn != null)
          return wpn.SpellSource;

        return null;
      }
    }

    private void OnEquipmentChanged(object sender, EquipmentChangedArgs args)//)
    {
      var eq = args.Equipment;
      CurrentEquipmentKind cek = args.CurrentEquipmentKind;
      RecalculateStatFactors(false);

      LootAction ac = null;
      CurrentEquipmentPosition pos;
      if (eq != null)
      {
        ac = new LootAction(eq as Loot, this)
        {
          Info = Name + " put on " + eq.Name,
          Kind = LootActionKind.PutOn,
          EquipmentKind = eq.EquipmentKind,
          CurrentEquipmentKind = cek
        };
      }
      else
      {
        string ofDesc = cek.ToString();
        if (args.Removed is ProjectileFightItem pfi)
        {
          ofDesc = pfi.FightItemKind.ToDescription();
        }
        ac = new LootAction(null, this)
        {
          Info = Name + " took off " + ofDesc,
          Kind = LootActionKind.PutOff,
          EquipmentKind = Equipment.FromCurrentEquipmentKind(cek, out pos),
          CurrentEquipmentKind = cek
        };
      }
      if(ac!=null)
        AppendAction(ac);
    }

    public void RecalculateStatFactors(bool fromLoad)
    {
      Stats.ResetStatFactors();

      //accumulate positive factors
      AccumulateEqFactors(true);

      //tested by TestSkeletonAttackIncWithStrength
      AlignMeleeAttack();

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

      //TODO why only torch ?
      var torch = Abilities.GetAbility(AbilityKind.ThrowingTorch);
      Stats.AccumulateFactor(torch.PrimaryStat.Kind, torch.PrimaryStat.Factor);

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

    //protected virtual float GetStrengthIncrease()
    //{
    //  return Stats.GetCurrentValue(EntityStatKind.Strength) - StartStrength;
    //}

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
        if (currentEquipment.SpareEquipmentUsed.ContainsKey(ek))
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
        if (ab.Kind == AbilityKind.RestoreHealth || ab.Kind == AbilityKind.RestoreMana)
        {
          var entityStatKind = EntityStatKind.Unset;

          if (ab.Kind == AbilityKind.RestoreHealth)
            entityStatKind = EntityStatKind.Health;
          else
            entityStatKind = EntityStatKind.Mana;

          var stat = Stats.GetStat(entityStatKind);
          var factor = stat.Value.Subtracted;

          if (factor > 0 && Math.Abs(factor) > 0.001)
          {
            //var inc = .Factor;
            //var val = stat.Value.Nominal * inc / 100f;
            Stats.IncreaseDynamicStatValue(entityStatKind, ab.PrimaryStat);
            //GameManager.Instance.AppendDiagnosticsUnityLog("restored " + entityStatKind + " " + val);
          }
        }
      }

      foreach (var ab in Abilities.ActiveItems)
      {
        if (ab.CoolDownCounter > 0)
        {
          ab.CoolDownCounter--;
          
        }
      }
    }

    public Equipment GetCurrentEquipment(EquipmentKind ek)
    {
      var cek = Equipment.FromEquipmentKind(ek, DefaultCurrentEquipmentPosition);
      return GetActiveEquipment()[cek] as Equipment;
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

    public override bool HasFightItem(FightItemKind fik)
    {
      return GetFightItem(fik)!=null;
    }

    /// <summary>
    /// User can throw both from hand and Inventory
    /// </summary>
    /// <param name="sl"></param>
    /// <returns></returns>
    public virtual int GetStackedCountForHotBar(StackedLoot sl)
    {
      var sc = 0;
      if (sl is FightItem pfi && pfi.FightItemKind == FightItemKind.ThrowingTorch)
      {
        var ci = GetFightItemFromCurrentEq();
        sc =  ci != null ? ci.Count : 0;
      }
      sc += Inventory.GetStackedCount(sl);
      return sc;
    }

    //public int GetFightItemTotalCount(FightItemKind fightItemKind)
    //{
    //  int count = 0;
    //  var fi = GetFightItemFromInv(fightItemKind);
    //  if (fi != null)
    //    count = fi.Count;

    //  var fi1 = GetFightItemFromCurrentEq();
    //  if (fi1 != null && fi1.FightItemKind == fightItemKind)
    //    count += fi1.Count;

    //  return count;
    //}

    public override FightItem GetFightItem(FightItemKind kind)
    {
 
      if (kind == FightItemKind.ThrowingTorch)
      {
        var pfi = GetFightItemFromCurrentEq();
        if (pfi != null && pfi.FightItemKind == kind)
          return pfi;
      }

      var fi = GetFightItemFromInv(kind);
      return fi;
    }

    public FightItem GetFightItemFromCurrentEq()
    {
      return CurrentEquipment.GetActiveEquipment(CurrentEquipmentKind.Shield) as FightItem;
    }

    private ProjectileFightItem GetFightItemFromInv(FightItemKind kind)
    {
      return Inventory.GetStacked<ProjectileFightItem>().FirstOrDefault(i => i.FightItemKind == kind);
    }

    public override bool RemoveFightItem(FightItem fi)
    {
      var fiCurrent = GetFightItemFromCurrentEq();
      if (fiCurrent != null && fi.FightItemKind == fiCurrent.FightItemKind)
      {
        var  arg = new RemoveItemArg();
        arg.StackedCount = 1;
        return CurrentEquipment.Remove(fiCurrent, arg) != null;
      }
      else
        return Inventory.Remove(fi) != null;
    }

    public override AttackDescription GetAttackValue(AttackKind attackKind)
    {
      var wpn = this.GetActiveWeapon();
      var withVariation = wpn != null ? !wpn.StableDamage : true;
      if (withVariation)
        withVariation = this.UseAttackVariation;
      return new AttackDescription(this, withVariation, attackKind);
    }

    public override AbilityKind SelectedActiveAbilityKind
    {
      get { return AbilityKind.Unset; }
    }

    public SpellStateSet Spells { get => spellStatesSet; set => spellStatesSet = value; }

    public FightItem GetFightItemKindAmmoForCurrentWeapon()
    {
      var wpn = this.GetActiveWeapon();
      return GetFightItemKindAmmo(wpn);
    }

    private FightItem GetFightItemKindAmmo(Weapon wpn)
    {
      ProjectileFightItem pfi = null;

      if (wpn.IsBowLike)
      {
        var projItems = Inventory.GetStacked<ProjectileFightItem>();
        if (wpn.kind == Weapon.WeaponKind.Bow)
          pfi = projItems.Where(i => i.FightItemKind.IsBowAmmoKind()).FirstOrDefault();
        else
          pfi = projItems.Where(i => i.FightItemKind.IsCrossBowAmmoKind()).FirstOrDefault();
      }
      return pfi;
    }

    static Dictionary<SpellKind, AbilityKind> SpellKind2AbilityKind = new Dictionary<SpellKind, AbilityKind>()
    {
      {SpellKind.FireBall, AbilityKind.FireBallMastering},
      {SpellKind.IceBall, AbilityKind.IceBallMastering},
      {SpellKind.PoisonBall, AbilityKind.PoisonBallMastering},
    };

    public override float GetExtraDamage(SpellKind kind, float damage)
    {
      if(!SpellKind2AbilityKind.ContainsKey(kind))
        return 0;

      var ab = this.GetPassiveAbility(SpellKind2AbilityKind[kind]);
      var dam = Calculated.FactorCalculator.CalcPercentageValue(damage, ab.PrimaryStat.Factor);
      return dam;
    }

    internal bool CanAddNextSkeleton(int currentCount)
    {
      //var ab = this.GetPassiveAbility(AbilityKind.SkeletonMastering);
      //if (currentCount > ab.AuxStat.Factor)
      //  return false;

      //return true;
      return false;
    }

    public override bool IsInProjectileReach(IProjectile fi, Point target)
    {
      if (fi is ProjectileFightItem pfi)//TODO
      {
        AbilityKind kind = AbilityKind.Unset;
        if (pfi.FightItemKind.IsBowAmmoKind())
          kind = AbilityKind.BowsMastering;
        if (pfi.FightItemKind.IsCrossBowAmmoKind())
          kind = AbilityKind.CrossBowsMastering;
        if (kind != AbilityKind.Unset)
        {
          var ab = abilities.GetAbility(kind);
          fi.Range = ProjectileFightItem.CalcRange(pfi.FightItemKind) + ab.GetExtraRange();
        }
      }
      return base.IsInProjectileReach(fi, target);
    }
  }
}