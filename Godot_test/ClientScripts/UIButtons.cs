using Godot;
using System;

public partial class UIButtons : Control
{
  private void _on_stats_button_pressed()
  {
	var playerStatsMenu = (PlayerStatsMenu)GetParent().GetNode("PlayerStatsMenu");
	if (!playerStatsMenu.isOn)
	{
	  DisplayPlayerStats(true);
  }
	else 
	{
	  DisplayPlayerStats(false);
  }
  }

	private void DisplayPlayerStats(bool show)
	{
		Vector3 customColor = new Vector3();
		Vector2 goalPosition = new Vector2();
	var playerStatsMenu = (PlayerStatsMenu)GetParent().GetNode("PlayerStatsMenu");
	if (show)
		{
	  playerStatsMenu.isOn = true;
	  playerStatsMenu.UpdateStatsData();
	  playerStatsMenu.Visible = true;
			customColor = new Vector3(1,1,1);
			goalPosition = new Vector2(0,0);
	} 
		else
		{
	  playerStatsMenu.isOn = false;
	  customColor = new Vector3(0.73f, 0.73f, 0.7f);
	  goalPosition = new Vector2(0, 1000);
	}

	var tween1 = GetTree().CreateTween();
	tween1.SetTrans(Tween.TransitionType.Cubic);
	tween1.TweenProperty(playerStatsMenu, "position", goalPosition, 0.3);
	var tween = GetTree().CreateTween();
	tween.TweenProperty((Button)GetNode("StatsButton"), "scale", new Vector2(1.1f, 1.1f), 0.1);
	tween.TweenProperty((Button)GetNode("StatsButton"), "scale", new Vector2(1, 1), 0.1);
	var icon = (Sprite2D)GetNode("StatsButton/Icon");
	var shader = (ShaderMaterial)icon.Material;
	shader.SetShaderParameter("custom_color", customColor);
  }
}
