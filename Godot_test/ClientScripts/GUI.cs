using Godot;
using System;
using System.Reflection.Metadata.Ecma335;

public partial class GUI : CanvasLayer
{
	public void ShowOptionMenu()
	{
		var options = (Control)GetNode("Options");
		if (options.Visible)
		{
			GetTree().Paused = false;
			options.Visible = false;
		}
		else
		{
		 options.Visible = true;
		 GetTree().Paused = true;
		}
	}

  private async void _on_quit_pressed()
  {
		var game = (Game)GetNode("/root/Game");
	var button = (Button)GetNode("Options/Quit");
	button.Disabled = true;
	var anim = (AnimationPlayer)GetNode("Options/Quit/AnimationPlayer");
	anim.Play("clicked");
	game.SaveGame();
	GetTree().Paused = false;
	await ToSignal(GetTree().CreateTimer(2), "timeout");
	GetTree().ReloadCurrentScene();
  }

  public override void _Input(InputEvent @event)
  {
	base._Input(@event);
	if (@event is InputEventKey k)
	{
	  if (k.IsPressed() && k.Keycode == Key.Escape && !k.IsEcho() && Game.isGameStarted)
	  {
		ShowOptionMenu();
	  }
	}
	if (@event is InputEventMouseButton m)
	{
		if (m.IsPressed())
		{
			if (m.ButtonIndex == MouseButton.WheelUp) 
			{
					GD.Print("up");
			}
		}
	}
  }
}
