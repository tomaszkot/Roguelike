//using Algorithms;
using Dungeons.Core;
using Newtonsoft.Json;
using Roguelike.Abstract;
using Roguelike.Abstract.Tiles;
using Roguelike.History;
using Roguelike.Serialization;
using Roguelike.Settings;
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

  public class GameState : IPersistable
  {
    public GameState()
    {
      CoreInfo.GameVersion = Game.Version;
      HeroPath = CreateHeroPath();
    }

    public virtual HeroPath CreateHeroPath()
    {
      return new HeroPath();
    }

    public CoreInfo CoreInfo { get; set; } = new CoreInfo();
    public HeroPath HeroPath { get; set; }
    public Point HeroInitGamePosition { get; set; } = new Point().Invalid();
    public HistoryContent History { get; set; } = new HistoryContent();
    //public Options Settings { get { return Options.Instance; } }
    //[JsonIgnore]
    //public bool Dirty { get; set; } = true;

    public override string ToString()
    {
      return CoreInfo.ToString() + ";" + HeroPath.ToString();
    }

  }
}
