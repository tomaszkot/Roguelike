﻿using Roguelike.Attributes;
using Roguelike.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  public enum ActiveAbilityKind
  {
    Unset, ExplosiveMastering, StoneThrowingMastering, DaggerThrowingMastering

  }
  /// <summary>
  /// ActiveAbility must be used explicitely by a user
  /// </summary>
  public class ActiveAbility : Ability
  {
    ActiveAbilityKind kind;

    public override bool useCustomStatDescription()
    {
      return Kind == ActiveAbilityKind.ExplosiveMastering;
    }


    public override bool IsPercentageFromKind()
    {
      return false;
    }

    public override float CalcFactor(bool primary, int level)
    {
      ActiveAbilityKind kind = Kind;

      float factor = 0;
      switch (kind)
      {
        case ActiveAbilityKind.ExplosiveMastering:
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
            if (kind == ActiveAbilityKind.ExplosiveMastering)
              factor *= 3;
            else
              factor *= 4;
          }
          factor = (int)Math.Ceiling(factor);
          if (primary)
          {
            if (kind == ActiveAbilityKind.ExplosiveMastering)
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
        default:
          break;
      }
      return factor;
    }

    public void SetPrimaryStatDescription()
    {
      var desc = "";
      switch (kind)
      {
        case ActiveAbilityKind.ExplosiveMastering:
          desc = "Bonus to Explosive Cocktail damage";
          break;
        default:
          break;
      }
      primaryStatDescription = desc;
    }

    public ActiveAbilityKind Kind
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
          case ActiveAbilityKind.ExplosiveMastering:
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
            if (kind == ActiveAbilityKind.ExplosiveMastering)
            {
              psk = EntityStatKind.ExlosiveCoctailDamage;
              ask = EntityStatKind.ChanceToBurnNeighbour;
            }
            //if(kind == AbilityKind.HuntingMastering)
            //  psk = EntityStatKind.bl
            break;
          case ActiveAbilityKind.DaggerThrowingMastering:
            ///psk = EntityStatKind.;
            break;
          case ActiveAbilityKind.DaggerThrowingMastering:
            break;
          default:

            break;
        }
        if (PrimaryStat == null)
          return;
        PrimaryStat.SetKind(psk);
        AuxStat.SetKind(ask);
      }
    }
  }
}
