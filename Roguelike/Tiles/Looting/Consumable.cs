using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public abstract class Consumable : StackedLoot, IConsumable
  {
    EntityStatKind enhancedStat = EntityStatKind.Unset;

    public Consumable()
    {
      EnhancedStat = EntityStatKind.Health;
      PercentableStatIncrease = true;
    }

    public int ConsumptionSteps { get; set; } = 1;

    public bool Roasted { get; set; }

    public EntityStatKind EnhancedStat { get => enhancedStat; set => enhancedStat = value; }

    public Loot Loot {get=> this;}

    public EffectType EffectType { get; set; }
    public bool PercentableStatIncrease { get ; set ; }

    //public abstract float GetStatIncrease(LivingEntity caller);
    public virtual float GetStatIncrease(LivingEntity caller)
    {
      var divider = 20;
      //TODO show different aboveHead icon in case of different divider
      //if (Kind == FoodKind.Mushroom)
      //  divider = 10;
      //if (Kind == FoodKind.Plum)
      //  divider = 8;

      if (Roasted)
        divider /= 2;
      var inc = 100 / divider;
      return inc;
    }

    protected string GetConsumeDesc(string desc)
    {
      return desc + ", " + Strings.ConsumeDescPart;
    }

    public override LootStatInfo[] GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null)
      {
        m_lootStatInfo = new LootStatInfo[1];
        var lsi = new LootStatInfo();
        lsi.Desc = enhancedStat.ToDescription() + ": +" + (int)GetStatIncrease(caller);
        if(PercentableStatIncrease)
          lsi.Desc += " %";

        lsi.EntityStatKind = EnhancedStat;

        m_lootStatInfo[0] = lsi;
      }

      return m_lootStatInfo;
    }
  }
}
