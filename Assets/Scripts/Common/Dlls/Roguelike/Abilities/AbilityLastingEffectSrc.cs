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
    int index;
    public AbilityLastingEffectSrc(Ability ab, int index)
    {
      ability = ab;
      this.index = index;
      StatKindPercentage = new PercentageFactor(0);
      StatKindEffective = new EffectiveFactor(0);
      if (ab.Stats[index].Unit == EntityStatUnit.Percentage)
        StatKindPercentage = new PercentageFactor(ab.PrimaryStat.Factor);
      else if(ab.Stats[index].Unit == EntityStatUnit.Absolute)
        StatKindEffective = new EffectiveFactor(ab.PrimaryStat.Factor);
    }

    public AbilityKind AbilityKind { get => ability.Kind; }

    public int Duration { get; set; } = 3;
    public EntityStatKind StatKind { get => ability.Stats[index].Kind; set => ability.PrimaryStat.Kind = value; }

    public PercentageFactor StatKindPercentage { get; private  set; }

    public EffectiveFactor StatKindEffective { get; private set; }
  }
}
