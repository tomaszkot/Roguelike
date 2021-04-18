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
using System.Linq;

namespace Roguelike.State
{
  public class AlliesStore : IPersistable
  {
    public AlliesStore() { Dirty = true; }
    public List<IAlly> Allies { get; set; } = new List<IAlly>();

    public bool Dirty { get ; set ; }
  }

  public class GameState : IPersistable
  {
    public GameState()
    {
      Settings.CoreInfo.GameVersion = Game.Version;
      HeroPath = CreateHeroPath();
    }

    public virtual HeroPath CreateHeroPath()
    {
      return new HeroPath();
    }

    public RpgGameSettings Settings { get; set; } = new RpgGameSettings();
    public HeroPath HeroPath { get; set; }
    public Point HeroInitGamePosition { get; set; } = new Point().Invalid();
    public HistoryContent History { get; set; } = new HistoryContent();

    [JsonIgnore]
    public bool Dirty { get; set; } = true;//TODO true

    public override string ToString()
    {
      return Settings.ToString() + ";" +  HeroPath.ToString();
    }

  }
}
