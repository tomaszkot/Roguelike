using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  public enum AbilityKind
  {
    Unknown, RestoreHealth, RestoreMana, ExplosiveMastering, LootingMastering,

    AxesMastering, BashingMastering, DaggersMastering, SwordsMastering,
    MeleeDefender, MagicDefender, StrikeBack, BulkAttack,

    Traps, RemoveClaws, RemoveTusk, Unskin, Bows, CrossBows, ThrowingWeaponsMastering,
    HuntingMastering /*<-(to del)*/
    
    ,Scroll//user must invest in each scroll indywidually
    
  }

  public class Ability : IDescriptable
  {
    AbilityKind kind;
    public bool BeginTurnApply;
    public int Level { get; set; }
    public EntityStat PrimaryStat { get; set; }
    public EntityStat AuxStat { get; set; }
    public int MaxLevel = 5;
    Dictionary<int, int> abilityLevelToPlayerLevel = new Dictionary<int, int>();


    public Ability()
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

    public AbilityKind Kind
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
          case AbilityKind.RestoreHealth:
            psk = EntityStatKind.Health;
            this.BeginTurnApply = true;
            break;
          case AbilityKind.RestoreMana:
            this.BeginTurnApply = true;
            psk = EntityStatKind.Mana;
            break;
          case AbilityKind.AxesMastering:
            psk = EntityStatKind.ChanceToCauseTearApart;
            ask = EntityStatKind.AxeExtraDamage;
            break;
          case AbilityKind.BashingMastering:
            psk = EntityStatKind.ChanceToCauseStunning;
            ask = EntityStatKind.BashingExtraDamage;
            break;
          case AbilityKind.DaggersMastering:
            psk = EntityStatKind.ChanceToCauseBleeding;
            ask = EntityStatKind.DaggerExtraDamage;
            break;
          case AbilityKind.SwordsMastering:
            psk = EntityStatKind.ChanceToHit;
            ask = EntityStatKind.SwordExtraDamage;
            break;
          case AbilityKind.MeleeDefender:
            psk = EntityStatKind.ChanceToEvadeMeleeAttack;
            ask = EntityStatKind.MeleeAttackDamageReduction;
            break;
          case AbilityKind.MagicDefender:
            psk = EntityStatKind.ChanceToEvadeMagicAttack;
            ask = EntityStatKind.MagicAttackDamageReduction;
            break;
          case AbilityKind.ExplosiveMastering:
          case AbilityKind.ThrowingWeaponsMastering:
          case AbilityKind.HuntingMastering:
            PageIndex = 1;
            abilityLevelToPlayerLevel.Add(1, 0);
            abilityLevelToPlayerLevel.Add(2, 2);
            abilityLevelToPlayerLevel.Add(3, 5);
            abilityLevelToPlayerLevel.Add(4, 7);
            abilityLevelToPlayerLevel.Add(5, 11);//max level is about 13
            if (kind == AbilityKind.ThrowingWeaponsMastering)
            {
              ask = EntityStatKind.ChanceToCauseBleeding;
              Name = "Throwing Mastery";
            }
            //if(kind == AbilityKind.HuntingMastering)
            //  psk = EntityStatKind.bl
            break;
          case AbilityKind.LootingMastering:
          case AbilityKind.StrikeBack:
          case AbilityKind.BulkAttack:
            PageIndex = 1;
            if (kind == AbilityKind.StrikeBack)
            {
              psk = EntityStatKind.ChanceToStrikeBack;

            }
            else if (kind == AbilityKind.BulkAttack)
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
      AbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case AbilityKind.RestoreHealth:
        case AbilityKind.RestoreMana:
          try
          {
            var mults = new float[] { 0, 0.15f, .3f, .75f, 1f, 1.5f };
            factor = mults[level];
          }
          catch (Exception)
          {
            throw;
          }
          break;
        case AbilityKind.AxesMastering:
        case AbilityKind.BashingMastering:
        case AbilityKind.DaggersMastering:
        case AbilityKind.SwordsMastering:

          if (primary)
            factor = level;
          else
            factor = level * 5;
          break;
        case AbilityKind.MeleeDefender:
        case AbilityKind.MagicDefender:
          if (primary)
          {
            var multsDef = new int[] { 0, 2, 4, 6, 8, 10 };
            factor = multsDef[level];
          }
          else
          {
            var multsDef = new int[] { 0, 2, 6, 10, 16, 25 };
            factor = multsDef[level];
          }

          factor = (int)Math.Ceiling(factor);
          break;
        case AbilityKind.StrikeBack:
          var multsDefSB = new int[] { 0, 2, 4, 7, 10, 15 };
          factor = multsDefSB[level];
          break;
        case AbilityKind.BulkAttack:
          var multsDefSB1 = new int[] { 0, 4, 7, 10, 15, 20 };
          factor = multsDefSB1[level];
          break;
        case AbilityKind.ExplosiveMastering:
        case AbilityKind.ThrowingWeaponsMastering:
        case AbilityKind.HuntingMastering:
          float fac = CalcFightItemFactor(level);
          //List<float> facs = new List<float>();
          //for (int i = 0; i < 5; i++)
          //{
          //  facs.Add(CalcExplFactor(i));
          //}
          factor = fac;

          if (!primary)
          {
            if (kind == AbilityKind.HuntingMastering)
              factor = level;
            else if (kind == AbilityKind.ExplosiveMastering)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == AbilityKind.ExplosiveMastering)
            {
              factor *= 2f;
            }
            else if (kind == AbilityKind.ThrowingWeaponsMastering)
            {
              factor *= 23f;
            }
            else if (kind == AbilityKind.HuntingMastering)
              factor *= 18f;
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
        case AbilityKind.RestoreHealth:
          desc = "Restores health at turn's begining";
          break;
        case AbilityKind.RestoreMana:
          desc = "Restores mana at turn's begining";
          break;
        case AbilityKind.AxesMastering:
        case AbilityKind.BashingMastering:
        case AbilityKind.DaggersMastering:
        case AbilityKind.SwordsMastering:
          desc = "Bonus when using ";
          var wpn = kind.ToString().Replace("Mastering", "");
          if (wpn.EndsWith("s"))
            wpn = wpn.TrimEnd("s".ToCharArray());
          desc += wpn + " weapon";
          break;
        case AbilityKind.MeleeDefender:
          desc = "Bonus to defense against melee damage";
          break;
        case AbilityKind.MagicDefender:
          desc = "Bonus to defense against magic damage";
          break;

        case AbilityKind.ExplosiveMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.LootingMastering:
          desc = "Bonus to loot experience";// frequency and quality";
          break;
        case AbilityKind.ThrowingWeaponsMastering:
          desc = "Bonus to Throwing Weapons";
          break;
        case AbilityKind.HuntingMastering:
          desc = "Bonus to Hunting (Traps)";
          break;
        case AbilityKind.StrikeBack:
          desc = "Chance to strike back when being hit";
          break;
        case AbilityKind.BulkAttack:
          desc = "Chance to strike all sourronding enemies in one turn";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }
    public bool IsPercentageFromKind()
    {
      if (this.kind == AbilityKind.RestoreHealth ||
          this.kind == AbilityKind.RestoreMana)
        return true;
      return false;
    }


    public bool IsPercentage(bool primary)
    {
      if (IsPercentageFromKind())
        return true;
      return primary ? PrimaryStat.IsPercentage : AuxStat.IsPercentage;
    }

    public string[] GetExtraStatDescription()
    {
      List<string> desc = new List<string>();

      Func<EntityStat, string> GetFormattedCurrentValue = (EntityStat es) =>
      {
        var fv = es.GetFormattedCurrentValue();
        if (!es.IsPercentage && IsPercentageFromKind())
          fv += "%";
        return fv;
      };

      //FightItemKind fightItemKind = GetFightItemKind(Kind);
      //FightItem fi = null;
      //if (fightItemKind != FightItemKind.None)
      //{
      //  fi = GameManager.Instance.Hero.Abilities.GetFightItem(fightItemKind);
      //  desc.AddRange(fi.GetExtraStatDescription(true, Level));
      //}

      if (Kind == AbilityKind.LootingMastering)
      {
        desc.AddRange(this.GetCustomExtraStatDescription(Level));
      }
      else
      {
        desc.Add(PrimaryStat.Kind + ": " + GetFormattedCurrentValue(PrimaryStat));
        if (AuxStat.Kind != EntityStatKind.Unset)
          desc.Add(AuxStat.Kind + ": " + GetFormattedCurrentValue(AuxStat));
      }

      if (Level < MaxLevel)
      {
        var esN = new EntityStat(PrimaryStat.Kind, 0);
        desc.Add("Next Level: ");

        //if (fightItemKind != FightItemKind.None)
        //{
        //  desc.AddRange(fi.GetExtraStatDescription(true, Level + 1));
        //}
        //else
        {
          var fac = CalcFactor(true, Level + 1);
          if (Kind == AbilityKind.LootingMastering)
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
        desc.Add(MaxLevelReached);
      }
      return desc.ToArray();
    }

    FightItemKind GetFightItemKind(AbilityKind abilityKind)
    {
      switch (abilityKind)

      {
        case AbilityKind.ExplosiveMastering:
          return FightItemKind.ExplodePotion;
        case AbilityKind.LootingMastering:
          break;
        case AbilityKind.ThrowingWeaponsMastering:
          return FightItemKind.Knife;

        case AbilityKind.HuntingMastering:
          return FightItemKind.Trap;

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

    public const string MaxLevelReached = "Max level reached";

  }
}
