using Roguelike.Abstract.Multimedia;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public class SoundManager
  {
    ISoundPlayer player;
    string sndToPlay;

    public ISoundPlayer Player
    {
      get { return player; }
      set
      {
        player = value;
      }
    }

    GameManager gm;
    public SoundManager(GameManager gm, Container container)
    {
      this.gm = gm;
      gm.EventsManager.ActionAppended += EventsManager_ActionAppended;
      Player = container.GetInstance<ISoundPlayer>();
    }

    public void PlaySound(string snd)
    {
      if (Player != null)
      {
        if(!string.IsNullOrEmpty(snd))
          Player.PlaySound(snd);
      }
      else
        sndToPlay = snd;
    }

    public void PlayBeepSound()
    {
      PlaySound("beep");
    }

  private void EventsManager_ActionAppended(object sender, Events.GameAction ac)
  {
      if (Player == null)
        return;
      var sndName = "";
      if (ac is EnemyAction)
      {
        var ea = ac as EnemyAction;
        if (ea.Kind == EnemyActionKind.Moved)
        {
          //sndName = "foot_steps";
        }
      }
      else if (ac is LootAction)
      {
        var la = ac as LootAction;
        if (la.LootActionKind == LootActionKind.Consumed)
        {
          sndName = (la.Loot as Consumable).ConsumedSound;
          
        }
        else if (la.LootActionKind == LootActionKind.Collected)
        {
          sndName = la.Loot.CollectedSound;
        }
      }
      else if (ac is InteractiveTileAction)
      {
        var ia = ac as InteractiveTileAction;
        if (ia.InteractiveKind == InteractiveActionKind.Destroyed)
          sndName = ia.InvolvedTile.DestroySound;
        else if (ia.InteractiveKind == InteractiveActionKind.DoorOpened)
          sndName = "door_open";
        else if (ia.InteractiveKind == InteractiveActionKind.DoorClosed)
          sndName = "door_close";
        else //if (ia.InteractiveKind == InteractiveActionKind.ChestOpened)
          sndName = ia.InvolvedTile.InteractSound;
      }
      else if (ac is HeroAction)
      {
        var ha = ac as HeroAction;
        if (ha.Kind == HeroActionKind.HitWall || ha.Kind == HeroActionKind.HitLockedChest)
          sndName = "punch";
        else if (ha.Kind == HeroActionKind.LeveledUp)
          sndName = "bell";
      }
      else if (ac is SoundRequestAction)
      {
        var snd = ac as SoundRequestAction;
        sndName = snd.SoundName;

      }
      
      else if (ac is LivingEntityAction)
      {
        var lea = ac as LivingEntityAction;
        if (lea.Kind == LivingEntityActionKind.Moved)
        {
          if (lea.InvolvedEntity is Hero)
          {
            var sur = gm.CurrentNode.GetSurfaceKindUnderTile(lea.InvolvedEntity);
            if(sur != SurfaceKind.DeepWater && sur != SurfaceKind.ShallowWater)
              sndName = "foot_steps";
          }
        }
        else if (lea.Kind == LivingEntityActionKind.Missed)
        {
          sndName = "melee_missed";
        }
        else if (lea.Kind == LivingEntityActionKind.Died)
        {
          sndName = "death";
          var enemy = lea.InvolvedEntity as Enemy;
          if (enemy != null)
          {
            if (enemy.PowerKind == EnemyPowerKind.Champion)
              sndName = "chemp_death";
            else if (enemy.PowerKind == EnemyPowerKind.Boss)
              sndName = "boss_death";
          }
        }
        //if (lea.Kind == LivingEntityActionKind.GainedDamage)
        //{
        //  Player.PlaySound("punch");
        //}
      }

      if(!string.IsNullOrEmpty(sndName))
        Player.PlaySound(sndName);
    }

  public void PlaySpellCastedSound(Roguelike.Spells.SpellKind spellKind)
  {
      string sndToPlay = "scroll_";
      sndToPlay += spellKind.ToString().ToLower();

      PlaySound(sndToPlay);
  }

  }
}
