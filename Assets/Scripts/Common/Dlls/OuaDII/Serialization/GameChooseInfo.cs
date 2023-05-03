using System;
using System.Collections.Generic;
using System.Text;

namespace OuaDII.Serialization
{
  public class GameChooseInfo
  {
    public string HeroName { get; set; }
    public State.GameState State { get; set; }
    public Roguelike.Serialization.SavedGameInfo SavedGameInfo { get; internal set; }

    public override string ToString()
    {
      return ToHumanDescription(true);
    }

    public string ToHumanDescription(bool showQuickSave)
    {
      var hero = SavedGameInfo.HeroNameFromGameName;
      if (showQuickSave)
        hero = SavedGameInfo.Name;
      var details = hero + ", " + State.HeroPath + ", " + State.CoreInfo.Difficulty;
      return details;
    }
  }
}
