using Roguelike.Serialization;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Roguelike.Settings
{
  public enum GameMode
  {
    Unset,
    Adventure,
    Roguelike
  }

  public enum GameKey
  {
    Unset,
    MoveLeft, MoveRight, MoveUp, MoveDown, Grab, DistanceCollect, SkipTurn, HighlightLoot, SwapActiveWeapon,

    UICharacter, UIHeroInventory, UIAbilities, UIMap, UICrafting, UIQuests, UIAlly,

    SwapActiveHotBar, PreviewSwapActiveHotBar
  }

  public enum GameControllingMode
  {
    MouseAndKeyboard,
    TouchTwoButtons,
    TouchNoButtons,
  };

  public class SettingsBase
  {
    public T DoClone<T>()
    {
      return (T)MemberwiseClone();
    }
  }


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

    public bool VoiceActingOn { get; set; } = true;
  };

  public class Serialization
  {
    /// <summary>
    /// If false Hero will be exactly at the save place (potentially dangerous)
    /// </summary>
    public bool RestoreHeroToSafePointAfterLoad { get; set; } = true;
    public bool RestoreHeroToDungeon { get; set; } = false;//TODO this way loading predefinied levels did not worked in Unity
    public bool RegenerateLevelsOnLoad { get; set; } = true;

    public bool AutoQuickSave { get; set; } = true;
  }

  public class Mechanics : SettingsBase
  {
    //public bool TurnOffSpellAfterUseOnTouchMode { get; set; }
    public bool AutoPutOnBetterEquipment { get; set; } = true;
    public bool AllowEnchantOnDragDrop { get; set; } = true;

    public bool AutoCollectLootOnEntering { get; set; }

    public bool ShowEnemiesLevelOnWorldPitEntries { get; set; } = true;

    public bool KeyIsRequiredToEnterBoosRoom { get; set; } = true;
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

    public bool AllowMovementUsingMouse { get; set; } = true;
  }

  public class View : SettingsBase
  {
    public bool DynamicTerrainLoad { get; set; } = true;
    public bool HintsOn { get; set; } = true;
    public bool ShowShortcuts { get; set; } = true;
    public bool ShowMiniMap { get; set; } = true;
    public bool AnimateHero { get; set; } = true;
    public bool PlaceLootToShortcutBar { get; set; } = true;
    bool useTouchInterface;
    public bool UseTouchInterface
    {
      get { return useTouchInterface; }
      set { useTouchInterface = value; }
    }

    public bool AutoCloseEntityDescriptor { get; set; } = true;
    public bool ShowDungeonKeyHint { get; set; } = true;

    //Darkness embraces hero as he gets hurt
    public bool DarknessEmbracesHero { get; set; } = true;
  }

  public class Colors
  {
    public Dictionary<string, Color> Values = new Dictionary<string, Color>();
  }

  public class UIScheme
  {
    public string SchemeName { get; set; } = "Default";
    public Colors Colors { get; set; }=  new Colors();

    public UIScheme()
    {
      Colors.Values["BasketItemQuantityFont"] = Color.Red;
    }
    //public Color BasketItemQuantityColor { get; set; } = Color.Red;
  }

  public class LookAndFeel : SettingsBase, IPersistable
  {
    public UIScheme SelectedScheme { get; set; } = new UIScheme();

    public bool ShowRecentEvents { get; set; } = true;
  }

  public class Options : SettingsBase, IPersistable
  {
    public SoundMusic SoundMusic { get; set; } = new SoundMusic();
    public Mechanics Mechanics { get; set; } = new Mechanics();
    public Input Input { get; set; } = new Input();
    public View View { get; set; } = new View();

    public LookAndFeel LookAndFeel { get; set; } = new LookAndFeel();

    public Serialization Serialization { get; set; } = new Serialization();

    public Dictionary<GameKey, int> GameKeyMappingsGamePlay { get; set; } = new Dictionary<GameKey, int>();
    public Dictionary<GameKey, int> GameKeyMappingsGamePlayAlt { get; set; } = new Dictionary<GameKey, int>();
    public Dictionary<GameKey, int> GameKeyMappingsUI { get; set; } = new Dictionary<GameKey, int>();

    public static Options Instance { get => instance; }

    static Options instance = new Options();

    public void SetData(Options options)
    {
      var clone = options.Clone();
      SoundMusic = clone.SoundMusic;
      Mechanics = clone.Mechanics;
      Input = clone.Input;
      View = clone.View;
      GameKeyMappingsGamePlay = clone.GameKeyMappingsGamePlay;
      GameKeyMappingsGamePlayAlt = clone.GameKeyMappingsGamePlayAlt;
      GameKeyMappingsUI = clone.GameKeyMappingsUI;
      LookAndFeel = clone.LookAndFeel;
    }

    public Options Clone()
    {
      var clone = DoClone<Options>();
      clone.SoundMusic = clone.SoundMusic.DoClone<SoundMusic>();
      clone.Mechanics = clone.Mechanics.DoClone<Mechanics>();
      clone.Input = clone.Input.DoClone<Input>();
      clone.View = clone.View.DoClone<View>();
      clone.GameKeyMappingsGamePlay = clone.GameKeyMappingsGamePlay.ToDictionary(pair => pair.Key, pair => pair.Value);
      clone.GameKeyMappingsGamePlayAlt = clone.GameKeyMappingsGamePlayAlt.ToDictionary(pair => pair.Key, pair => pair.Value);
      clone.GameKeyMappingsUI = clone.GameKeyMappingsUI.ToDictionary(pair => pair.Key, pair => pair.Value);
      clone.LookAndFeel = clone.LookAndFeel.DoClone<LookAndFeel>();
      return clone;
    }

    public void Save(string fullFilePath)
    {
    }

    public void Load(string fullFilePath)
    {
    }

  }

  public class TestingData : IPersistable
  {
    public int AbilitiesPoints { get; set; } = 20;
    public int LevelUpPoints { get; set; } = 30;

    public int HeroLevel { get; set; } = 5;
  }

}
