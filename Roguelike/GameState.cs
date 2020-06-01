using Newtonsoft.Json;
using Roguelike.Serialization;
using Roguelike.Settings;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike
{
      
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
        
    public RpgGameSettings Settings { get; set; } = new RpgGameSettings();
    public HeroPath HeroPathValue { get; set; } = new HeroPath();

    [JsonIgnore]
    public bool Dirty { get; set; } = true;//TODO true

    public override string ToString()
    {
      return Settings.ToString() + ";" +  HeroPathValue.ToString();
    }

  }
}
