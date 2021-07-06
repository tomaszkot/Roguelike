using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Extensions;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;

namespace Roguelike.Abilities
{
  public enum PassiveAbilityKind
  {
    Unset, RestoreHealth, RestoreMana, ExplosiveMastering, LootingMastering,

    AxesMastering, BashingMastering, DaggersMastering, SwordsMastering,
    StrikeBack, BulkAttack,
    ThrowingWeaponsMastering, BowsMastering, CrossBowsMastering

    //Traps, RemoveClaws, RemoveTusk, Skinning, , ,
    //HuntingMastering /*<-(to del)*/

    //,Scroll//user must invest in each scroll indywidually

  }

  public class PassiveAbility : IDescriptable
  {
    PassiveAbilityKind kind;
    public bool BeginTurnApply;
    public int Level { get; set; }
    public EntityStat PrimaryStat { get; set; }
    public EntityStat AuxStat { get; set; }
    public int MaxLevel = 5;
    Dictionary<int, int> abilityLevelToPlayerLevel = new Dictionary<int, int>();


    public PassiveAbility()
    {
      PrimaryStat = new EntityStat();
      AuxStat = new EntityStat();
      Revealed = true;
    }

    public override string ToString()
    {
      return base.ToString() + Kind;
    }

    public int PageIndex { get; set; }
    public int PositionInPage { get; set; }

