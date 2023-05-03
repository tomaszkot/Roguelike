using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Tiles.Abstract;
using Roguelike.Tiles.LivingEntities;
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

    public Loot Loot { get => this; }

    public EffectType EffectType { get; set; }
    public bool PercentageStatIncrease { get; set; }
    public int Duration { get => ConsumptionSteps; set => ConsumptionSteps = value; }
    //public EntityStatKind StatKind { get => EnhancedStat; set => EnhancedStat = value; }
    public PercentageFactor StatKindPercentage
    {
      get => GetPercentageStatIncrease();
    }

    private float defaultEffectiveFactor = 0;

    public virtual EffectiveFactor StatKindEffective => new EffectiveFactor(DefaultEffectiveFactor);

    //EffectiveFactor GetEffectiveStatIncrease() { return new EffectiveFactor(0); }
    public bool NegativeFactor { get; set; }
    public string ConsumedSound { get => consumedSound; protected set => consumedSound = value; }

    public virtual int GetPercentageStatIncreaseDiviver()
    {
      return 20;
    }

    public virtual PercentageFactor GetPercentageStatIncrease()
    {
      var divider = GetPercentageStatIncreaseDiviver();

      if (Roasted)
        divider /= 2;
      var inc = 100 / divider;
      if (NegativeFactor)
        inc *= -1;
      return new PercentageFactor(inc);
    }

    protected string GetConsumeDesc(string desc)
    {
      if (Strings.ConsumeDescPart.Any())
        return desc + ", " + Strings.ConsumeDescPart;
      return desc;
    }

    public bool TourLastingProperty { get; set; }
    public float DefaultEffectiveFactor { get => defaultEffectiveFactor; set => defaultEffectiveFactor = value; }

    public override List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      if (m_lootStatInfo == null || !m_lootStatInfo.Any())
      {
        m_lootStatInfo = new List<LootStatInfo>();
        var lsi = new LootStatInfo();
        lsi.Desc = StatToDescription();
        
        lsi.EntityStatKind = StatKind;

        m_lootStatInfo.Add(lsi);
      }

      return m_lootStatInfo;
    }

    protected virtual string StatToDescription()
    {
      var res = statKind.ToDescription() + ": ";
      if (PercentageStatIncrease)
      {
        res += GetPercentageStatIncrease();
        if (Duration > 1 && !TourLastingProperty)
          res += FormatTurns(Duration);
      }
      else
        res += " +" + StatKindEffective.Value;

      return res;
    }
  }
}
