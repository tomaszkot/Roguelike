using Dungeons.Tiles;
using Roguelike.Attributes;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Spells
{
  public class DefensiveSpell : Spell
  {
    protected Tile tile;
    public int TourLasting { get; set; }
    //public int TourLasting { get; set; }

    //public DefensiveSpell() : this(new LivingEntity()) { }
    public DefensiveSpell(LivingEntity caller) : base(caller)
    {
    }

    protected void SetHealthFromLevel(LivingEntity spellTarget, float factor = 1)
    {
      var lvl = GetCurrentLevel();
      var he = GetHealthFromLevel(lvl) * factor;
      spellTarget.Stats.SetNominal(EntityStatKind.Health, he);
    }

    const int baseHealth = 20;
    protected int GetHealthFromLevel(int lvl)
    {
      return FactorCalculator.CalcFromLevel(lvl, baseHealth);
      //if (lvl == 0)
      //	return 0;

      //if (lvl == 1)
      //     return baseHealth;

      //int prev = GetHealthFromLevel(lvl - 1);
      //return prev + (int)(prev * 10f/100f);
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

    protected int CalcTourLasting(float factor = 1)
    {
      return CalcTourLasting(GetCurrentLevel(), factor);
    }

    protected int CalcTourLasting(int magicLevel, float factor = 1)
    {
      var he = GetHealthFromLevel(magicLevel);
      float baseVal = ((float)he) * factor;
      return (int)(baseVal / 4f);
    }
  }
}
