using Dungeons.ASCIIDisplay;
using Godot;
using Roguelike.Abstract.Multimedia;
using Roguelike.Abstract;
using Roguelike;
using Roguelike.Managers;
using Roguelike.Multimedia;
using System;
using Roguelike.Events;
using System.IO;
using Dungeons.Tiles;
using Roguelike.Tiles.LivingEntities;
using static System.Net.WebRequestMethods;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Roguelike.Tiles.Interactive;
using System.Drawing;
using Dungeons.TileContainers;
namespace God4_1.ClientScripts
{
  public class GameEventHandler
  {
    public void ActionsManager_ActionAppended(object sender, GameEvent ev)
    {
      if (ev is LivingEntityAction)
      {
        var lea = ev as LivingEntityAction;
        if (lea.Kind == LivingEntityActionKind.Moved)
        {
          if (lea.InvolvedEntity is Hero)
          {
            Game.SetPositionFromTile(Game.hero.HeroTile, Game.hero, true);
          }
          if (lea.InvolvedEntity is Enemy en)
          {
            var enGodot = GameLevel.enemyList.SingleOrDefault(i => i.EnemyTile == en);
            Game.SetPositionFromTile(en, enGodot, true);
          }
        }
        if (lea.Kind == LivingEntityActionKind.Died && lea.InvolvedEntity is Enemy enemy)
        {
          var enGodot = GameLevel.enemyList.SingleOrDefault(i => i.EnemyTile == enemy);
          enGodot.GetParent().QueueFree();
        }
        if (lea.Kind == LivingEntityActionKind.GainedDamage)
        {
          if (lea.InvolvedEntity is Enemy en)
          {
            var enGodot = GameLevel.enemyList.SingleOrDefault(i => i.EnemyTile == en);
            enGodot.updateHpBar((float)lea.InvolvedValue);
          }
          if (lea.InvolvedEntity is Hero)
          {
            Game.hero.ShowDamageLabel((float)lea.InvolvedValue);
          }
        }
        if (lea.Kind == LivingEntityActionKind.Missed)
        {
          if (lea.InvolvedEntity is Enemy en)
          {
            Game.hero.ShowDamageLabel((float)lea.InvolvedValue, "Evaded");
          }
          if (lea.InvolvedEntity is Hero)
          {
            var targetTile = Game.dungeon.GetTile(lea.targetEntityPosition);
            var enGodot = GameLevel.enemyList.SingleOrDefault(i => i.EnemyTile == targetTile);
            enGodot.updateHpBar((float)lea.InvolvedValue, "Evaded");
          }
        }
      }
      else if (ev is InteractiveTileAction)
      {
        var ita = ev as InteractiveTileAction;
        if (ita.InteractiveKind == InteractiveActionKind.DoorOpened)
        {
          GameLevel.AddTile(ita.InvolvedTile);
        }
        if (ita.InteractiveKind == InteractiveActionKind.Destroyed) 
        {
          var barrel = GameLevel.interactiveList[(Barrel)ita.InvolvedTile];
          var anim = (AnimationPlayer)barrel.GetNode("AnimationPlayer");
          anim.Play("destroy");
        }
      }
      else if (ev is GameStateAction)
      {

      }
      else if (ev is LootAction)
      {

      }

    }
  }
}
