using Newtonsoft.Json;
using Roguelike.Attributes;
using Roguelike.Events;
using Roguelike.LootContainers;
using Roguelike.Serialization;
using Roguelike.Spells;
using Roguelike.TileParts;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;

namespace Roguelike.Tiles
{
  public class AdvancedLivingEntity : LivingEntity, IPersistable, IEquipable
  {
    public event EventHandler ExpChanged;
    protected CurrentEquipment currentEquipment = new CurrentEquipment();
    protected Inventory inventory = null;

    //[JsonIgnoreAttribute]
    public Inventory Inventory { get => inventory; set => inventory = value; }
    //[JsonIgnoreAttribute]
    public CurrentEquipment CurrentEquipment { get => currentEquipment; set => currentEquipment = value; }
    public event EventHandler<EntityStatKind> StatLeveledUp;
    //Character info
    public int Level { get; set; } = 1;
    public int Experience { get; private set; }
    public int NextLevelExperience { get; set; }
    public int AvailableExpPoints { get; set; }
    protected bool canAdvanceInExp = false;
    int levelUpPoints;
    Dictionary<SpellKind, int> coolingDownSpells = new Dictionary<SpellKind, int>();

    public Scroll ActiveScroll
    {
      get { return ActiveLoot as Scroll; }
    }

    public virtual Loot ActiveLoot
    {
      get;
      set;
    }

    public AdvancedLivingEntity(Point point, char symbol) : base(point, symbol)
    {
    }

    public new static AdvancedLivingEntity CreateDummy()
    {
      return new AdvancedLivingEntity(new Point(0, 0), '\0');
    }

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

