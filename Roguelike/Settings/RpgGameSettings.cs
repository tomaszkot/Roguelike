using Roguelike.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Roguelike.Settings
{
  public enum GameKey { Unset, MoveLeft, MoveRight, MoveUp, MoveDown, Grab, SkipTurn, HighlightLoot }

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
    public static bool Demo { get; set; } = true;
    //public GameSession Session = new GameSession();

    public override string ToString()
    {
      return Difficulty + ", " + PermanentDeath;
    }
  };

  public class SoundMusic : SettingsBase
  {
    //public bool SoundOn { get; set; } = true;
    //public bool MusicOn { get; set; } = true;
    public bool GodsVoiceOn { get; set; } = true;

    public float MusicVolume
    {
      get;
      set;
    } = .25f;

    public float SoundVolume
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
    public bool RestoreHeroToSafePointAfterLoad { get; set; } = true;
    public bool RestoreHeroToDungeon { get; set; } = false;////TODO this way loading predefinied levels did not worked in Unity
    public bool RegenerateLevelsOnLoad { get; set; } = true;
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

  public class Options : SettingsBase, IPersistable
  {
    public SoundMusic SoundMusic { get; set; } = new SoundMusic();
    public Mechanics Mechanics { get; set; } = new Mechanics();
    public Input Input { get; set; } = new Input();
    public View View { get; set; } = new View();
    public Dictionary<GameKey, int> GameKeysMapping { get; set; } = new Dictionary<GameKey, int>();

    public static Options Instance { get => instance; }

    static Options instance = new Options();

    //private Options()
    //{ 
    
    //}

    public void SetData(Options options)
    {
      var clone = options.Clone();
      SoundMusic = clone.SoundMusic;
      Mechanics = clone.Mechanics;
      Input = clone.Input;
      View = clone.View;
      GameKeysMapping = clone.GameKeysMapping;
    }

    public Options Clone()
    {
      var clone = DoClone<Options>();
      clone.SoundMusic = clone.SoundMusic.DoClone<SoundMusic>();
      clone.Mechanics = clone.Mechanics.DoClone<Mechanics>();
      clone.Input = clone.Input.DoClone<Input>();
      clone.View = clone.View.DoClone<View>();
      clone.GameKeysMapping = clone.GameKeysMapping.ToDictionary(pair => pair.Key, pair => pair.Value);
      return clone;
    }

    public void Save(string fullFilePath)
    { 
    }

    public void Load(string fullFilePath)
    {
    }

  }

  //[Serializable]
  //public class RpgGameSettings : SettingsBase
  //{
  //  public CoreInfo CoreInfo { get; set; }  = new CoreInfo();
  //}

    //  //public Options Options { get { return Options.Instance; } }

    //  public RpgGameSettings()
    //  {
    //  }

    //  public override string ToString()
    //  {
    //    return CoreInfo.ToString() + base.ToString();
    //  }

    //  public RpgGameSettings Clone()
    //  {
    //    RpgGameSettings clone = DoClone<RpgGameSettings>();
    //    clone.CoreInfo = clone.CoreInfo.DoClone<CoreInfo>();
    //    clone.Options = clone.Options.DoClone<Options>();
    //    return clone;
    //  }

    //  //void Save()
    //  //{
    //  //	PlayerPrefs.SetInt("DifficultyLevel", (int)DifficultyLevel);
    //  //	PlayerPrefs.SetInt("EverShownToUser", EverShownToUser ? 1 : 0);
    //  //}

    //  //void Load()
    //  //{
    //  //	difficultyLevel = (Difficulty)PlayerPrefs.GetInt("DifficultyLevel", (int)Difficulty.Easy);
    //  //	everShownToUser = PlayerPrefs.GetInt("EverShownToUser", 1) == 1 ? true : false;
    //  //}
    //}
  }
