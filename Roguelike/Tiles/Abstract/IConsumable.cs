using Roguelike.Abstract;
using Roguelike.Attributes;
using Roguelike.Effects;
using Roguelike.Factors;

namespace Roguelike.Tiles.Abstract
{
  public interface IConsumable : ILastingEffectSrc
  {
    Loot Loot { get; }
    //EntityStatKind EnhancedStat { get; }
    //PercentageFactor GetPercentageStatIncrease();
    //EffectiveFactor  GetEffectiveStatIncrease();

    EffectType EffectType { get; }//mushroom->poisoned
    bool PercentageStatIncrease { get; set; }//special potions are not percentage
    bool Roasted { get; set; }
    int ConsumptionSteps { get; set; }
  }
}
