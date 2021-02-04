using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.LootContainers;
using Roguelike.Serialization;
using Roguelike.Spells;
using Roguelike.Effects;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Markup;
using SimpleInjector;
using Roguelike.Abstract;
using Roguelike.Discussions;
using Roguelike.Abilities;

namespace Roguelike.Tiles
{
  //public class GodActivationChangedArgs : EventArgs
  //{ 
  //}
  public enum EntityProffesionKind { Unset, King, Prince, Knight, Priest, Mercenary, Merchant, Peasant, Bandit, Adventurer, Slave }
  public enum EntityGender { Unset, Male, Female }

  public class AdvancedLivingEntity : LivingEntity, IPersistable, IEquipable, IAdvancedEntity
  {
    public bool HasUrgentTopic { get; set; }
    public Discussion Discussion { get; set; }
    public EntityProffesionKind Proffesion { get; set; }
    public event EventHandler ExpChanged;
    public event EventHandler<bool> UrgentTopicChanged;
    public event EventHandler StatsRecalculated;
    public event EventHandler LeveledUp;
    //public event EventHandler<GodActivationChangedArgs> GodActivationChanged;
    protected CurrentEquipment currentEquipment = new CurrentEquipment();
    protected Inventory inventory = null;

    public virtual Inventory Inventory 
    { 
      get => inventory; 
      set => inventory = value; 
    }
    //[JsonIgnoreAttribute]
    public CurrentEquipment CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
    public event EventHandler<EntityStatKind> StatLeveledUp;
    public event EventHandler<int> GoldChanged;
        
    public int Experience { get; private set; }
    public int NextLevelExperience { get; set; }

    int gold;
    public int Gold 
    {
      get { return gold; }
      set {
        gold = value;
        if (GoldChanged != null)
          GoldChanged(this,gold);
      } 
    }

    //public int AvailableExpPoints { get; set; } = 3;
    protected bool canAdvanceInExp = false;
    int levelUpPoints = 0;

    Dictionary<SpellKind, int> coolingDownSpells = new Dictionary<SpellKind, int>();
    Abilities.AbilitiesSet abilities = new Abilities.AbilitiesSet();

    public int AbilityPoints { get; set; } = 2;

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

    public AdvancedLivingEntity(Point point, char symbol) : base(point, symbol)
    {
    }

    public virtual bool IsSellable(Loot loot)
    {
      return loot.Price >= 0;
    }

    public bool IncreaseAbility(AbilityKind kind)
    {
      var ab = GetAbility(kind);
      var increased = ab.IncreaseLevel(this);
      if (increased)
      {
        AbilityPoints--;
        RecalculateStatFactors(false);
      }

      return increased;
    }

    public Ability GetAbility(AbilityKind kind)
    {
      //Abilities.EnsureAbilities(false);
      return Abilities.Items.Where(i => i.Kind == kind).SingleOrDefault();
    }

    public int GetPrice(Loot loot)
    {
      int count = 1;
      //if (loot.StackedInInventory) ??
      //{
      //  count = this.Inventory.GetStackedCount(loot as StackedLoot);
      //}
      var price = (int)(loot.Price * Inventory.PriceFactor)* count;
      return price;
    }

    public new static AdvancedLivingEntity CreateDummy()
    {
      return new AdvancedLivingEntity(new Point(0, 0), '\0');
    }
        
