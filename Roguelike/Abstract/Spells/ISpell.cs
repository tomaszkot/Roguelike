using Roguelike.Attributes;
using Roguelike.Tiles.LivingEntities;
using System.Collections.Generic;

namespace Roguelike.Abstract.Spells
{
  public interface IDamagingSpell
  {
    float Damage { get; }
  }

  public class SpellStatsDescription
  {
    public int Level { get; set; }
    public int ManaCost { get; set; }
    public int MagicRequired { get; set; }

    public float? Damage { get; set; }
    public int? TourLasting { get; set; }
    public EntityStatKind? StatKind { get; set; }
    List<string> extraStatDescription = new List<string>();
    
    public SpellStatsDescription(int level, int manaCost, int magicRequired)
    {
      Level = level;
      ManaCost = manaCost;
      MagicRequired = magicRequired;
    }

    public void AddString(string str, bool addIndent = true)
    {
      if (addIndent)
        str = "  " + str;
      extraStatDescription.Add(str);
    }

    public string[] GetDescription(bool addIndent = true, bool includeLevel = false)
    {
      extraStatDescription.Clear();

      if (includeLevel)
        AddString("Level: " + Level);
      AddString("Mana: " + ManaCost);
      if(Damage!=null)
        AddString("Damage: " + Damage);
      if (TourLasting != null)
        AddString("TourLasting: " + Damage);
      if (StatKind != null)
        AddString("Stat: " + StatKind);
      return extraStatDescription.ToArray();
    }

  }

  public interface ISpell : Dungeons.Tiles.Abstract.ISpell
  {
    LivingEntity Caller { get; set; }
    int CoolingDown { get; set; }
    int ManaCost { get; }
    bool Utylized { get; set; }
    int CurrentLevel { get; }

    SpellStatsDescription CreateSpellStatsDescription(bool currentLevel);
  }
}
