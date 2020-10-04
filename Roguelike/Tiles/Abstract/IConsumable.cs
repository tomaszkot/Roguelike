using Roguelike.Attributes;

namespace Roguelike.Tiles.Abstract
{
  public interface IConsumable
  {
    Loot Loot { get; }
    EntityStatKind EnhancedStat { get; }
    float GetStatIncrease(LivingEntity caller);
    EffectType EffectType { get; }//mushroom->poisoned
    bool PercentableStatIncrease { get; set; }
    bool Roasted { get; set; }
    int ConsumptionSteps { get; set; }
  }
}
