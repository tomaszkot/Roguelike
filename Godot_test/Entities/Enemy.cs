using Godot;

namespace GodotGame
{
	namespace Entities 
	{
		public partial class Enemy : God4_1.ClientScripts.LivingEntity
		{
			Roguelike.Tiles.LivingEntities.Enemy enemyTile;
			public Roguelike.Tiles.LivingEntities.Enemy EnemyTile { get => enemyTile; set => enemyTile = value; }

	  public void getDamaged(float damageValue, bool missed = false)
	  {
		if (!missed)
		  showDamageLabel(damageValue, new Color(1, 0, 0), enemyTile);
		else
		  showDamageLabel(damageValue, new Color(1, 0, 0), enemyTile, "Evaded");
	  }
	}
	}
}
