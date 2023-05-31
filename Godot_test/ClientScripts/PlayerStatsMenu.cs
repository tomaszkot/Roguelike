using Godot;
using System;

public partial class PlayerStatsMenu : Control
{
  public bool isOn = false;

  public void UpdateStatsData()
  {
	var hero = Game.hero.HeroTile;

	var name = (Label)GetNode("General/Name/Value");
	name.Text = hero.Name;
	var gold = (Label)GetNode("General/Gold/Value");
	gold.Text = hero.Gold.ToString();
	var level = (Label)GetNode("General/Level/Value");
	level.Text = hero.Level.ToString();
	var exp = (Label)GetNode("General/Exp/Value");
	exp.Text = Math.Round(hero.Experience).ToString() + "/" + Math.Round(hero.NextLevelExperience).ToString();

	var availablePoints = (Label)GetNode("Attributes/AvailablePoints");
	availablePoints.Text = "Expirience Points to assign: " + hero.LevelUpPoints.ToString();
	var health = (Label)GetNode("Attributes/Health/Value");
	health.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Health).ToString() + "/" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Health);
	var strength = (Label)GetNode("Attributes/Strength/Value");
	strength.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Strength).ToString() + "/" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Strength);
	var magic = (Label)GetNode("Attributes/Magic/Value");
	magic.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Magic).ToString() + "/" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Magic);
	var defence = (Label)GetNode("Attributes/Defence/Value");
	defence.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Defense).ToString() + "/" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Defense);

	var attack = (Label)GetNode("Stats/Attack/Value");
	attack.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.MeleeAttack).ToString() + "-" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.MeleeAttack);
	var mana = (Label)GetNode("Stats/Mana/Value");
	mana.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.Mana).ToString() + "/" + hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Mana);
	var hitChance = (Label)GetNode("Stats/HitChance/Value");
	hitChance.Text = hero.Stats.ChanceToMeleeHit.ToString() + "%";
	var spellChance = (Label)GetNode("Stats/SpellChance/Value");
	spellChance.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ChanceToCastSpell).ToString() + "%";
	var fireRes = (Label)GetNode("Stats/FireRes/Value");
	fireRes.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistFire).ToString();
	var coldRes = (Label)GetNode("Stats/ColdRes/Value");
  coldRes.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistCold).ToString();
  var poisonRes = (Label)GetNode("Stats/PoisonRes/Value");
  poisonRes.Text = hero.Stats.GetCurrentValue(Roguelike.Attributes.EntityStatKind.ResistPoison).ToString();
  }
}
