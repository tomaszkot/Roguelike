using Roguelike.Abstract.Effects;
using Roguelike.Attributes;
using Roguelike.Factors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abilities
{
  public class AbilityLastingEffectSrc : ILastingEffectSrc
  {
    Ability ability;
    public AbilityLastingEffectSrc(Ability ab)
    {
      ability = ab;
    }
    public int Duration { get => 3; set { } }
    public EntityStatKind StatKind { get => ability.PrimaryStat.Kind; set => ability.PrimaryStat.Kind = value; }

    public PercentageFactor StatKindPercentage => new PercentageFactor(ability.PrimaryStat.Factor);

    public EffectiveFactor StatKindEffective => new EffectiveFactor(0);
  }
}
