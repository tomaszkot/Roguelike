using Roguelike.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public enum PotionKind { Unset, Health, Mana, Poison, Special }

  public class Potion : Consumable
  {
    public PotionKind Kind { get; private set; }

    public Potion() : this(PotionKind.Health)
    { 
    }

    public Potion(PotionKind kind)
    {
      Price = 5;
      Symbol = PotionSymbol;
      LootKind = LootKind.Potion;
      SetKind(kind);
    }

    public override float GetStatIncrease(LivingEntity caller)
    {
      var inc = Kind == PotionKind.Poison ? 0 : 50;
      return inc;
    }

    public void SetKind(PotionKind kind)
    {
      this.Kind = kind;
      if (kind == PotionKind.Health)
      {
        Name = "Health Potion";
        tag1 = "health_potion";
        EnhancedStat = EntityStatKind.Health;
        primaryStatDesc = "Restores " + EnhancedStat;
      }
      else if (kind == PotionKind.Mana)
      {
        Name = "Mana Potion";
        tag1 = "mana_potion";
        EnhancedStat = EntityStatKind.Mana;
        primaryStatDesc = "Restores " + EnhancedStat;
      }
      else if (kind == PotionKind.Poison)
      {
        Name = "Poison Potion";
        tag1 = "poison_potion";
        primaryStatDesc = "Remove poison effect";
        EnhancedStat = EntityStatKind.Unset;
      }
    }

    public override string PrimaryStatDescription => primaryStatDesc;
        
    public override string GetId()
    {
      return base.GetId() + "_" + Kind.ToString();
    }

    public override string ToString()
    {
      return base.ToString() + " PotionKind: "+ Kind;
    }

    //public EntityStatKind StatKind
    //{
    //  get
    //  {
    //    switch (Kind)
    //    {
    //      case PotionKind.Health:
    //        return EntityStatKind.Health;
    //      case PotionKind.Mana:
    //        return EntityStatKind.Mana;
    //      default:
    //        //Poison ?    
    //        break;
    //    }

    //    return EntityStatKind.Unset;
    //  }
    //}
  }
}
