using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Abstract.Spells
{
  public interface IDamagingSpell
  {
    float Damage { get; }
  }

  public interface ISpell : Dungeons.Tiles.Abstract.ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
    int ManaCost { get; }

    bool Utylized { get; set; }
  }
}
