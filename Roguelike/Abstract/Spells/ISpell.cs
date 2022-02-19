using Roguelike.Attributes;
using Roguelike.Factors;
using Roguelike.Spells;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

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
    int CurrentLevel { get; }
    SpellKind Kind { get;}

    SpellStatsDescription CreateSpellStatsDescription(bool currentLevel);

    int NextLevelMagicNeeded { get; }
    //EntityStatKind[] GetEntityStatKinds();

  }

  public interface IProjectileSpell : ISpell, Roguelike.Abstract.Projectiles.IProjectile
  { 
  }
}
