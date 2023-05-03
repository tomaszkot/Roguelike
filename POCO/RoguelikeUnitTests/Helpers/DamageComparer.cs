using Roguelike.Tiles.LivingEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoguelikeUnitTests.Helpers
{
  public abstract class DamageComparer
  {
    float? h1;
    float? h2;
    int? duration1;
    int? duration2;

    public DamageComparer(LivingEntity le)
    {
      RegisterHealth(le);
    }

    public void RegisterHealth(LivingEntity le)
    {
      if (!h1.HasValue)
        h1 = le.Stats.Health;
      else if (!h2.HasValue)
        h2 = le.Stats.Health;
      else
        throw new System.Exception("out of space");
    }

    public float HealthPercentage
    {
      get
      {
        return 100 - (h2.Value * 100 / h1.Value);
      }
    }

    public float HealthDifference
    {
      get
      {
        return h1.Value - h2.Value;
      }
    }

    public int EffectDuration
    {
      get
      {
        if (duration2.HasValue)
          return duration2.Value;

        return duration1.Value;
      }
    }

    public int RegisterDuration(Enemy en, Roguelike.Effects.EffectType et)
    {
      int effDur = 0;
      while (en.LastingEffectsSet.GetByType(et) != null)
      {
        effDur++;
        GotoNextHeroTurn();
      }
      if (!duration1.HasValue)
        duration1 = effDur;
      else if (!duration2.HasValue)
        duration2 = effDur;
      else
        throw new System.Exception("out of space");
      return effDur;
    }

    public float CalcHealthDiffPerc(DamageComparer dc1)
    {
      return HealthDifference * 100 / dc1.HealthDifference;
    }

    public abstract void GotoNextHeroTurn();
  };

  public class OuaDDamageComparer : DamageComparer
  {
    TestBase tbase;
    public OuaDDamageComparer(LivingEntity le, TestBase tbase) : base(le)
    {
      this.tbase = tbase;
    }
    public override void GotoNextHeroTurn()
    {
      tbase.GotoNextHeroTurn();
    }
  }
}
