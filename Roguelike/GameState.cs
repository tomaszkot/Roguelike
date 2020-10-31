//using Algorithms;
using Newtonsoft.Json;
using Roguelike.Serialization;
using Roguelike.Settings;
using System.Drawing;
using System.Linq;

namespace Roguelike
{
  public class GameState : IPersistable
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
    public Point HeroInitGamePosition { get; set; }
    public History History { get; set; } = new History();

    [JsonIgnore]
    public bool Dirty { get; set; } = true;//TODO true

    public override string ToString()
    {
      return Settings.ToString() + ";" +  HeroPathValue.ToString();
    }

  }
}
