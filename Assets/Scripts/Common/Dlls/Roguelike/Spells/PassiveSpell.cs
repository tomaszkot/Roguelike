using Dungeons.Core;
using Dungeons.Tiles;
using Roguelike.Abilities;
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
   
    public readonly int BaseFactor = 30;
    const int baseHealth = 20;
    public EntityStatKind StatKind { get; set; }
    public PercentageFactor StatKindPercentage { get; set; }
    public EffectiveFactor StatKindEffective { get; set; }

    public PassiveSpell(LivingEntity caller, SpellKind sk, EntityStatKind statKind, int baseFactor = 30) : base(caller, null, sk)
    {
      BaseFactor = baseFactor;
      manaCost = (float)(BaseManaCost * 2);
      StatKind = statKind;
         
      StatKindPercentage = CalcFactor(CurrentLevel);
      StatKindEffective = caller.CalcEffectiveFactor(StatKind, StatKindPercentage.Value);
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
      return CalcPropFromLevel(lvl, baseHealth);
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
   
    public override SpellStatsDescription CreateSpellStatsDescription(bool currentMagicLevel)
    {
      var desc = base.CreateSpellStatsDescription(currentMagicLevel);

      
      if (StatKind != EntityStatKind.Unset && Kind != SpellKind.ManaShield)
      {
        desc.StatKind = StatKind;
        desc.StatKindPercentage = StatKindPercentage;
      }
      return desc;
    }
  }
}
