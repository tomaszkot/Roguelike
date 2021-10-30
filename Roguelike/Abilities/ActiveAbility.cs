using Roguelike.Attributes;
using Roguelike.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
      return Kind == AbilityKind.ExplosiveMastering;
    }


    public override bool IsPercentageFromKind()
    {
      return false;
    }

    public override float CalcFactor(bool primary, int level)
    {
      AbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case AbilityKind.ExplosiveMastering:
        case AbilityKind.ThrowingStoneMastering:
        case AbilityKind.ThrowingKnifeMastering:
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
            if (kind == AbilityKind.ExplosiveMastering)
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
            //else if (kind == PassiveAbilityKind.ThrowingWeaponsMastering)
            //{
            //  factor *= 23f;
            //}
            //else if (kind == AbilityKind.HuntingMastering)
            //  factor *= 18f;
          }
          break;
        
        case AbilityKind.HunterTrapMastering:
          var multsHunt = new int[] { 0, 10, 20, 30, 40, 50 };
          factor = multsHunt[level];
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
        case AbilityKind.ExplosiveMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        case AbilityKind.ThrowingStoneMastering:
          desc = "Bonus to throwing stones";
          break;
        case AbilityKind.ThrowingKnifeMastering:
          desc = "Bonus to throwing knife";
          break;
        case AbilityKind.HunterTrapMastering:
          desc = "Bonus to hunter trap";
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

    public override AbilityKind Kind
    {
      get { return kind; }
      set
      {
        kind = value;
        SetName(Kind.ToDescription());
        SetPrimaryStatDescription();
        EntityStatKind psk = EntityStatKind.Unset;
        EntityStatKind ask = EntityStatKind.Unset;
        switch (kind)
        {
          case AbilityKind.ExplosiveMastering:
            //case AbilityKind.ThrowingWeaponsMastering:
            //case AbilityKind.HuntingMastering:
            //PageIndex = 1;
            //abilityLevelToPlayerLevel.Add(1, 0);
            //abilityLevelToPlayerLevel.Add(2, 2);
            //abilityLevelToPlayerLevel.Add(3, 5);
            //abilityLevelToPlayerLevel.Add(4, 7);
            //abilityLevelToPlayerLevel.Add(5, 11);//max level is about 13
            //if (kind == AbilityKind.ThrowingWeaponsMastering)
            //{
            //  ask = EntityStatKind.ChanceToCauseBleeding;
            //  Name = "Throwing Mastery";
            //}
            //if (kind == AbilityKind.ExplosiveMastering)
            {
              psk = EntityStatKind.ExlosiveCoctailExtraDamage;
              //ask = EntityStatKind.ChanceToBurnNeighbour; TODO 
            }
            //if(kind == AbilityKind.HuntingMastering)
            //  psk = EntityStatKind.bl
            break;
          case AbilityKind.ThrowingKnifeMastering:
            psk = EntityStatKind.ThrowingKnifeExtraDamage;
            break;
          case AbilityKind.ThrowingStoneMastering:
            psk = EntityStatKind.ThrowingStoneExtraDamage;
            break;
          case AbilityKind.HunterTrapMastering:
            psk = EntityStatKind.HunterTrapExtraDamage;
            break;
          default:

            break;
        }
        //if (PrimaryStat == null)
        //  return;
        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
      }
    }
  }
}
