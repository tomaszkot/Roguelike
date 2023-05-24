using Godot;
using Roguelike.Abstract;
using System;
using System.IO;

public partial class TopMenu : Control
{
  public override void _Ready()
  {
		string path = System.IO.Path.GetTempPath() + "Roguelike";
	string[] folders = Directory.GetDirectories(path);
		int numberOfSaves = folders.Length;
	int folderCount = folders.Length;
	if (folderCount == 0)
		{
	  var button = (Button)GetNode("VBoxContainer/Continue");
			button.Visible = false;
	}
			

	var game = (Game)GetNode("/root/Game");
		
  }

  private async void _on_exit_pressed()
	{
		var anim = (AnimationPlayer)GetNode("VBoxContainer/Exit/AnimationPlayer");
		anim.Play("clicked");
		await ToSignal(GetTree().CreateTimer(0.4),"timeout");
		GetTree().Quit();
	}

  private async void _on_start_pressed()
  {
		var startButton = (Button)GetNode("VBoxContainer/Start");
		startButton.Disabled= true;
		var anim = (AnimationPlayer)GetNode("VBoxContainer/Start/AnimationPlayer");
		anim.Play("clicked");
		await ToSignal(GetTree().CreateTimer(0.4), "timeout");
		var game = (Game)GetNode("/root/Game");
		game.GenerateDungeon();
		var parent = (CanvasLayer)GetParent();
		parent.Visible = false;
  }

  private async void _on_continue_pressed()
  {
		var button = (Button)GetNode("VBoxContainer/Continue");
		button.Disabled = true;
		var anim = (AnimationPlayer)GetNode("VBoxContainer/Continue/AnimationPlayer");
		anim.Play("clicked");
		await ToSignal(GetTree().CreateTimer(0.4), "timeout");
		var game = (Game)GetNode("/root/Game");
		Game.GameManager.Load("Godot Hero", false);
		var parent = (CanvasLayer)GetParent();
		parent.Visible = false;
  }
}

