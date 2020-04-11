namespace Roguelike
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

  //[Serializable]
  public class RpgGameSettings 
  {
    GameControllingMode gameControllingMode = GameControllingMode.MouseAndKeyboard;
    public bool TurnOffSpellAfterUseOnTouchMode { get; set; }
    public bool SoundOn { get; set; }
    public bool MusicOn { get; set; }
    public bool TipsOn { get; set; }
    public bool GodsVoiceOn { get; set; }
    public bool ShowShortcuts { get; set; }
    public bool AutomaticallyRestMerchantInv { get; set; }
    public float DefaultMusicVolume
    {
      get;
      set;
    }

    public float DefaultSoundVolume
    {
      get;
      set;
    }

    public GameControllingMode GameControllingMode
    {
      get
      {
        return gameControllingMode;
      }

      set
      {
        gameControllingMode = value;
        if (gameControllingMode == GameControllingMode.TouchTwoButtons)
        {
          //int k = 0;
        }
      }
    }
    bool autoPutOnBetterEquipment;
    bool allowInPlaceInventoryCrafting = true;

    public bool AutoPutOnBetterEquipment
    {
      get { return autoPutOnBetterEquipment; }
      set { autoPutOnBetterEquipment = value; }
    }

    public bool AllowInPlaceInventoryCrafting { get => allowInPlaceInventoryCrafting; set => allowInPlaceInventoryCrafting = value; }

    public RpgGameSettings()
    {
      AutoPutOnBetterEquipment = true;
      gameControllingMode = GameControllingMode.MouseAndKeyboard;
      SoundOn = true;
      MusicOn = true;
      TipsOn = true;
      GodsVoiceOn = true;
      AutomaticallyRestMerchantInv = true;
      DefaultMusicVolume = .25f;
      DefaultSoundVolume = 1f;
      ShowShortcuts = true;
    }

    //public bool PermanentDeath { get; set; }

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
