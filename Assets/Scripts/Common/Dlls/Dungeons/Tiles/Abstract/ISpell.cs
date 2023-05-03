namespace Dungeons.Tiles.Abstract
{
  public interface ISpell
  {
  }

  /// <summary>
  /// Any spell that causes a damage
  /// </summary>
  public interface IDamagingSpell : ISpell
  {
    string HitSound { get; }
    float Damage { get; }
  }

}
