using Godot;
using System;

public partial class Sprite2D : Godot.Sprite2D
{
  public const int TileSize = 128;
  const int moveStep = TileSize;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

  public override void _UnhandledKeyInput(InputEvent @event)
  {
    base._UnhandledKeyInput(@event);
    if (@event.IsActionPressed("ui_up"))
    {
      GlobalPosition += new Vector2(0, -moveStep);
    }
    else if (@event.IsActionPressed("ui_down"))
    {
      GlobalPosition += new Vector2(0, moveStep);
    }
    else if (@event.IsActionPressed("ui_left"))
    {
      GlobalPosition += new Vector2(-moveStep, 0);
    }
    else if (@event.IsActionPressed("ui_right"))
    {
      GlobalPosition += new Vector2(moveStep, 0);
    }
  }

  // Called every frame. 'delta' is the elapsed time since the previous frame.
  public override void _Process(double delta)
	{
    //var up = Input.IsActionPressed("walk_up");
    //if(up)
    //  GlobalPosition += new Vector2(0, 1);
    //else
    //{
    //  GlobalPosition += new Vector2(1, 0);
    //}
    
	}
}
