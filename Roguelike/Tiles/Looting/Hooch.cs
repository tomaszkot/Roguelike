﻿using Roguelike.Attributes;
using Roguelike.Factors;
using System.Collections.Generic;

namespace Roguelike.Tiles.Looting
{
  public class Hooch : Consumable
  {
    //public const string HoochGuid = "4fe17985-47d3-2b35-bddf-99a4af2b1dfc";
    //static Dictionary<EntityStatKind, double> effects = new Dictionary<EntityStatKind, double>();
    //public static float StrengthPercentage = 50;
    //public static float ChanceToHitPercentage = 15;
    public EntityStatKind SecondStatKind { get; set; }

    public Hooch()
    {
      Symbol = '&';
      collectedSound = "bottle1";
      consumedSound = "drink";
      Price = 10;
      tag1 = "hooch";
      Name = "Hooch";
      //#if ASCII_BUILD
      //      color = GoldColor;
      //#endif
      PrimaryStatDescription = "Powerful liquid, can be drunk or used as a part of a recipe.";
      StatKind = EntityStatKind.Strength;
      SecondStatKind = EntityStatKind.ChanceToHit;
      TourLasting = 7;
    }

    public override PercentageFactor GetPercentageStatIncrease()
    {
      return new PercentageFactor(50);
    }

    public PercentageFactor GetSecondPercentageStatIncrease()
    {
      return new PercentageFactor(-20);
    }

    //string[] extDesc;
    //public override string[] GetExtraStatDescription()
    //{
    //  if (extDesc == null)
    //  {
    //    extDesc = new string[4];
    //    //extDesc[0] = "Press O to drink it.";
    //    extDesc[0] = "Drink Effect:";
    //    extDesc[1] = "Strength +" + StrengthPercentage + "%";
    //    extDesc[2] = "ChanceToChit -" + ChanceToHitPercentage + "%";
    //    extDesc[3] = "Tour Lasting: " + TourLasting;
    //  }
    //  return extDesc;
    //}
  }
}
