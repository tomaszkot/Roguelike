using Godot;
using System;

public partial class TopMenu : Control
{
	private async void _on_exit_pressed()
	{
		var anim = (AnimationPlayer)GetNode("Exit/AnimationPlayer");
		anim.Play("clicked");
		await ToSignal(GetTree().CreateTimer(0.4),"timeout");
		GetTree().Quit();
	}

  private async void _on_start_pressed()
  {
		var startButton = (Button)GetNode("Start");
		startButton.Disabled= true;
		var anim = (AnimationPlayer)GetNode("Start/AnimationPlayer");
		anim.Play("clicked");
		await ToSignal(GetTree().CreateTimer(0.4), "timeout");
		var game = (Game)GetNode("/root/Game");
		game.GenerateDungeon();
		var parent = (CanvasLayer)GetParent();
		parent.Visible = false;
  }
}
