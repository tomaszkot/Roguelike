using System;

namespace Roguelike.Tiles.Looting
{
  public class ExplosiveCocktail : Loot// FightItem, IMovingDamager
  {
    public const string Guid = "1fe17985-47d3-2b35-bddf-99a4af2b1aaa";
    
    public ExplosiveCocktail()
    {
      Symbol = '&';
#if ASCII_BUILD
      color = GoldColor;
#endif
      tag1 = "expl_cocktail";
      Name = "Explosive Cocktail";
      Price = 10;
      PrimaryStatDescription = "Explodes hurting the victim and nearby entities with fire";
      //Kind = FightItemKind.ExplodePotion;
      //abilityKind = AbilityKind.ExplosiveMastering;
      //auxFactorName = "ChanceToBurnNeighbor";
    }

  }
}
