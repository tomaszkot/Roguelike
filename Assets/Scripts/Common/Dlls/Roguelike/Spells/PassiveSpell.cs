using Dungeons.Tiles;
using Roguelike.Abstract.Effects;
using Roguelike.Abstract.Spells;
using Roguelike.Attributes;
using Roguelike.Calculated;
using Roguelike.Factors;
using Roguelike.Tiles.LivingEntities;

namespace Roguelike.Spells
{
  public class PassiveSpell : Spell, ILastingSpell
  {
    protected Tile tile;
    const int baseRange = 3;
    public int Duration { get; set; }
    public int BaseDuration { get; set; } = 4;
    public readonly int BaseFactor = 30;
    const int baseHealth = 20;
    public EntityStatKind StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    public EffectiveFactor StatKindEffective { get; set; }

    public PassiveSpell(LivingEntity caller, SpellKind sk, EntityStatKind statKind, int baseFactor = 30) : base(caller, null)
    {
      BaseFactor = baseFactor;
      manaCost = (float)(BaseManaCost * 2);
      StatKind = statKind;
      this.Kind = sk;

      var ale = caller as AdvancedLivingEntity;
      if (ale != null)
        CurrentLevel = ale.Spells.GetState(Kind).Level;

      StatKindPercentage = CalcFactor(CurrentLevel);
      StatKindEffective = caller.CalcEffectiveFactor(StatKind, StatKindPercentage.Value);
         
      Duration = CalcDuration(); 
    }

    public int CalcRange(bool currentLevel)
    {
      var ra = baseRange + CurrentLevel;
      if (!currentLevel)
      {
        ra += 1;
      }
      return ra;
    }

    public ISpell ToSpell()
    {
      return this;
    }

    protected virtual PercentageFactor CalcFactor()
    {
      return CalcFactor(CurrentLevel);
    }

    protected virtual PercentageFactor CalcFactor(int magicLevel)
    {
      return new PercentageFactor(BaseFactor + magicLevel);
    }

    protected void SetHealthFromLevel(LivingEntity spellTarget, float factor = 1)
    {
      var lvl = CurrentLevel;
      var he = GetHealthFromLevel(lvl) * factor;
      spellTarget.Stats.SetNominal(EntityStatKind.Health, he);
    }    
    protected int GetHealthFromLevel(int lvl)
    {
      return CalcHealthFromLevel(lvl);
    }

    public static int CalcHealthFromLevel(int lvl)
    {
      return FactorCalculator.CalcFromLevel(lvl, baseHealth);
    }

    public virtual Tile Tile
    {
      get
      {
        return tile;
      }

      set
      {
        tile = value;
      }
    }

    protected int CalcDuration()//float factor = 1)
    {
      //var level = 1;
     
      var dur = CalcDuration(CurrentLevel);//, factor);
      //if (CurrentLevel > 1)
      //{
      //  var durPrev = CalcDuration(CurrentLevel-1, factor);
      //  if (durPrev == dur)
      //    dur++; 
      //}
      return dur;
    }

    protected int CalcDuration(int level)//, float factor = 1)
    {
      //var he = GetHealthFromLevel(level);
      //float baseVal = ((float)he) * factor;
      //return (int)(baseVal / 4f);
      return BaseDuration + level;
    }

    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);
      if (Kind != SpellKind.Teleport && Kind != SpellKind.SwapPosition)
      {
        if (currentMagicLevel)
          desc.Duration = Duration;
        else
          desc.Duration = CalcDuration(CurrentLevel + 1);
      }
      else {
        if (!currentMagicLevel)
        {
          desc.Range = CalcRange(false);
        }
      }
      if (StatKind != EntityStatKind.Unset && Kind != SpellKind.ManaShield)
      {
        desc.StatKind = StatKind;
        desc.StatKindPercentage = StatKindPercentage;
      }
      return desc;
    }
  }
}
