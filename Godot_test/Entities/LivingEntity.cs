using System;
using God4_1.Entities;
using Godot;

namespace God4_1.ClientScripts
{
  public abstract partial class LivingEntity : Entity
  {
    public Sprite2D sprite;

    public override void _Ready()
    {
      base._Ready();
      sprite = (Sprite2D)GetChild(0);
    }

    public void showDamageLabel(float damageValue, Color color, Roguelike.Tiles.LivingEntities.LivingEntity tile, string text = "")
    {
      updateHealthBar(tile);
      var damageLabel = (DamageLabel)ResourceLoader.Load<PackedScene>("res://ClientScripts/damage_label.tscn").Instantiate();
      var label = (Label)damageLabel.GetChild(0);
      label.Text = (Math.Round(damageValue, 2)).ToString();
      if (text != "")
        label.Text = text;
      sprite.AddChild(damageLabel);
      label.Position = sprite.Position;
      label.Position = new Vector2(label.Position.X - 30, label.Position.Y - 75);
      damageLabel.StartAnimation();
    }
    public void updateHealthBar(Roguelike.Tiles.LivingEntities.LivingEntity tile)
    {
      var hpBar = (Node2D)GetNode("Sprite2D/HpBar/Hp");
      var percentOfHealth = tile.Stats.Health / tile.Stats.GetTotalValue(Roguelike.Attributes.EntityStatKind.Health);
      if (percentOfHealth < 0) percentOfHealth = 0;
      hpBar.Scale = new Vector2((float)percentOfHealth, hpBar.Scale.Y);
    }

    public abstract void getDamaged(float damageValue, bool missed = false);
  }
}
