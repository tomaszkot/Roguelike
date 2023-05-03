using Roguelike.Abstract.Multimedia;
using Roguelike.Events;
using Roguelike.Tiles;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;

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
    public event EventHandler<string> PlayedSound;
    public SoundManager(GameManager gm, Container container)
    {
      this.gm = gm;
      gm.EventsManager.EventAppended += EventsManager_ActionAppended;
      ISoundPlayer player;
      if (container.TryGetInstance<ISoundPlayer>(out player))
        Player = player;
    }

    public void PlayVoice(string voice)
    {
      var path = "Voices/";
      PlaySound(path+voice);
    }

    public void PlaySound(string snd)
    {
      if (Player != null)
      {
        if (!string.IsNullOrEmpty(snd))
        {
          Player.PlaySound(snd);
          if (PlayedSound != null)
            PlayedSound(this, snd);
         }
      }
      else
        sndToPlay = snd;
    }

    public void PlayBeepSound()
    {
      PlaySound("beep");
    }

    private void EventsManager_ActionAppended(object sender, Events.GameEvent ac)
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
        if (la.Kind == LootActionKind.Consumed)
        {
          sndName = (la.Loot as Consumable).ConsumedSound;

        }
        else if (la.Kind == LootActionKind.Collected)
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
        {
          var door = ia.InvolvedTile as Tiles.Interactive.Door;
          sndName = door.Secret ? "annulet-of-absorption" : door.OpenedSound();
        }
        else if (ia.InteractiveKind == InteractiveActionKind.DoorClosed)
        {
          var door = ia.InvolvedTile as Tiles.Interactive.Door;
          sndName = door.ClosedSound();
        }
        else if (ia.InteractiveKind == InteractiveActionKind.HitWhenLooted)
        {
          sndName = "door-creaking2";// "Hit_Wood";
        }
        else if (ia.InteractiveKind == InteractiveActionKind.AppendedToLevel)
          sndName = ia.InvolvedTile.AppendedSound;
        else //if (ia.InteractiveKind == InteractiveActionKind.Hit)
          sndName = ia.InvolvedTile.InteractSound;
      }
      else if (ac is HeroAction)
      {
        var ha = ac as HeroAction;
        if (ha.Kind == HeroActionKind.HitWall || ha.Kind == HeroActionKind.HitLockedChest)
          sndName = "punch";
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
            var surs = gm.CurrentNode.GetSurfaceKindsUnderTile(lea.InvolvedEntity);
            if (!surs.Contains(SurfaceKind.DeepWater) && !surs.Contains(SurfaceKind.ShallowWater))
              sndName = "foot_steps";
          }
        }
        else if (lea.Kind == LivingEntityActionKind.LeveledUp)
          sndName = "bell";
        else if (lea.Kind == LivingEntityActionKind.Missed)
        {
          sndName = "melee_missed";
        }
        else if (lea.Kind == LivingEntityActionKind.Died)
        {
          sndName = lea.InvolvedEntity.DestroySound;
        }
        //if (lea.Kind == LivingEntityActionKind.GainedDamage)
        //{
        //  Player.PlaySound("punch");
        //}
      }

      if (!string.IsNullOrEmpty(sndName))
        PlaySound(sndName);
    }

    public void PlaySpellCastedSound(Roguelike.Spells.SpellKind spellKind)
    {
      string sndToPlay = "scroll_";
      sndToPlay += spellKind.ToString().ToLower();

      PlaySound(sndToPlay);
    }

  }
}
