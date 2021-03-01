using Roguelike.Attributes;
using Roguelike.Factors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum PotionKind { Unset, Health, Mana, Poison, Special }

  public class Potion : Consumable
  {
    public PotionKind Kind { get; set; }

    public Potion() : this(PotionKind.Health)
    {
      collectedSound = "bottle1";
      consumedSound = "drink";
    }

    public Potion(PotionKind kind)
    {
      Price = 50;
      Symbol = PotionSymbol;
      LootKind = LootKind.Potion;
      SetKind(kind);
    }

    public override PercentageFactor GetPercentageStatIncrease()
    {
      var inc = Kind == PotionKind.Poison ? 0 : 50;
      return new PercentageFactor(inc);
    }

    public void SetKind(PotionKind kind)
    {
      this.Kind = kind;
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
      else if (kind == PotionKind.Poison)
      {
        Name = "Poison Potion";
        tag1 = "poison_potion";
        PrimaryStatDescription = "Removes poison effect";
        StatKind = EntityStatKind.Unset;
      }
      else if (kind == PotionKind.Unset)
        throw new Exception("kind == PotionKind.Unset");
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind.ToString();
    }

    public override string ToString()
    {
      return base.ToString() + " PotionKind: "+ Kind;
    }
  }
}