    public bool IncreaseExp(int factor)
    {
      bool leveledUp = false;
      Experience += factor;
      bool thresholdReached = Experience >= NextLevelExperience;
      if (thresholdReached && canAdvanceInExp)
      {
        PrevLevelExperience = NextLevelExperience;
        Level++;
        LevelUpPoints += GenerationInfo.LevelUpPoints;
        //AbilityPoints += 2;
        NextLevelExperience = (int)(NextLevelExperience + (NextLevelExperience * GenerationInfo.NextExperienceIncrease));
        //if (Level == 2)
        //  nextExperience += Hero.BaseExperience;//TODO

        leveledUp = true;

        this.Stats.GetStat(EntityStatKind.Health).SetSubtraction(0);
        this.Stats.GetStat(EntityStatKind.Mana).SetSubtraction(0);

        if (LeveledUp!=null)
          LeveledUp(this, EventArgs.Empty);

        AppendAction(new HeroAction() { Kind = HeroActionKind.LeveledUp, Info = "Hero has gained a new level!" });
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

      coolingDownSpells[kind] = ActiveScroll.CreateSpell(this).CoolingDown;
    }

    //private void OnSpellUsed(Spell spell, Enemy targetEn)
    //{
    //  ReduceMana(spell.ManaCost);

    //  if (spell.CoolingDown > 0)
    //  {
    //    SetSpellCoolingDown(spell.Kind);
    //  }

    //  //TODO
    //  //AppendAction(new ScrollAppliedAction() { Info = Hero.ActiveScroll + " used by " + Hero.Name, Kind = Hero.ActiveScroll.Kind, Spell = spell, Target = targetEn });
    //  //HeroTurn = false;
    //}

    public void Consume(IConsumable consumable)
    {
      //TODO hero turn?
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
              //DoConsume(consumable.StatKind, factor);
            }
            else
            {
              var factor = LastingEffectsSet.CalcLastingEffectInfo(EffectType.Unset, consumable);
              DoConsume(consumable.StatKind, factor);
            }
          }
          else
          {
            EffectType et = EffectType.ConsumedRawFood;
            if (consumable.Roasted)
              et = EffectType.ConsumedRoastedFood;
            LastingEffectsSet.AddPercentageLastingEffect(et, consumable, consumable.Loot);
          }
        }

        var info = Name + " consumed " + (consumable as Dungeons.Tiles.Tile).Name + ", Health: "+this.GetCurrentValue(EntityStatKind.Health);
        AppendAction(new LootAction(consumable.Loot) { LootActionKind = LootActionKind.Consumed, Info = info });
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

    public virtual string GetFormattedStatValue(EntityStatKind kind)
    {
      var currentValue = GetCurrentValue(kind);
      var stat = Stats.GetStat(kind);
      var value = stat.GetFormattedCurrentValue(currentValue);
      
      //var value = stat.Value.CurrentValue.ToString(""); ;
      //if (stat.IsPercentage)
      //{
      //  value += " %";
      //}
      return value;
    }

    //public virtual string GetLevel()
    //{
    //  return "1";
    //}

