using Roguelike.Attributes;
using Roguelike.Extensions;
using Roguelike.Factors;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Tiles.Looting
{
  public class Hooch : Consumable
  {
    //public const string HoochGuid = "4fe17985-47d3-2b35-bddf-99a4af2b1dfc";
    //static Dictionary<EntityStatKind, double> effects = new Dictionary<EntityStatKind, double>();
    //public static float StrengthPercentage = 50;
    //public static float ChanceToHitPercentage = 15;
    public EntityStatKind SecondStatKind { get; set; }

    public Hooch()
    {
      TourLastingProperty = true;
      Symbol = '&';
      collectedSound = "bottle1";
      consumedSound = "drink";
      Price = 10;
      tag1 = "hooch";
      Name = "Hooch";
      //#if ASCII_BUILD
      //      color = GoldColor;
      //#endif
      PrimaryStatDescription = "Powerful liquid, can be drunk or used as a part of a recipe.";
      StatKind = EntityStatKind.Strength;
      SecondStatKind = EntityStatKind.ChanceToMeleeHit;
      Duration = 7;
      LootKind = LootKind.Other;
    }

    public override List<LootStatInfo> GetLootStatInfo(LivingEntity caller)
    {
      var add = m_lootStatInfo == null || !m_lootStatInfo.Any();
      var res = base.GetLootStatInfo(caller);
      if (add)
        res.Add(
          new LootStatInfo()
          {
            EntityStatKind = EntityStatKind.ChanceToMeleeHit,
            Kind = LootStatKind.Unset,
            Desc = SecondStatKind.ToDescription() + ": " + GetSecondPercentageStatIncrease()
          });
      
      return res;
    }

    public override PercentageFactor GetPercentageStatIncrease()
    {
      return new PercentageFactor(50);
    }

    public PercentageFactor GetSecondPercentageStatIncrease()
    {
      return new PercentageFactor(-20);
    }

    public override bool IsMatchingRecipe(RecipeKind kind)
    {
      if (kind == RecipeKind.AntidotePotion || kind == RecipeKind.ExplosiveCocktail)
        return true;
      return false;
    }

  }
}
