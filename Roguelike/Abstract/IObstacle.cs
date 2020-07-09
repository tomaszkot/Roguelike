using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract
{
  public interface IObstacle : Dungeons.Tiles.IObstacle
  {
    bool OnHitBy(ISpell damager);
    bool CanBeHitBySpell();
  }
}
