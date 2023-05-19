using Godot;
using Roguelike.Tiles.LivingEntities;
using System;

namespace GodotGame
{
  namespace Entities
  {
	public partial class Hero : Godot.Sprite2D
	{
	  const int moveStep = WorldTileMap.TileSize;
	  Roguelike.Tiles.LivingEntities.Hero heroTile;
	  public event EventHandler<Vector2> Moved;

	  public Roguelike.Tiles.LivingEntities.Hero HeroTile { get => heroTile; set => heroTile = value; }

	  // Called when the node enters the scene tree for the first time.
	  public override void _Ready()
	  {
	  }

	  public override void _UnhandledKeyInput(InputEvent @event)
	  {
		base._UnhandledKeyInput(@event);

		int vertical = 0;
		int horizontal = 0;
		if (@event.IsActionPressed("ui_up"))
		{
		  vertical = -1;
		  //GlobalPosition += new Vector2(0, -moveStep);
		}
		else if (@event.IsActionPressed("ui_down"))
		{
		  vertical = 1;
		}
		else if (@event.IsActionPressed("ui_left"))
		{
		  horizontal = -1;
		}
		else if (@event.IsActionPressed("ui_right"))
		{
		  horizontal = 1;
		}

		if (horizontal != 0 || vertical != 0)
		{
		  if (Moved != null)
			Moved(this, new Vector2(horizontal, vertical));
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
  }
}
