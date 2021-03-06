using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract.Spells
{
  public interface IDamagingSpell
  {
    float Damage { get; }
  }

  public interface ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
  }
}
