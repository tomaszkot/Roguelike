using Godot;
using Roguelike.Tiles.Looting;
using System;

public partial class EquipmentItem : Node
{
	public Roguelike.Tiles.Looting.Equipment equipment;
  private string pathToEquipment = "res://Sprites/Loot/Equipment/";

  public override void _Ready()
  {
		base._Ready();
		CallDeferred("SetSprite");
  }

  public void SetSprite()
	{
		var spr = (Sprite2D)GetChild(0);
		if (equipment is Weapon w)
		{
	  var texture = pathToEquipment + "Weapons/";
	  if (w.kind == Weapon.WeaponKind.Sword)
	  {
		texture += "Swords/rusty_sword.png";
	  }
	  else if (w.kind == Weapon.WeaponKind.Bashing)
	  {
		texture += "Bashing/branch.png";
	  }
	  else if (w.kind == Weapon.WeaponKind.Axe) 
	  {
		texture += "Axes/axe.png";
	  }
	  else if (w.kind == Weapon.WeaponKind.Dagger) 
	  {
		texture += "Daggers/dagger.png";
	  }
	} 
	}
}
