using Godot;
using System;

namespace GodotGame
{
  namespace Entities
  {
	public partial class Hero : God4_1.ClientScripts.LivingEntity
	{
	  const int moveStep = WorldTileMap.TileSize;
	  Roguelike.Tiles.LivingEntities.Hero heroTile;
	  public event EventHandler<Vector2> Moved;
	  private float scrollingSpeed = 0.05f;
	  private bool canMove = true;

	  public Roguelike.Tiles.LivingEntities.Hero HeroTile { get => heroTile; set => heroTile = value; }

	  public override void _UnhandledKeyInput(InputEvent @event)
	  {
		base._UnhandledKeyInput(@event);

		int vertical = 0;
		int horizontal = 0;
		if (@event.IsAction("ui_up"))
		{
		  vertical = -1;
		  //GlobalPosition += new Vector2(0, -moveStep);
		}
		else if (@event.IsAction("ui_down"))
		{
		  vertical = 1;
		}
		else if (@event.IsAction("ui_left"))
		{
		  horizontal = -1;
		}
		else if (@event.IsAction("ui_right"))
		{
		  horizontal = 1;
		}
		else if (@event.IsAction("ui_accept") && canMove)
		{
		  canMove = false;
		  Game.GameManager.SkipHeroTurn();
		  WaitForMovement();
		}

		if (horizontal != 0 || vertical != 0)
		{
		  if (Moved != null && canMove)
		  {
			canMove = false;
			Moved(this, new Vector2(horizontal, vertical));
			WaitForMovement();
		  }
		}
	  }

	  private async void WaitForMovement()
	  {
		await ToSignal(GetTree().CreateTimer(0.2), "timeout");
		canMove = true;
	  }

	  public void getDamaged(float damageValue, bool missed = false)
	  {
		if (!missed)
		  showDamageLabel(damageValue, new Color(1, 1, 1), heroTile);
		else
		  showDamageLabel(damageValue, new Color(1, 1, 1), heroTile, "Evaded");
			Game.gui.statsPanel.UpdateStats();
	}


	  public override void _Input(InputEvent @event)
	  {
		base._Input(@event);
		if (@event is InputEventMouseButton m)
		{
		  if (m.IsPressed())
		  {
			var camera = (Camera2D)GetNode("Sprite2D/Camera2D");
			if (m.ButtonIndex == MouseButton.WheelUp)
			{
			  if (camera.Zoom > new Vector2(0.4f, 0.4f))
				camera.Zoom = new Vector2(camera.Zoom.X - scrollingSpeed, camera.Zoom.Y - scrollingSpeed);
			}
			if (m.ButtonIndex == MouseButton.WheelDown)
			{
			  if (camera.Zoom < new Vector2(1.2f, 1.2f))
				camera.Zoom = new Vector2(camera.Zoom.X + scrollingSpeed, camera.Zoom.Y + scrollingSpeed);
			}
		  }
		}
		Game.GameManager.CollectLootOnHeroPosition();

	  }
	}
  }
}
