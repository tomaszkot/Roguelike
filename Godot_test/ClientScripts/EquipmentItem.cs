using Godot;
using Godot.Collections;
using Roguelike.Tiles.Looting;
using System;

public partial class EquipmentItem : Node
{
	public Roguelike.Tiles.Looting.Equipment equipment;
	private string pathToEquipment = "res://Sprites/Loot/Equipment/";
	private Dictionary<Weapon.WeaponKind, string> weaponKindPaths = new Dictionary<Weapon.WeaponKind, string> { { Weapon.WeaponKind.Sword, "Swords/rusty_sword.png" }, { Weapon.WeaponKind.Bashing, "Bashing/branch.png" }, { Weapon.WeaponKind.Axe, "Axes/axe.png" }, { Weapon.WeaponKind.Dagger, "Daggers/dagger.png" } };

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
		texture += weaponKindPaths[w.kind];
			spr.Texture = ResourceLoader.Load(texture) as Texture2D;
		} 
	}
}
