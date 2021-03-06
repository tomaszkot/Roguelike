using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Effects;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Tiles
{
  public class CrackedStone : Tile//LivingEntity
  {
    public CrackedStone(Point point) : base(point, '%')
    {
      //Stats.SetNominal(EntityStatKind.Health, 20);
      //Stats.SetNominal(EntityStatKind.Defense, 1);
    }

    public CrackedStone() : this(new Point().Invalid())
    {
      //immunedEffects.Add(EffectType.Bleeding);
    }

    //internal CrackedStone Clone()
    //{
    //  return MemberwiseClone() as CrackedStone;
    //}
  }
}
