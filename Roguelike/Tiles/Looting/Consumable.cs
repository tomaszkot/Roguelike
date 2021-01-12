using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Factors;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public abstract class Consumable : StackedLoot, IConsumable
  {
    EntityStatKind statKind = EntityStatKind.Unset;
    protected string consumedSound = "eat_chewing1";

    public Consumable()
    {
      StatKind = EntityStatKind.Health;
      PercentageStatIncrease = true;
      collectedSound = "collected_food";
    }

    public int ConsumptionSteps { get; set; } = 1;

    public bool Roasted { get; set; }

    public EntityStatKind StatKind { get => statKind; set => statKind = value; }

    public Loot Loot {get=> this;}

    public EffectType EffectType { get; set; }
    public bool PercentageStatIncrease { get ; set ; }
    public int TourLasting { get => ConsumptionSteps; set => ConsumptionSteps = value; }
    //public EntityStatKind StatKind { get => EnhancedStat; set => EnhancedStat = value; }
    public PercentageFactor StatKindPercentage 
    { 
      get => GetPercentageStatIncrease();  
    }

    public virtual EffectiveFactor StatKindEffective => new EffectiveFactor(0);

    EffectiveFactor GetEffectiveStatIncrease() { return new EffectiveFactor(0); }
    public bool NegativeFactor { get; set; }
    public string ConsumedSound { get => consumedSound; protected set => consumedSound = value; }

    public virtual PercentageFactor GetPercentageStatIncrease()
    {
      var divider = 20;
      
      if (Roasted)
        divider /= 2;
      var inc = 100 / divider;
      if (NegativeFactor)
        inc *= -1;
      return new PercentageFactor(inc);
    }

    protected string GetConsumeDesc(string desc)
    {
      if(Strings.ConsumeDescPart.Any())
        return desc + ", " + Strings.ConsumeDescPart;
      return desc;
    }

    public override LootStatInfo[] GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null)
      {
        m_lootStatInfo = new LootStatInfo[1];
        var lsi = new LootStatInfo();
        lsi.Desc = statKind.ToDescription() + ": ";
        if (PercentageStatIncrease)
        {
          lsi.Desc += GetPercentageStatIncrease();
          if(TourLasting > 1)
            lsi.Desc += " (x" + TourLasting + " turns)";
        }

        lsi.EntityStatKind = StatKind;

        m_lootStatInfo[0] = lsi;
      }

      return m_lootStatInfo;
    }
  }
}
