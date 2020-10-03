using Roguelike.Attributes;
using Roguelike.Tiles.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public abstract class Consumable : Looting.StackedLoot, IConsumable
  {
    EntityStatKind enhancedStat = EntityStatKind.Unset;

    public Consumable()
    {
      EnhancedStat = EntityStatKind.Health;
      PercentableStatIncrease = true;
    }

    public EntityStatKind EnhancedStat { get => enhancedStat; set => enhancedStat = value; }

    public Loot Loot {get=> this;}

    public EffectType EffectType { get; set; }
    public bool PercentableStatIncrease { get ; set ; }

    public abstract float GetStatIncrease(LivingEntity caller);

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
