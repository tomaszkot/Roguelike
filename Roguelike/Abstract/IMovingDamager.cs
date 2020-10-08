using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Roguelike.Attributes;
using Roguelike.Tiles;

namespace Roguelike.Abstract
{
  public interface ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
    
    //TODO move them to derived class
    EntityStatKind StatKind { get; set; }
    float StatKindFactor { get; set; }
  }

  public interface ILastingSpell
  {
    int TourLasting { get; set; }
  }
}
