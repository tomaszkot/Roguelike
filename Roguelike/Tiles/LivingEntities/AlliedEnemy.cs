using Roguelike.Attributes;
using Roguelike.Spells;
using SimpleInjector;
using System;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public class AlliedEnemy : Ally
  {
    //public AlliedEnemy() : base(null)
    //{
    //  throw new Exception("use other ctor");
    //}

    public AlliedEnemy(Container cont) : base(cont)
    {
      Kind = AllyKind.Enemy;
      SetInitialStat(EntityStatKind.Strength, SkeletonSpell.SkeletonSpellStrengthIncrease);
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public override void PlayAllySpawnedSound()
    {
      //bark(false);
    }

    public override void SetTag()
    {
      tag1 = EnemySymbols.EnemiesToSymbols.Where(i => i.Value == Symbol).Single().Key; //"skeleton";
    }

    
  }
}