    public bool IncreaseExp(int factor)
    {
      bool leveledUp = false;
      Experience += factor;
      bool lu = Experience >= NextLevelExperience;
      if (lu && canAdvanceInExp)
      {
        PrevLevelExperience = NextLevelExperience;
        Level++;
        LevelUpPoints += GenerationInfo.LevelUpPoints;
        //AbilityPoints += 2;
        NextLevelExperience = (int)(NextLevelExperience + (NextLevelExperience * GenerationInfo.NextExperienceIncrease));
        //if (Level == 2)
        //  nextExperience += Hero.BaseExperience;//TODO

        leveledUp = true;
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

    private void OnSpellUsed(Spell spell, Enemy targetEn)
    {
      ReduceMana(spell.ManaCost);

      if (spell.CoolingDown > 0)
      {
        SetSpellCoolingDown(spell.Kind);
      }

      //TODO
      //AppendAction(new ScrollAppliedAction() { Info = Hero.ActiveScroll + " used by " + Hero.Name, Kind = Hero.ActiveScroll.Kind, Spell = spell, Target = targetEn });
      //HeroTurn = false;
    }

    public void Consume(IConsumable consumable)
    {
      //hero turn?
      //var ac = LootManager.CreateLootGameAction(loot, "Drunk " + loot.Name);
      //PlaySound("drink");
      if (inventory.Contains(consumable.Loot))
      {
        if (consumable.EnhancedStat == EntityStatKind.Unset)
        {
          var pot = consumable.Loot as Potion;
          Debug.Assert(pot != null && pot.Kind == PotionKind.Poison);
        }
        Stats.IncreaseStatFactor(consumable.EnhancedStat);
        var stacked = consumable.Loot as StackedLoot;
        inventory.Remove(stacked);
        AppendAction(new LootAction(consumable.Loot) { LootActionKind = LootActionKind.Consumed });
      }
      else
        Debug.Assert(false);
      //else if (loot is Hooch)
      //  Hero.AddLastingEffect(LivingEntity.EffectType.Hooch, 6);

      //return ac;
    }

    //public void IncreaseStatByLevelUpPoint(EntityStatKind stat)
    //{
    //  if (LevelUpPoints == 0)
    //    return;
    //  this[stat].Nominal += 1;
    //  LevelUpPoints--;
    //  EmitStatsLeveledUp(stat);
    //}

    public void EmitStatsLeveledUp(EntityStatKind stat)
    {
      if (StatLeveledUp != null)
        StatLeveledUp(this, stat);
    }

    public virtual string GetFormattedStatValue(EntityStatKind kind)
    {
      var stat = Stats.GetStat(kind);
      var value = stat.GetFormattedCurrentValue();
      
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

    protected virtual void CreateInventory()
    {
      this.inventory = new Inventory();
    }

    public bool MoveEquipmentInv2Current(Equipment eq, EquipmentKind ek)
    {
      return MoveEquipment(Inventory, CurrentEquipment, eq, ek);
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq)
    {
      return MoveEquipment(CurrentEquipment, Inventory, eq, eq.EquipmentKind);
    }

    public bool MoveEquipmentCurrent2Inv(Equipment eq, EquipmentKind ek)
    {
      return MoveEquipment(CurrentEquipment, Inventory, eq, ek);
    }

    public bool HandleEquipmentFound(Equipment eq)
    {
      var activeSet = GetActiveEquipment();
      var currentEq = activeSet[eq.EquipmentKind];
      if (currentEq == null || eq.IsBetter(currentEq))
      {
        if (currentEq != null)
        {
          if (!MoveEquipmentCurrent2Inv(currentEq))
            return false;
        }
        return MoveEquipment(Inventory, CurrentEquipment, eq, eq.EquipmentKind);
      }
      
      return false;
    }

    public bool MoveEquipment(Inventory from, CurrentEquipment to, Equipment eq, EquipmentKind ek, bool primary = true)
    {
      bool removed = from.Remove(eq);
      if (removed)
        return SetEquipment(ek, eq, primary);
      else
      {
        bool reset = from.Add(eq);
        Assert(reset, "from.Add(eq)");
      }

      return removed;
    }

    public bool MoveEquipment(CurrentEquipment from, Inventory to, Equipment eq, EquipmentKind ek, bool primary = true)
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

    public Dictionary<EquipmentKind, Equipment> GetActiveEquipment()
    {
      var result = new Dictionary<EquipmentKind, Equipment>();
      foreach (var pos in CurrentEquipment.PrimaryEquipment)//PrimaryEquipment has all kinds
      {
        var eq = CurrentEquipment.SpareEquipmentUsed[pos.Key] ? CurrentEquipment.SpareEquipment[pos.Key] : CurrentEquipment.PrimaryEquipment[pos.Key];
        result[pos.Key] = eq;
      }

      return result;
    }

    public bool MakeActive(EquipmentKind kind, Equipment eq, bool primary)
    {
      return CurrentEquipment.SpareEquipmentUsed[kind] = !primary;
    }

    bool SetEquipment(EquipmentKind kind, Equipment eq, bool primary = true)
    {
      //EventsManager.Assert(Inventory.Contains(eq), "Inventory.Contains(eq)");
      if (eq != null)
      {
        var matches = kind == eq.EquipmentKind;
        if (eq.EquipmentKind == EquipmentKind.RingLeft && kind == EquipmentKind.RingRight ||
          eq.EquipmentKind == EquipmentKind.RingRight && kind == EquipmentKind.RingLeft)
          matches = true;
        if (!matches)
          return false;//TODO action
      }

      if(primary)
        CurrentEquipment.PrimaryEquipment[kind] = eq;
      else
        CurrentEquipment.SpareEquipment[kind] = eq;

      MakeActive(kind, eq, primary);

      RecalculateStatFactors(false);

      LootAction ac = null;
      if (eq != null)
        ac = new LootAction(eq) { Info = Name + " put on " + eq, LootActionKind = LootActionKind.PutOn, EquipmentKind = eq.EquipmentKind };
      else
        ac = new LootAction(null) { Info = Name + " took off " + kind, LootActionKind = LootActionKind.TookOff, EquipmentKind = kind };
      AppendAction(ac);

      return true;
    }

    public void RecalculateStatFactors(bool fromLoad)
    {
      Stats.ResetStatFactors();
      if (fromLoad)//this shall not be affected by any after load
      {
        Stats.GetStat(EntityStatKind.ChanceToHit).SetSubtraction(0);
        Stats.GetStat(EntityStatKind.Defence).SetSubtraction(0);
        Stats.GetStat(EntityStatKind.Attack).SetSubtraction(0);
      }

      //accumulate positive factors
      AccumulateEqFactors(true);

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
    }

    public bool CanUseEquipment(Equipment eq, EntityStat eqStat)
    {
      return Stats.GetNominal(eqStat.Kind) >= eq.GetReqStatValue(eqStat);
    }

    private void AccumulateEqFactors(bool positive)
    {
      var eqipKinds = Enum.GetValues(typeof(EquipmentKind)).Cast<EquipmentKind>();

      foreach (EquipmentKind ek in eqipKinds)
      {
        if (currentEquipment.SpareEquipmentUsed.ContainsKey(ek))//old game save ?
        {
          bool spareUsed = currentEquipment.SpareEquipmentUsed[ek];
          var eq = spareUsed ? currentEquipment.SpareEquipment[ek] : currentEquipment.PrimaryEquipment[ek];
          if (eq != null)
          {
            var stats = eq.GetStats();
            //var att = stats.Stats[EntityStatKind.Attack];
            Stats.AccumulateFactors(stats, positive);
          }
        }
      }
    }
  }
}
