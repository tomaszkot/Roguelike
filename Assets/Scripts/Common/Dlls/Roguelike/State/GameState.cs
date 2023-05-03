//using Algorithms;
using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Abstract.Tiles;
using Roguelike.History;
using Roguelike.Serialization;
using Roguelike.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Roguelike.State
{
  public class AlliesStore : IPersistable
  {
    public AlliesStore() { Dirty = true; }
    public List<IAlly> Allies { get; set; } = new List<IAlly>();

    public bool Dirty { get; set; }
  }

  public class CoreInfo : SettingsBase
  {
    public GameMode Mode { get; set; }
    public Difficulty Difficulty { get; set; }
    public string GameVersion { get; set; }
    public bool IsPlayerPermanentlyDead { get; set; }
    public bool PermanentDeath { get; set; }
    public DateTime LastSaved { get; set; }
    public bool Demo { get; set; }

    public CoreInfo()
    {
      GameVersion = Game.Version;
      Difficulty = Generators.GenerationInfo.Difficulty;
    }

    public override string ToString()
    {
      return Difficulty + ", " + PermanentDeath;
    }
  };

  public class GameState : IPersistable
  {
    public GameState()
    {
      HeroPath = CreateHeroPath();
    }

    public virtual HeroPath CreateHeroPath()
    {
      return new HeroPath();
    }

    public CoreInfo CoreInfo { get; set; } = new CoreInfo();
    public HeroPath HeroPath { get; set; }

    Point heroInitGamePosition = new Point().Invalid();
    public Point HeroInitGamePosition 
    {
      get { return heroInitGamePosition; }
      set { heroInitGamePosition = value; } 
    } 
    public HistoryContent History { get; set; } = new HistoryContent();
    public bool QuickSave { get; set; }

    public override string ToString()
    {
      return CoreInfo.ToString() + ";" + HeroPath.ToString();
    }

  }
}
