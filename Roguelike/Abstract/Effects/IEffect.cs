﻿using Roguelike.Attributes;
using Roguelike.Factors;

namespace Roguelike.Abstract.Effects
{
  /// <summary>
  /// ILastingEffectSrc can be Spell, Hit by melee or projectile
  /// </summary>
  public interface ILastingEffectSrc
  {
    int TourLasting { get; set; }
    EntityStatKind StatKind { get; set; }

    PercentageFactor StatKindPercentage { get; }
    EffectiveFactor StatKindEffective { get; }
  }

  public interface ILastingSpell : ILastingEffectSrc
  {
  }
}
