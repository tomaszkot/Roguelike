using Newtonsoft.Json;
using Roguelike.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
  public class GameInfo
  {
    //public string FileName { get; set; }
    //public Hero Hero { get; set; }
    //public int LastLevelIndex { get; set; }
    public Difficulty Difficulty { get; set; }
    public string GameVersion { get; set; }
    public bool IsPlayerPermanentlyDead { get; set; }
    public bool PermanentDeath { get; set; }
    public DateTime LastSaved { get; set; }
    //public DateTime LastPlayed { get; set; }
    //public GameSession Session = new GameSession();

    public override string ToString()
    {
      return Difficulty + ", "+ PermanentDeath;
    }
  };



  public class GameState:  IPersistable
  {
    public class HeroPath
    {
      public string World { get; set; }
      public string Pit { get; set; } = "";
      public int LevelIndex { get; set; }

      public override string ToString()
      {
        return Pit.Any() ? this.World + " " + Pit + " " + LevelIndex  : World;
      }
    }

    public GameInfo GameInfo { get; set; } = new GameInfo();
    public HeroPath HeroPathValue { get; set; } = new HeroPath();

    [JsonIgnore]
    public bool Dirty { get; set; } = true;//TODO true

    public override string ToString()
    {
      return GameInfo.ToString() + ";" +  HeroPathValue.ToString();
    }

  }
}