    public PassiveAbilityKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        SetName();
        SetStatDescriptions();
        EntityStatKind psk = EntityStatKind.Unset;
        EntityStatKind ask = EntityStatKind.Unset;
        switch (kind)
        {
          case PassiveAbilityKind.RestoreHealth:
            psk = EntityStatKind.Health;
            this.BeginTurnApply = true;
            break;
          case PassiveAbilityKind.RestoreMana:
            this.BeginTurnApply = true;
            psk = EntityStatKind.Mana;
            break;
          case PassiveAbilityKind.AxesMastering:
            psk = EntityStatKind.ChanceToCauseTearApart;
            ask = EntityStatKind.AxeExtraDamage;
            break;
          case PassiveAbilityKind.BashingMastering:
            psk = EntityStatKind.ChanceToCauseStunning;
            ask = EntityStatKind.BashingExtraDamage;
            break;
          case PassiveAbilityKind.DaggersMastering:
            psk = EntityStatKind.ChanceToCauseBleeding;
            ask = EntityStatKind.DaggerExtraDamage;
            break;
          case PassiveAbilityKind.SwordsMastering:
            psk = EntityStatKind.ChanceToHit;
            ask = EntityStatKind.SwordExtraDamage;
            break;

          case PassiveAbilityKind.ExplosiveMastering:
            //case AbilityKind.ThrowingWeaponsMastering:
            //case AbilityKind.HuntingMastering:
            PageIndex = 1;
            abilityLevelToPlayerLevel.Add(1, 0);
            abilityLevelToPlayerLevel.Add(2, 2);
            abilityLevelToPlayerLevel.Add(3, 5);
            abilityLevelToPlayerLevel.Add(4, 7);
            abilityLevelToPlayerLevel.Add(5, 11);//max level is about 13
            //if (kind == AbilityKind.ThrowingWeaponsMastering)
            //{
            //  ask = EntityStatKind.ChanceToCauseBleeding;
            //  Name = "Throwing Mastery";
            //}
            if (kind == PassiveAbilityKind.ExplosiveMastering)
            {
              psk = EntityStatKind.ExlosiveCoctailDamage;
              ask = EntityStatKind.ChanceToBurnNeighbour;
            }
            //if(kind == AbilityKind.HuntingMastering)
            //  psk = EntityStatKind.bl
            break;
          case PassiveAbilityKind.LootingMastering:
          case PassiveAbilityKind.StrikeBack:
          case PassiveAbilityKind.BulkAttack:
            PageIndex = 1;
            if (kind == PassiveAbilityKind.StrikeBack)
            {
              psk = EntityStatKind.ChanceToStrikeBack;

            }
            else if (kind == PassiveAbilityKind.BulkAttack)
            {
              psk = EntityStatKind.ChanceToBulkAttack;

            }
            break;
          default:
            break;
        }
        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
      }
    }

    public float GetFactor(bool primary)
    {
      return primary ? PrimaryStat.Factor : AuxStat.Factor;
    }

    public void SetFactor(bool primary, float factor)
    {
      if (primary)
        PrimaryStat.Factor = factor;
      else
        AuxStat.Factor = factor;
    }

    EntityStatKind GetEffectiveStatKind(bool primary)
    {
      return primary ? PrimaryStat.Kind : AuxStat.Kind;
    }

    public bool Revealed
    {
      get;
      set;
    }

    void SetName()
    {
      var nameToDisplay = Kind.ToDescription();
      nameToDisplay = nameToDisplay.Replace("Restore", "Restore ");
      nameToDisplay = nameToDisplay.Replace("Mastering", " Mastery");
      nameToDisplay = nameToDisplay.Replace("Defender", " Defender");
      Name = nameToDisplay;
    }

    public string Name
    {
      get;
      set;
    }
    public string LastIncError { get; set; }

    public bool IncreaseLevel(IAdvancedEntity entity)
    {
      LastIncError = "";
      if (Level == MaxLevel)
      {
        LastIncError = "Max level of ability reached";
        return false;
      }
      if (abilityLevelToPlayerLevel.ContainsKey(Level + 1))
      {
        var lev = abilityLevelToPlayerLevel[Level + 1];
        if (lev > entity.Level)
        {
          LastIncError = "Required character level for ablility increase: " + lev;
          return false;
        }
      }
      Level++;
      SetStatsForLevel();
      return true;
    }

    public float CalcFactor(bool primary)
    {
      return CalcFactor(primary, Level);
    }

    public float CalcFactor(bool primary, int level)
    {
      PassiveAbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case PassiveAbilityKind.RestoreHealth:
        case PassiveAbilityKind.RestoreMana:
          try
          {
            var mults = new float[] { 0, .3f, .75f, 1f, 1.5f, 2.5f };
            factor = mults[level];
          }
          catch (Exception)
          {
            throw;
          }
          break;
        case PassiveAbilityKind.AxesMastering:
        case PassiveAbilityKind.BashingMastering:
        case PassiveAbilityKind.DaggersMastering:
        case PassiveAbilityKind.SwordsMastering:

          if (primary)
            factor = level;
          else
            factor = level * 5;
          break;

        case PassiveAbilityKind.StrikeBack:
          var multsDefSB = new int[] { 0, 2, 4, 7, 10, 15 };
          factor = multsDefSB[level];
          break;
        case PassiveAbilityKind.BulkAttack:
          var multsDefSB1 = new int[] { 0, 4, 7, 10, 15, 20 };
          factor = multsDefSB1[level];
          break;
        case PassiveAbilityKind.ExplosiveMastering:
        case PassiveAbilityKind.ThrowingWeaponsMastering:
          float fac = CalcFightItemFactor(level);
          //List<float> facs = new List<float>();
          //for (int i = 0; i < 5; i++)
          //{
          //  facs.Add(CalcExplFactor(i));
          //}
          factor = fac;

          if (!primary)
          {
            //if (kind == AbilityKind.HuntingMastering)
            //  factor = level;
            if (kind == PassiveAbilityKind.ExplosiveMastering)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == PassiveAbilityKind.ExplosiveMastering)
            {
              factor *= 2f;
            }
            else if (kind == PassiveAbilityKind.ThrowingWeaponsMastering)
            {
              factor *= 23f;
            }
            //else if (kind == AbilityKind.HuntingMastering)
            //  factor *= 18f;
          }
          break;
        default:
          break;
      }
      return factor;
    }

    private static float CalcFightItemFactor(int level)
    {
      var fac = FactorCalculator.CalcFromLevel2(level + 1, 4) * 2.3f;
      return fac;
    }

    private float GetFightItemFactor(bool primary, int level, FightItemKind kind)
    {
      //HACK GameManager.Instance.Hero.Abilities
      //return GameManager.Instance.Hero.Abilities.GetFightItem(kind).GetFactor(primary, level);
      return 0;
    }

    public virtual void SetStatsForLevel()
    {
      SetFactor(true, CalcFactor(true));
      SetFactor(false, CalcFactor(false));
    }
    string primaryStatDescription;
    public string GetPrimaryStatDescription()
    {
      return primaryStatDescription;
    }
    public void SetStatDescriptions()
    {
      var desc = "";
      switch (kind)
      {
        case PassiveAbilityKind.RestoreHealth:
          desc = "Restores health at turn's begining";
          break;
        case PassiveAbilityKind.RestoreMana:
          desc = "Restores mana at turn's begining";
          break;
        case PassiveAbilityKind.AxesMastering:
        case PassiveAbilityKind.BashingMastering:
        case PassiveAbilityKind.DaggersMastering:
        case PassiveAbilityKind.SwordsMastering:
          desc = "Bonus when using ";
          var wpn = kind.ToString().Replace("Mastering", "");
          if (wpn.EndsWith("s"))
            wpn = wpn.TrimEnd("s".ToCharArray());
          desc += wpn + " weapon";
          break;
        case PassiveAbilityKind.ExplosiveMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case PassiveAbilityKind.LootingMastering:
          desc = "Bonus to loot experience";// frequency and quality";
          break;
        case PassiveAbilityKind.ThrowingWeaponsMastering:
          desc = "Bonus to Throwing Weapons";
          break;
        //case AbilityKind.HuntingMastering:
        //  desc = "Bonus to Hunting (Traps)";
        //  break;
        case PassiveAbilityKind.StrikeBack:
          desc = "Chance to strike back when being hit";
          break;
        case PassiveAbilityKind.BulkAttack:
          desc = "Chance to strike all sourronding enemies in one turn";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }

    public bool IsPercentageFromKind()
    {
      return IsPercentageFromKind(Kind);
    }

    public static bool IsPercentageFromKind(PassiveAbilityKind kind)
    {
      if (kind == PassiveAbilityKind.RestoreHealth ||
          kind == PassiveAbilityKind.RestoreMana)
        return true;
      return false;
    }

    public bool IsPercentage(bool primary)
    {
      if (IsPercentageFromKind())
        return true;
      return primary ? PrimaryStat.IsPercentage : AuxStat.IsPercentage;
    }

    public string GetFormattedCurrentValue(EntityStat es)
    {
      var fv = es.GetFormattedCurrentValue();
      if (!es.IsPercentage && IsPercentageFromKind())
        fv += "%";
      return fv;
    }

    public string[] GetExtraStatDescription(bool currentLevel)
    {
      List<string> desc = new List<string>();

      //FightItemKind fightItemKind = GetFightItemKind(Kind);
      //FightItem fi = null;
      //if (fightItemKind != FightItemKind.None)
      //{
      //  fi = GameManager.Instance.Hero.Abilities.GetFightItem(fightItemKind);
      //}
      if (currentLevel)
      {
        if (Kind == PassiveAbilityKind.LootingMastering ||
            Kind == PassiveAbilityKind.ExplosiveMastering)
        {
          desc.AddRange(this.GetCustomExtraStatDescription(Level));
        }
        else
        {
          desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(PrimaryStat));
          if (AuxStat.Kind != EntityStatKind.Unset)
            desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(AuxStat));
        }
      }
      else
      {
        if (Level < MaxLevel)
        {
          var esN = new EntityStat(PrimaryStat.Kind, 0);
          desc.Add("Next Level: ");

          //if (fightItemKind != FightItemKind.None)
          //{

          //}
          //else
          {
            var fac = CalcFactor(true, Level + 1);
            if (Kind == PassiveAbilityKind.LootingMastering || Kind == PassiveAbilityKind.ExplosiveMastering)
            {
              desc.AddRange(this.GetCustomExtraStatDescription(Level + 1));
            }
            else
            {
              esN.Factor = fac;
              desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(esN));
            }
            if (AuxStat.Kind != EntityStatKind.Unset)
            {
              fac = CalcFactor(false, Level + 1);
              esN = new EntityStat(AuxStat.Kind, 0);
              esN.Factor = fac;
              desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(esN));
            }
          }
        }
        else
        {
          desc.Add(MessageMaxLevelReached);
        }
      }
      return desc.ToArray();
    }

    public Tuple<EntityStat, EntityStat> GetNextLevelStats()
    {
      var primary = new EntityStat(PrimaryStat.Kind, 0);
      var secondary = new EntityStat(AuxStat.Kind, 0);
      if (Level < MaxLevel)
      {
        //if (fightItemKind != FightItemKind.None)
        //{
        //}
        //else
        {
          var fac = CalcFactor(true, Level + 1);
          if (Kind == PassiveAbilityKind.LootingMastering)
          {
            //desc.AddRange(this.GetCustomExtraStatDescription(Level + 1));
          }
          else
          {
            primary.Factor = fac;
            //desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
          if (AuxStat.Kind != EntityStatKind.Unset)
          {
            fac = CalcFactor(false, Level + 1);
            secondary = new EntityStat(AuxStat.Kind, 0);
            secondary.Factor = fac;
            //desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(esN));
          }
        }
      }
      return new Tuple<EntityStat, EntityStat>(primary, secondary);
    }

    FightItemKind GetFightItemKind(PassiveAbilityKind abilityKind)
    {
      switch (abilityKind)

      {
        case PassiveAbilityKind.ExplosiveMastering:
          return FightItemKind.ExplodePotion;
        case PassiveAbilityKind.LootingMastering:
          break;
        case PassiveAbilityKind.ThrowingWeaponsMastering:
          return FightItemKind.Knife;

        //case AbilityKind.HuntingMastering:
        //  return FightItemKind.Trap;

        default:
          break;
      }

      return FightItemKind.None;
    }

    protected List<string> customExtraStatDescription = new List<string>();
    protected virtual List<string> GetCustomExtraStatDescription(int level)
    {
      return customExtraStatDescription;
    }

    public const string MessageMaxLevelReached = "Max level reached";

    public bool MaxLevelReached
    {
      get
      {
        return Level >= MaxLevel;
      }
    }
  }
}
