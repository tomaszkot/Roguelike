using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Effects;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class CrackedStone : LivingEntity
  {
    public CrackedStone(Point point) : base(point, '%')
    {
      Stats.SetNominal(EntityStatKind.Health, 20);
      Stats.SetNominal(EntityStatKind.Defense, 1);
    }

    public CrackedStone() : this(new Point().Invalid())
    {
      immunedEffects.Add(EffectType.Bleeding);
    }

    internal CrackedStone Clone()
    {
      return MemberwiseClone() as CrackedStone;
    }
  }
}
