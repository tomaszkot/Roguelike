using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Attributes;
using Roguelike.Tiles;

namespace Roguelike.Abstract
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

  public interface ILastingSpell
  {
    int TourLasting { get; set; }
    EntityStatKind StatKind { get; set; }
    float StatKindPercImpact { get; set; }
  }
}
