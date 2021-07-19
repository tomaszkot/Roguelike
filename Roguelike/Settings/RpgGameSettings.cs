using System;

namespace Roguelike.Settings
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

  public class SettingsBase
  {
    public T DoClone<T>()
    {
      return (T)MemberwiseClone();
    }
  }

  public class CoreInfo : SettingsBase
  {
    public Difficulty Difficulty { get; set; }
    public string GameVersion { get; set; }
    public bool IsPlayerPermanentlyDead { get; set; }
    public bool PermanentDeath { get; set; }
    public DateTime LastSaved { get; set; }
    public bool RestoreHeroToSafePointAfterLoad { get; set; } = true;
    public bool RestoreHeroToDungeon { get; set; } = false;////TODO this way loading predefinied levels did not worked in Unity
    public bool RegenerateLevelsOnLoad { get; set; } = true;
    public static bool Demo { get; set; } = true;
    //public GameSession Session = new GameSession();

    public override string ToString()
    {
      return Difficulty + ", " + PermanentDeath;
    }
  };

  public class SoundMusic : SettingsBase
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

  public class Mechanics : SettingsBase
  {
    public bool TurnOffSpellAfterUseOnTouchMode { get; set; }
    public bool AutoPutOnBetterEquipment { get; set; } = true;
    public bool AllowInPlaceInventoryCrafting { get; set; } = true;
    public bool PlaceLootToShortcutBar { get; set; } = true;
  }

  public class Input : SettingsBase
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

  public class View : SettingsBase
  {
    public bool HintsOn { get; set; } = true;
    public bool ShowShortcuts { get; set; } = true;
  }

  //[Serializable]
  public class RpgGameSettings : SettingsBase
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
      return CoreInfo.ToString() + base.ToString();
    }

    public RpgGameSettings Clone()
    {
      RpgGameSettings clone = DoClone<RpgGameSettings>();
      clone.CoreInfo = clone.CoreInfo.DoClone<CoreInfo>();
      clone.SoundMusic = clone.SoundMusic.DoClone<SoundMusic>();
      clone.Mechanics = clone.Mechanics.DoClone<Mechanics>();
      clone.Input = clone.Input.DoClone<Input>();
      clone.View = clone.View.DoClone<View>();
      return clone;
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
