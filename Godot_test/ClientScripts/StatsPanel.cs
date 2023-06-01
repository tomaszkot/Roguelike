using Godot;
using System;

public partial class StatsPanel : Control
{
	public void UpdateStats()
	{
		var hero = Game.hero.HeroTile;

		var name = (Label)GetNode("HeroName");
		name.Text = hero.Name;
		var dungeonLevel = (Label)GetNode("Dungeon");
		dungeonLevel.Text = "Dungeon: " + Game.GameManager.GetCurrentDungeonLevel().Index.ToString();
		var level = (Label)GetNode("HeroLevel");
		level.Text = "Level: " + hero.Level;

		var health = (ProgressBar)GetNode("Health");
		health.MaxValue = hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Health);
		health.Value = hero.Stats.Health;
		var mana = (ProgressBar)GetNode("Mana");
		mana.MaxValue = hero.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Mana);
		mana.Value = hero.Stats.Mana;
		var exp = (ProgressBar)GetNode("Exp");
		exp.MaxValue = hero.NextLevelExperience;
		exp.Value = hero.Experience;
  }
}
