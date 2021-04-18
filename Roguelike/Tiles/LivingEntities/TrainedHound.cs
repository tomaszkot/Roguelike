using Dungeons.Core;
using Roguelike.Abstract;
using Roguelike.Abstract.Tiles;
using Roguelike.Attributes;
using Roguelike.Managers;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles.LivingEntities
{
  public class TrainedHound : Ally
  {
    public TrainedHound(Container cont) : base(cont)
    {
      
      Kind = AllyKind.Hound;
#if ASCII_BUILD
        color = ConsoleColor.Yellow;
#endif
    }

    public void bark(bool strong)
    {
      PlaySound("ANIMAL_Dog_Bark_02_Mono");
    }

    public override void PlayAllySpawnedSound() 
    {
      bark(false);
    }

    public override void SetTag()
    {
      tag1 = "hound";
    }
  }
}
