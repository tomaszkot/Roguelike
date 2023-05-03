using Roguelike.Attributes;
using Roguelike.Spells;
using SimpleInjector;
using System;
using System.Linq;

namespace Roguelike.Tiles.LivingEntities
{
  public class AlliedEnemy : Ally
  {
    public AlliedEnemy(Container cont) : base(cont)
    {
      Kind = AllyKind.Enemy;
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public override float GetStartStat(EntityStatKind esk)
    {
      var startStat = base.GetStartStat(esk);
      if (esk == EntityStatKind.Strength)
        startStat += SkeletonSpell.SkeletonSpellStrengthIncrease;

      else if (esk == EntityStatKind.Defense)
        startStat += SkeletonSpell.SkeletonSpellDefenseIncrease;

      return startStat;
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
