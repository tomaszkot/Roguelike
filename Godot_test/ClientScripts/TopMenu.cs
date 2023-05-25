using Godot;
using Roguelike.Abstract;
using Roguelike.Core.Serialization;
using System;
using System.IO;

public partial class TopMenu : Control
{
  public override void _Ready()
  {
	int numberOfSaves = SavedGames.GetSavedGamesList().Count;
	if (numberOfSaves == 0)
	{
	  var button = (Button)GetNode("VBoxContainer/Continue");
	  button.Visible = false;
	}
	var game = (Game)GetNode("/root/Game");
  }

  private async void _on_exit_pressed()
  {
	playAnimation("Exit");
	await ToSignal(GetTree().CreateTimer(0.4), "timeout");
	GetTree().Quit();
  }

  private async void _on_start_pressed()
  {
	playAnimation("Start");
	await ToSignal(GetTree().CreateTimer(0.4), "timeout");
	var game = (Game)GetNode("/root/Game");
	game.GenerateDungeon();
	var parent = (CanvasLayer)GetParent();
	parent.Visible = false;
  }

  private async void _on_continue_pressed()
  {
	playAnimation("Continue");
	await ToSignal(GetTree().CreateTimer(0.4), "timeout");
	var game = (Game)GetNode("/root/Game");
	Game.GameManager.Load("Godot Hero", false);
	var parent = (CanvasLayer)GetParent();
	parent.Visible = false;
  }

  private void playAnimation(string nameOfButton)
  {
	var button = (Button)GetNode("VBoxContainer/" + nameOfButton);
	button.Disabled = true;
	var anim = (AnimationPlayer)GetNode("VBoxContainer/" + nameOfButton + "/AnimationPlayer");
	anim.Play("clicked");
  }
}

