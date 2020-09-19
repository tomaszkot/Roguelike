using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.Looting
{
  public enum PotionKind { Unset, Health, Mana, Poison, Special }

  public class Potion : StackedLoot, IConsumable
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

    public EffectType EffectType { get; set; }

    public Loot Loot { get => this; }

    public float GetStatIncrease(LivingEntity caller)
    {
      var divider = 2;
      var inc = caller.Stats[EnhancedStat].TotalValue / divider;
      return inc;
    }

    public void SetKind(PotionKind kind)
    {
      this.Kind = kind;
      if (kind == PotionKind.Health)
      {
        Name = "Health Potion";
        tag1 = "health_potion";
        primaryStatDesc = "Restores health";
      }
      else if (kind == PotionKind.Mana)
      {
        Name = "Mana Potion";
        tag1 = "mana_potion";
        primaryStatDesc = "Restores mana";
      }
      else if (kind == PotionKind.Poison)
      {
        Name = "Poison Potion";
        tag1 = "poison_potion";
        primaryStatDesc = "Remove poison effect";
      }
    }

    public override string PrimaryStatDescription => primaryStatDesc;

    public EntityStatKind EnhancedStat
    {
      get {
        return this.StatKind;
      }
    }

    public override string GetId()
    {
      return base.GetId() + "_" + Kind.ToString();
    }

    public override string ToString()
    {
      return base.ToString() + " PotionKind: "+ Kind;
    }

    public EntityStatKind StatKind
    {
      get
      {
        switch (Kind)
        {
          case PotionKind.Health:
            return EntityStatKind.Health;
          case PotionKind.Mana:
            return EntityStatKind.Mana;
          default:
            //Poison ?    
            break;
        }

        return EntityStatKind.Unset;
      }
    }
  }
}
