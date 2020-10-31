using System;

namespace Roguelike
{
  namespace Settings
  {
    /// <summary>
    /// These values shall not be renamed cause old saves will not work anymore!
    /// </summary>
    public enum GameControllingMode
    {
      TwoButtons,//deprecated the same as MouseAndKeyboard
      MouseAndKeyboard,
      TouchNoButtons,
      TouchTwoButtons
    };

    public class CoreInfo
    {
      public Difficulty Difficulty { get; set; }
      public string GameVersion { get; set; }
      public bool IsPlayerPermanentlyDead { get; set; }
      public bool PermanentDeath { get; set; }
      public DateTime LastSaved { get; set; }
      //public GameSession Session = new GameSession();

      public override string ToString()
      {
        return Difficulty + ", " + PermanentDeath;
      }
    };

    public class SoundMusic
    {
      public bool SoundOn { get; set; } = true;
      public bool MusicOn { get; set; } = true;
      public bool GodsVoiceOn { get; set; } = true;

      public float DefaultMusicVolume
      {
        get;
        set;
      } = .25f;

      public float DefaultSoundVolume
      {
        get;
        set;
      } = 1f;
    };

    public class Mechanics
    {
      public bool TurnOffSpellAfterUseOnTouchMode { get; set; }
      public bool AutoPutOnBetterEquipment { get; set; } = true;
      public bool AllowInPlaceInventoryCrafting { get; set; } = true;
      public bool PlaceLootToShortcutBar { get; set; } = true;
    }

    public class Input
    {
      GameControllingMode gameControllingMode = GameControllingMode.MouseAndKeyboard;
      public GameControllingMode GameControllingMode
      {
        get
        {
          return gameControllingMode;
        }

        set
        {
          gameControllingMode = value;
        }
      }
    }

    public class View
    {
      public bool HintsOn { get; set; } = true;
      public bool ShowShortcuts { get; set; } = true;
    }

    //[Serializable]
    public class RpgGameSettings
    {
      public CoreInfo CoreInfo { get; set; } = new CoreInfo();
      public SoundMusic SoundMusic { get; set; } = new SoundMusic();
      public Mechanics Mechanics { get; set; } = new Mechanics();
      public Input Input { get; set; } = new Input();
      public View View { get; set; } = new View();

      public RpgGameSettings()
      {
      }

      public override string ToString()
      {
        return CoreInfo.ToString() +  base.ToString();
      }

      //void Save()
      //{
      //	PlayerPrefs.SetInt("DifficultyLevel", (int)DifficultyLevel);
      //	PlayerPrefs.SetInt("EverShownToUser", EverShownToUser ? 1 : 0);
      //}

      //void Load()
      //{
      //	difficultyLevel = (Difficulty)PlayerPrefs.GetInt("DifficultyLevel", (int)Difficulty.Easy);
      //	everShownToUser = PlayerPrefs.GetInt("EverShownToUser", 1) == 1 ? true : false;
      //}
    }
  }
}
