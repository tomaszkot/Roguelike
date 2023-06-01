using Godot;
using Godot.Collections;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class PlayerStatsMenu : Control
{
  public bool isOn = false;

  public void UpdateStatsData()
  {
	var hero = Game.hero.HeroTile;

		List<(string, string)> statList = new List<(string, string)> { ("General/Name",hero.Name),
		("General/Gold",hero.Gold.ToString()),
	("General/Level", hero.Level.ToString()),
	("Attributes/AvailablePoints", "Expirience Points to assign: " + hero.LevelUpPoints.ToString()),
	("General/Exp",Math.Round(hero.Experience).ToString() + "/" + Math.Round(hero.NextLevelExperience).ToString()),
  ("Attributes/Health", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.Health)),
  ("Attributes/Strength", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.Strength)),
  ("Attributes/Magic", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.Magic)),
	("Attributes/Defense", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.Defense)),
  ("Stats/MeleeAttack", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.MeleeAttack)),
	("Stats/Mana", GetDisplayedStat(Roguelike.Attributes.EntityStatKind.Mana)),
	("Stats/ChanceToMeleeHit", hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ChanceToMeleeHit).ToString() + "%"),
  ("Stats/ChanceToCastSpell", hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ChanceToCastSpell).ToString() + "%"),
  ("Stats/ResistFire", hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistFire).ToString() + "%"),
  ("Stats/ResistCold", hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistCold).ToString() + "%"),
  ("Stats/ResistPoison", hero.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistPoison).ToString() + "%")};

	foreach (var stat in statList)
	{
	  var godotStatField = (Label)GetNode(stat.Item1 + "/Value");
	  godotStatField.Text = stat.Item2;
	}
  }

  private string GetDisplayedStat(Roguelike.Attributes.EntityStatKind statKind)
  {
  var hero = Game.hero.HeroTile;
  var value = hero.Stats.GetStat(statKind);
	return value.GetFormattedCurrentValue().ToString() + "/" + value.GetValueToCalcPercentage(false).ToString();
  }
}
