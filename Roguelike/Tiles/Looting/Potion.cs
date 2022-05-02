﻿using Roguelike.Attributes;
using Roguelike.Factors;
using System;

namespace Roguelike.Tiles.Looting
{
  public enum PotionKind { Unset, Health, Mana, Antidote, Special }

  public class Potion : Consumable
  {
    PotionKind kind;
    public PotionKind Kind
    {
      get { return kind; }
      set 
      {
        kind = value;

        if (kind == PotionKind.Health)
        {
          Name = "Health Potion";
          tag1 = "health_potion";
          StatKind = EntityStatKind.Health;
          PrimaryStatDescription = "Restores " + StatKind;
        }
        else if (kind == PotionKind.Mana)
        {
          Name = "Mana Potion";
          tag1 = "mana_potion";
          StatKind = EntityStatKind.Mana;
          PrimaryStatDescription = "Restores " + StatKind;
        }
        else if (kind == PotionKind.Antidote)
        {
          Name = "Antidote Potion";
          tag1 = "antidote_potion";
          PrimaryStatDescription = "Removes poison effect";
          StatKind = EntityStatKind.Unset;
        }
        else if (kind == PotionKind.Unset)
          throw new Exception("kind == PotionKind.Unset");
      }
    }

    public Potion() : this(PotionKind.Health)
    {
      
    }

    public Potion(PotionKind kind)
    {
      Price = 50;
      Symbol = PotionSymbol;
      LootKind = LootKind.Potion;
      SetKind(kind);
      collectedSound = "bottle1";
      consumedSound = "drink";
    }

    public override PercentageFactor GetPercentageStatIncrease()
    {
      var inc = Kind == PotionKind.Antidote ? 0 : 50;
      return new PercentageFactor(inc);
    }

    public void SetKind(PotionKind kind)
    {
      this.Kind = kind;
      
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind.ToString();
    }

    public override string ToString()
    {
      return base.ToString() + " PotionKind: " + Kind;
    }
  }
}
