using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Godot;
using static System.Net.Mime.MediaTypeNames;

namespace God4_1.ClientScripts
{
  public partial class LivingEntity : Sprite2D
  {
    public void showDamageLabel(float damageValue, Color color, Roguelike.Tiles.LivingEntities.LivingEntity tile, string text = "")
    {
      updateHealthBar(tile);
      var label = (DamageLabel)ResourceLoader.Load<PackedScene>("res://ClientScripts/damage_label.tscn").Instantiate();
      label.Text = (Math.Round(damageValue, 2)).ToString();
      if (text != "")
        label.Text = text;
      AddChild(label);
    }
    public void updateHealthBar(Roguelike.Tiles.LivingEntities.LivingEntity tile)
    {
      var hpBar = (Node2D)GetNode("HpBar/Hp");
      var percentOfHealth = tile.Stats.Health / tile.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Health);
      if (percentOfHealth < 0) percentOfHealth = 0;
      hpBar.Scale = new Vector2((float)percentOfHealth, hpBar.Scale.Y);
    }
  }
}