    [JsonIgnore]
    public bool Dirty { get; set; }
    public int PrevLevelExperience { get; private set; }
    CurrentEquipment IEquipable.CurrentEquipment { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public Abilities.AbilitiesSet Abilities { get => abilities; set => abilities = value; }

    protected virtual void CreateInventory(Container cont)
    {
      this.Inventory = new Inventory(cont);
    }

    public bool MoveEquipmentInv2Current(Equipment eq, CurrentEquipmentKind ek)
    {
      return MoveEquipment(Inventory, CurrentEquipment, eq, ek);
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq, CurrentEquipmentPosition pos)
    {
      //var cek = FromEquipmentKind(eq.EquipmentKind, pos);
      //return MoveEquipment(CurrentEquipment, Inventory, eq, cek);
      return MoveEquipmentCurrent2Inv(eq, eq.EquipmentKind, pos);
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq, EquipmentKind ek, CurrentEquipmentPosition pos)
    {
      var cek = Equipment.FromEquipmentKind(eq.EquipmentKind, pos);
      return MoveEquipment(CurrentEquipment, Inventory, eq, cek);
    }

    public static CurrentEquipmentPosition DefaultCurrentEquipmentPosition = CurrentEquipmentPosition.Left;

    public bool HandleEquipmentFound(Equipment eq)
    {
      if (!eq.IsIdentified)
      {
        return false;
      }

      var activeSet = GetActiveEquipment();//primary or secondary 
      var cep = CurrentEquipmentPosition.Unset;
      if (eq.EquipmentKind == EquipmentKind.Ring || eq.EquipmentKind == EquipmentKind.Trophy)
        cep = CurrentEquipmentPosition.Left;
      //TODO handle right
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

      if (CanUseEquipment(eq) && (currentEq == null || eq.IsBetter(currentEq)))
      {
        if (currentEq != null)
        {
          if (!MoveEquipmentCurrent2Inv(currentEq, cep))
            return false;
        }
        var destKind = Equipment.FromEquipmentKind(eq.EquipmentKind, cep);
        return MoveEquipment(Inventory, CurrentEquipment, eq, destKind);
      }
      
      return false;
    }

    public bool CanUseEquipment(Equipment eq)
    {
      return Level >= eq.RequiredLevel;
    }
        
    public bool MoveEquipment(InventoryBase from, CurrentEquipment to, Equipment eq,
                              CurrentEquipmentKind ek, bool primary = true)
    {
      bool removed = from.Remove(eq);
      if (removed)
      {
        if (to.GetActiveEquipment()[ek] != null)
        {
          var prev = to.GetActiveEquipment()[ek];
          if (!from.Add(prev))
          {
            Assert(false, "from.Add(prev)");
            return false;
          }
        }
        var set = SetEquipment(ek, eq, primary);

        return set;
      }
      else
      {
        bool reset = from.Add(eq);
        Assert(reset, "from.Add(eq)");
      }

      return removed;
    }

    public bool MoveEquipment(CurrentEquipment from, InventoryBase to, Equipment eq, CurrentEquipmentKind ek, bool primary = true)
    {
      bool done = SetEquipment(ek, null, primary);
      if (done)
      {
        done = to.Add(eq);
        if (!done)
        {
          //revert!
          if (primary)
            from.PrimaryEquipment[ek] = eq;
          else
            from.SpareEquipment[ek] = eq;
        }
      }
      
      return done;
    }

    public Dictionary<CurrentEquipmentKind, Equipment> GetActiveEquipment()
    {
      return CurrentEquipment.GetActiveEquipment();
    }

    //public bool MakeActive(CurrentEquipmentKind kind, Equipment eq, bool primary)
    //{
    //  return CurrentEquipment.SpareEquipmentUsed[kind] = !primary;
    //}

    public bool SetEquipment(CurrentEquipmentKind kind, Equipment eq, bool primary = true)
    {
      var set = CurrentEquipment.SetEquipment(kind, eq, primary);
      if (!set)
        return false;//TODO LOG

      //if (kind == CurrentEquipmentKind.God)
      //{
        
      //}
      //??
      //MakeActive(kind, eq, primary);

      RecalculateStatFactors(false);

      LootAction ac = null;
      CurrentEquipmentPosition pos;
      if (eq != null)
      {
        ac = new LootAction(eq)
        {
          Info = Name + " put on " + eq.Name,
          LootActionKind = LootActionKind.PutOn,
          EquipmentKind = eq.EquipmentKind,
          CurrentEquipmentKind = kind
        };
      }
      else
      {
        ac = new LootAction(null)
        {
          Info = Name + " took off " + kind,
          LootActionKind = LootActionKind.PutOff,
          EquipmentKind = Equipment.FromCurrentEquipmentKind(kind, out pos),
          CurrentEquipmentKind = kind
        };
      }
      AppendAction(ac);

      return true;
    }

    public void RecalculateStatFactors(bool fromLoad)
    {
      Stats.ResetStatFactors();
      if (fromLoad)//this shall not be affected by any after load
      {
        Stats.GetStat(EntityStatKind.ChanceToHit).SetSubtraction(0);
        Stats.GetStat(EntityStatKind.Defense).SetSubtraction(0);
        Stats.GetStat(EntityStatKind.Attack).SetSubtraction(0);
      }

      //accumulate positive factors
      AccumulateEqFactors(true);

      var si = GetStrengthIncrease();
      Stats.AccumulateFactor(EntityStatKind.Attack, si);
      //var abs = Abilities.GetItems();
      //foreach (var ab in abs)
      //{
      //  if (!ab.BeginTurnApply)
      //  {
      //    if (ab.PrimaryStat.Kind != EntityStatKind.Unknown)
      //    {
      //      Stats.AccumulateFactor(ab.PrimaryStat.Kind, ab.PrimaryStat.Factor);
      //      AddAuxStat(ab);
      //    }
      //  }
      //}

      AccumulateEqFactors(false);

      if (StatsRecalculated != null)
        StatsRecalculated(this, EventArgs.Empty);
    }

    protected virtual float GetStrengthIncrease()
    {
      return 0;
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
      if(UrgentTopicChanged!=null)
        UrgentTopicChanged(this, HasUrgentTopic);
    }
  }
}
