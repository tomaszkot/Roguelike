using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
