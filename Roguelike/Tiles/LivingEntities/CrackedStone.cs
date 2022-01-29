using Dungeons.Core;
using Roguelike.Attributes;
using Roguelike.Effects;
using SimpleInjector;
using System.Drawing;

namespace Roguelike.Tiles.LivingEntities
{
  public class CrackedStone : LivingEntity
  {
    public CrackedStone(Container cont) : base(new Point(-1, -1), '%', cont)
    {
      Stats.SetNominal(EntityStatKind.Health, 20);
      Stats.SetNominal(EntityStatKind.Defense, 1);
      immunedEffects.Add(EffectType.Bleeding);
    }
        
    internal CrackedStone Clone()
    {
      return MemberwiseClone() as CrackedStone;
    }
  }
}
