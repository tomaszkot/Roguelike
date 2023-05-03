using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Tiles.Looting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  /// <summary>
  /// ActiveAbility must be used explicitely by a user
  /// </summary>
  public class ActiveAbility : Ability
  {
    public override bool useCustomStatDescription()
    {
      return false;
    }

    public override bool IsPercentageFromKind => kind == AbilityKind.Stride;//|| kind == AbilityKind.CauseBleeding;
    public static  readonly EntityStatKind[] ElementalVengeanceEntityStatKinds = new[] { EntityStatKind.FireAttack, EntityStatKind.PoisonAttack, EntityStatKind.ColdAttack };
    public override AbilityKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;

        UsesCoolDownCounter = true;
        SetName(Kind.ToDescription());
        SetPrimaryStatDescription();
        EntityStatKind psk = EntityStatKind.Unset;
        EntityStatKind ask = EntityStatKind.Unset;
        switch (kind)
        {
          case AbilityKind.ExplosiveCocktail:
            psk = EntityStatKind.ExlosiveCoctailExtraDamage;
            MaxLevel = 10;
            UsesCoolDownCounter = false;
            break;
          case AbilityKind.PoisonCocktail:
            psk = EntityStatKind.PoisonCoctailExtraDamage;
            UsesCoolDownCounter = false;
            MaxLevel = 10;
            break;
          case AbilityKind.ThrowingKnife:
            psk = EntityStatKind.ThrowingKnifeExtraDamage;
            UsesCoolDownCounter = false;
            MaxLevel = 10;
            break;
          case AbilityKind.Cannon:
            psk = EntityStatKind.MaxCannonsCount;
            ask = EntityStatKind.CannonExtraChanceToHit;
            MaxLevel = 5;
            break;
          case AbilityKind.ThrowingTorch:
            psk = EntityStatKind.ThrowingTorchChanceToCauseFiring;
            UsesCoolDownCounter = false;
            MaxLevel = 5;
            break;
          case AbilityKind.ThrowingStone:
            psk = EntityStatKind.ThrowingStoneExtraDamage;
            MaxLevel = 10;
            UsesCoolDownCounter = false;
            break;
          case AbilityKind.HunterTrap:
            psk = EntityStatKind.HunterTrapExtraDamage;
            UsesCoolDownCounter = false;
            MaxLevel = 10;
            break;
          case AbilityKind.Stride:
            psk = EntityStatKind.StrideExtraDamage;
            break;
          case AbilityKind.OpenWound:
            psk = EntityStatKind.BleedingExtraDamage;
            ask = EntityStatKind.BleedingDuration;
            break;
          case AbilityKind.Rage:
            psk = EntityStatKind.MeleeAttack;
            break;
          case AbilityKind.PiercingArrow:
            psk = EntityStatKind.ChanceForPiercing;
            ask = EntityStatKind.NumberOfPiercedVictims;
            break;
          case AbilityKind.ArrowVolley:
            psk = EntityStatKind.ArrowVolleyCount;
            break;

          case AbilityKind.WeightedNet:
            psk = EntityStatKind.WeightedNetDuration;
            ask = EntityStatKind.WeightedNetExtraRange;
            UsesCoolDownCounter = false;
            break;
          case AbilityKind.PerfectHit:
            psk = EntityStatKind.PerfectHitDamage;
            ask = EntityStatKind.PerfectHitChanceToHit;
            break;

          case AbilityKind.IronSkin:
            psk = EntityStatKind.Defense;
            ask = EntityStatKind.IronSkinDuration;
            PrimaryStat.Unit = EntityStatUnit.Percentage;
            AuxStat.Unit = EntityStatUnit.Absolute;
            TurnsIntoLastingEffect = true;
            MaxCollDownCounter = 10;
            break;

          case AbilityKind.Smoke:
            psk = EntityStatKind.SmokeScope;
            ask = EntityStatKind.SmokeDuration;
            PrimaryStat.Unit = EntityStatUnit.Absolute;
            AuxStat.Unit = EntityStatUnit.Absolute;
            MaxCollDownCounter = 12;
            break;

          case AbilityKind.ElementalVengeance:
            for (int i = 0; i < ElementalVengeanceEntityStatKinds.Count(); i++)
            {
              var es = new EntityStat(ElementalVengeanceEntityStatKinds[i], 0, EntityStatUnit.Absolute);
              es.UseSign = true;
              SetAt(i, es);
            }
           
            MaxLevel = 5;
            TurnsIntoLastingEffect = true;
            break;

          case AbilityKind.ZealAttack:
            psk = EntityStatKind.ZealAttackVictimsCount;
            PrimaryStat.Unit = EntityStatUnit.Absolute;
            MaxLevel = 3;
            break;
          default:
            Debug.WriteLine("Unsupported kind:  " + kind);
            break;
        }

        if(PrimaryStat.Kind == EntityStatKind.Unset)
          PrimaryStat.SetKind(psk);
        if (AuxStat.Kind == EntityStatKind.Unset)
          AuxStat.SetKind(ask);

        if (kind == AbilityKind.Stride || kind == AbilityKind.PiercingArrow)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
          if (kind == AbilityKind.PiercingArrow)
          {
            //PrimaryStat.Unit = EntityStatUnit.Absolute;
            AuxStat.Unit = EntityStatUnit.Absolute;
          }
        }
        else if (kind == AbilityKind.IronSkin)
          PrimaryStat.Unit = EntityStatUnit.Percentage;

        else if (kind == AbilityKind.ArrowVolley)
          PrimaryStat.Unit = EntityStatUnit.Absolute;

        else if (kind == AbilityKind.ThrowingTorch)
          PrimaryStat.Unit = EntityStatUnit.Percentage;

        else if (kind == AbilityKind.OpenWound)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
          AuxStat.Unit = EntityStatUnit.Absolute;
        }
        
        else if (kind == AbilityKind.Rage)
        {
          PrimaryStat.Unit = EntityStatUnit.Percentage;
          TurnsIntoLastingEffect = true;
        }
        else if (kind == AbilityKind.WeightedNet)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
          AuxStat.Unit = EntityStatUnit.Absolute;
        }
        else if (kind == AbilityKind.Smoke)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
        }
        else if (kind == AbilityKind.Cannon)
        {
          PrimaryStat.Unit = EntityStatUnit.Absolute;
          AuxStat.Unit = EntityStatUnit.Percentage;
        }

        if (//kind == AbilityKind.Rage ||
            kind == AbilityKind.IronSkin //||
            //kind == AbilityKind.OpenWound ||
            //kind == AbilityKind.ElementalVengeance 
            //kind == AbilityKind.ZealAttack
          )
        {
          RunAtTurnStart = true;
          //AutoApply = true;
          //Activated = true;
        }
      }
    }

    public bool RunAtTurnStart { get; internal set; }

    public override float CalcFactor(int index, int level)
    {
      AbilityKind kind = Kind;
      bool primary = index == 0;//TODO
      float factor = 0;
      switch (kind)
      {
        case AbilityKind.Stride:
                
          float fac = CalcFightItemFactor(level);
          factor = fac;

          if (!primary)
          {
            if (kind == AbilityKind.ExplosiveCocktail ||
              kind == AbilityKind.PoisonCocktail)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == AbilityKind.ExplosiveCocktail ||
              kind == AbilityKind.PoisonCocktail)
            {
              factor *= 2f;
            }
            if (kind == AbilityKind.Stride)
              factor *= 6f;
          }
          break;
        
        case AbilityKind.HunterTrap:
        case AbilityKind.ExplosiveCocktail:
        case AbilityKind.PoisonCocktail:
        case AbilityKind.ThrowingStone:
        case AbilityKind.ThrowingKnife:
          var multsHunt = new int[] { 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
          factor = multsHunt[level]*2;
          break;

        case AbilityKind.Cannon:
          if (primary)
          {
            var cannonsCount = new int[] { 0, 1, 2, 3, 4, 4 };
            factor = cannonsCount[level] ;
          }
          else {
            var percentToDo = new int[] { 0, 5, 10, 15, 20, 25 };
            factor = percentToDo[level];
          }
          break;

        case AbilityKind.ThrowingTorch:
          var multsHunt1 = new int[] { 0, 20, 40, 60, 80, 100 };
          factor = multsHunt1[level];
          break;
        case AbilityKind.PiercingArrow:
          //psk = EntityStatKind.ChanceForPiercing;
          //ask = EntityStatKind.NumberOfPiercedVictims;
          if (primary)
          {
            var percentToDo = new int[] { 0, 5, 10, 15, 20, 25 };
            factor = percentToDo[level];
          }
          else 
          {
            var victims = new int[] { 0, 2, 3, 4, 5, 6 };
            factor = victims[level];
          }
          break;
        case AbilityKind.ArrowVolley:
          if (primary)
          {
            var victims = new int[] { 0, 2, 3, 4, 5, 6 };
            factor = victims[level];
          }
          break;
        case AbilityKind.OpenWound:
          if (primary)
          {
            var extraDamages = new int[] { 0, 5, 10, 15, 20, 25 };
            factor = extraDamages[level];
          }
          else
          {
            //duration
            var durations = new int[] { 0, 3, 3, 4, 4, 5 };
            factor = durations[level];
          }
          break;
        case AbilityKind.Rage:
          if (primary)
          {
            var perc = new int[] { 0, 15, 30, 45, 60, 75 };
            factor = perc[level];
          }
          break;
        case AbilityKind.Smoke:
          if (primary)
          {
            var perc = new int[] { 0, 1, 2, 3, 4, 5 };
            factor = perc[level];
          }
          else
          {
            var durs = new int[] { 0, 2, 4, 6, 8, 10 };// %
            factor = durs[level];
          }
          break;
        case AbilityKind.WeightedNet:
            var vals = new int[] { 0, 1, 2, 3, 4, 5 };
            factor = vals[level];
  
          break;

        case AbilityKind.IronSkin:
          //psk = EntityStatKind.Defense;
          //ask = EntityStatKind.IronSkinDuration;
          if (primary)
          {
            var defs = new int[] { 0, 10, 20, 30, 40, 50 };
            factor = defs[level];
          }
          else
          {
            var durs = new int[] { 0, 3, 4, 5, 6, 7 };// %
            factor = durs[level];
          }
          break;

        case AbilityKind.ElementalVengeance:
          var elBuffs = new int[] { 0, 1, 2, 3, 4, 5 };
          factor = elBuffs[level];
          break;
        case AbilityKind.ZealAttack:
          var maxEn = new int[] { 0, 1, 2, 3 };
          factor = maxEn[level];
          break;
        case AbilityKind.PerfectHit:
          if (primary)
          {
            var victims = new int[] { 0, 10, 15, 35, 50, 70 };//PerfectHitDamage %
            factor = victims[level];
          }
          else
          {
            var extraDamages = new int[] { 0, 5, 10, 15, 20, 25 };//PerfectHitChanceToHit %
            factor = extraDamages[level];
          }
          break;
        default:
          break;
      }
      return factor;
    }

    public void SetPrimaryStatDescription()
    {
      var desc = "";
      //todo  add map to FightItem AbilityKind/FightItemKind and use here
      switch (kind)
      {
        case AbilityKind.ExplosiveCocktail:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.PoisonCocktail:
          desc = "Bonus to Poison Cocktail damage";
          break;
          
        case AbilityKind.ThrowingStone:
          desc = "Bonus to a throwing stone";
          break;
        case AbilityKind.ThrowingKnife:
          desc = "Bonus to a throwing knife";
          break;
        case AbilityKind.Cannon:
          desc = "Bonus to a cannon projectile attack";
          break;
        case AbilityKind.ThrowingTorch:
          desc = "Bonus to a throwing torch";
          break;
        case AbilityKind.HunterTrap:
          desc = "Bonus to hunter trap";
          break;
        case AbilityKind.Stride:
          desc = "Hit target with your body causing damage and possibly knocking it back";
          break;
        case AbilityKind.OpenWound:
          desc = "Hit target with a sharp melee weapon to cause bleeding";
          break;
        case AbilityKind.Rage:
          desc = "Increases the melee damage";
          break;
        case AbilityKind.ArrowVolley:
          desc = "Discharges a number of arrows at one time";
          break;
        case AbilityKind.PiercingArrow:
          desc = "Pierces an enemy, hitting other one behind";
          break;
        case AbilityKind.PerfectHit:
          desc = "Increases chance to hit and damage made by projectile";
          break;

        case AbilityKind.IronSkin:
          desc = "Increases defense for a few turns";
          break;
        case AbilityKind.ElementalVengeance:
          desc = "Adds Elemental (Fire, Poison and Cold) damage to melee attacks";
          break;

        case AbilityKind.ZealAttack:
          desc = "Attacks multiple adjacent enemies";
          break;
        case AbilityKind.Smoke:
          desc = "Creates dense smoke around the hero making enemies confused";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }

    public override string ToString()
    {
      return base.ToString() + Kind;
    }

    static Dictionary<AbilityKind, FightItemKind> Ab2Fi = new Dictionary<AbilityKind, FightItemKind>()
    {
      { AbilityKind.ThrowingKnife, FightItemKind.ThrowingKnife},
      { AbilityKind.ThrowingStone, FightItemKind.Stone},
      { AbilityKind.PoisonCocktail, FightItemKind.PoisonCocktail},
      { AbilityKind.ExplosiveCocktail, FightItemKind.ExplosiveCocktail},
      { AbilityKind.HunterTrap, FightItemKind.HunterTrap},
      { AbilityKind.WeightedNet, FightItemKind.WeightedNet},
      { AbilityKind.ThrowingTorch,  FightItemKind.ThrowingTorch},
      { AbilityKind.Cannon,  FightItemKind.CannonBall},
      { AbilityKind.Smoke,  FightItemKind.Smoke},
    };

    public static FightItemKind GetFightItemKind(AbilityKind kind)
    {
      if (Ab2Fi.ContainsKey(kind))
      {
        return Ab2Fi[kind];
      }

      return FightItemKind.Unset;
    }
  }
}
