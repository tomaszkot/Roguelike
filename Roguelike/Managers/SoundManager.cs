using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public interface ISoundPlayer
  {
    void PlaySound(string sound);
    void StopSound();
  }

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

    public SoundManager(GameManager gm, Container container)
    {
      gm.EventsManager.ActionAppended += EventsManager_ActionAppended;
      Player = container.GetInstance<ISoundPlayer>();
    }

    public void PlaySound(string snd)
    {
      if (Player != null)
        Player.PlaySound(snd);
      else
        sndToPlay = snd;
    }

  private void EventsManager_ActionAppended(object sender, Events.GameAction e)
  {
      if (Player == null)
        return;
      if (e is EnemyAction)
      {
        var ea = e as EnemyAction;
        if (ea.Kind == EnemyActionKind.Moved)
          return;
        if (ea.Kind == EnemyActionKind.Died)
        {
          var sndName = "death";
          if (ea.Enemy.PowerKind == EnemyPowerKind.Champion)
            sndName = "chemp_death";
          else if (ea.Enemy.PowerKind == EnemyPowerKind.Boss)
            sndName = "boss_death";
          Player.PlaySound(sndName);
        }
      }
      else if (e is LootAction)
      {
        var la = e as LootAction;
        if (la.LootActionKind == LootActionKind.Consumed)
        {
          if(la.Loot is Potion /*|| la.Loot is Bibmer*/)
            Player.PlaySound("drink");
          else
            Player.PlaySound("eat_chip");
        }
      }
      else if (e is InteractiveTileAction)
      {
        //var door = (e.EventData as DoorStateChangedAction).Door;
        //if (door.Kind == Tiles.DoorKind.LeverOpened)
        //{
        //  //played in RPG Door Open()/Close()
        //}
        //else
        //{
        //  if (door.Secret)
        //    Player.PlaySound("annulet-of-absorption");
        //  if (door.SealLock)
        //    Player.PlaySound("door_unlock");//TODO use on panel
        //  else
        //    Player.PlaySound("door_open");
        //}
      }
      else if (e is HeroAction)
      {
        var ha = e as HeroAction;
        if (ha.Kind == HeroActionKind.Moved)
          Player.PlaySound("foot_steps");
      }
      else if (e is SoundRequestAction)
      {
        var snd = e as SoundRequestAction;
        if (!string.IsNullOrEmpty(snd.SoundName))
          Player.PlaySound(snd.SoundName);
      }
      else if (e is LivingEntityAction)
      {
        //var lea = e as LivingEntityAction;
        //if (lea.Kind == LivingEntityActionKind.GainedDamage)
        //{
        //  Player.PlaySound("punch");
        //}
      }
    }
  }
}
