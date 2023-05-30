using Godot;
using System;
using Roguelike.Events;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.Looting;
using Roguelike.Tiles;
using GodotGame.Entities;

namespace God4_1.ClientScripts
{
  public class GameEventHandler
  {
    public void ActionsManager_ActionAppended(object sender, GameEvent ev)
    {
      switch (ev)
      {
        case LivingEntityAction:
          {
            var lea = ev as LivingEntityAction;
            if (lea.Kind == LivingEntityActionKind.Moved)
            {
              if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Hero)
              {
                Game.SetPositionFromTile(Game.hero.HeroTile, Game.hero.sprite, true);
              }
              else if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Enemy en)
              {
                var enGodotNode = (GodotGame.Entities.Enemy)Game.gameLevel.GetNode(en);
                var enGodot = enGodotNode.sprite;
                Game.SetPositionFromTile(en, enGodot, true);
                enGodot.Scale = new Vector2(-1, 1);

                var nextPosition = Game.hero.sprite.Position;
                if (enGodot.GlobalPosition.X > nextPosition.X)
                {
                  enGodot.Scale = new Vector2(1, 1);
                }
                else
                {
                  enGodot.Scale = new Vector2(-1, 1);
                }
              }
            }
            else if (lea.Kind == LivingEntityActionKind.Died && lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Enemy enemy)
            {
              var enGodot = Game.gameLevel.GetNode(enemy);
              enGodot.CallDeferred("queue_free");
            }
            else if (lea.Kind == LivingEntityActionKind.Died && lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Hero hero)
            {
              Game.gui.ShowDeathScreen();
            }
            else if (lea.Kind == LivingEntityActionKind.GainedDamage)
            {
              if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Enemy en)
              {
                var enGodot = (GodotGame.Entities.Enemy)Game.gameLevel.GetNode(en);
                enGodot.getDamaged((float)lea.InvolvedValue);
              }
              else if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Hero)
              {
                Game.hero.getDamaged((float)lea.InvolvedValue);
              }
            }
            else if (lea.Kind == LivingEntityActionKind.Missed)
            {
              if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Enemy en)
              {
                Game.hero.getDamaged((float)lea.InvolvedValue, true);
              }
              else if (lea.InvolvedEntity is Roguelike.Tiles.LivingEntities.Hero)
              {
                var targetTile = Game.dungeon.GetTile(lea.targetEntityPosition);
                var enGodot = (GodotGame.Entities.Enemy)Game.gameLevel.GetNode((Roguelike.Tiles.LivingEntities.Enemy)targetTile);
                enGodot.getDamaged((float)lea.InvolvedValue, true);
              }
            }
            else if (lea.Kind == LivingEntityActionKind.AppendedToLevel)
            {
              Dungeons.Tiles.Tile[,] tileArray = new Dungeons.Tiles.Tile[1, 1];
              tileArray[0, 0] = lea.InvolvedEntity;
              Game.gameLevel.CreateEntities(tileArray);
            }
          }

          break;

        case InteractiveTileAction:
          {
            var ita = ev as InteractiveTileAction;
            if (ita.InteractiveKind == InteractiveActionKind.DoorOpened)
            {
              GameLevel.AddTile(ita.InvolvedTile);
            }
            else if (ita.InteractiveKind == InteractiveActionKind.Destroyed)
            {
              var barrel = Game.gameLevel.GetNode(ita.InvolvedTile);
              if (ita.InvolvedTile is Barrel)
              {
                var anim = (AnimationPlayer)barrel.GetNode("AnimationPlayer");
                anim.Play("destroy");
              }
              else if (ita.InvolvedTile is Roguelike.Tiles.Interactive.Chest)
              {
                var chest = Game.gameLevel.GetNode(ita.InvolvedTile);
                chest.CallDeferred("queue_free");
              }
            }
            else if (ita.InteractiveKind == InteractiveActionKind.ChestOpened)
            {
              var chest = (Chest)Game.gameLevel.GetNode(ita.InvolvedTile);
              chest.updateChestTexture((Roguelike.Tiles.Interactive.Chest)ita.InvolvedTile);
            }
            break;
          }
        case GameStateAction:
          {
            var gsa = ev as GameStateAction;
            break;
          }
        case LootAction:
          {
            var la = ev as LootAction;
            if (la.Kind == LootActionKind.Generated)
            {
              if (la.Loot is Equipment i)
              {
                if (i is Weapon w)
                {
                  Game.gameLevel.AddChildFromScene(Game.dungeon.GetTile(w.point), "res://Entities/equipment_item.tscn");
                }
              }
            }
            else if (la.Kind == LootActionKind.Collected)
            {
              var item = la.Loot;
              var godotObject = Game.gameLevel.GetNode(item);
              if (godotObject != null)
                godotObject.QueueFree();
              else
                throw new Exception("Item not implemented in Godot");
            }
            break;
          }
      }
      Game.logContainer.showNewLog();
    }
  }
}
