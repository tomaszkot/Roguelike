using Dungeons.Core;
using Roguelike;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Roguelike.Serialization.JSONPersister;

namespace OuaDII.Serialization
{
  public class StoredGamesProvider
  {
    Container container;

    public StoredGamesProvider(Container container)
    {
      this.container = container;
    }

    public Container Container { get => container; set => container = value; }


    public string GetGameName(string fullFolderPath)
    {
      return Path.GetFileName(fullFolderPath);
    }

    public List<GameChooseInfo> GetSavedGames(bool isGameDemo, string heroName = "")
    {
      var games = new List<GameChooseInfo>();
      var persister = container.GetInstance<JSONPersister>();
      var folderItems = Directory.GetDirectories(persister.GamePath);
      
      foreach (var fullFolderPath in folderItems)
      {
        try
        {
          //var gameName = GetGameName(fullFolderPath);
          var game = new Roguelike.Serialization.SavedGameInfo(new DirectoryInfo(fullFolderPath));
          if (heroName.Any() && game.HeroNameFromGameName != heroName)
          {
            continue;
          }
          var info = GetGameInfo(game);
          if (info.State.CoreInfo.Demo != isGameDemo)
            continue;
          if (info != null)
            games.Add(info);
        }
        catch (Exception ex)
        {
          GetLogger().LogError("fullFolderPath: " + fullFolderPath + ", ex: " + ex);
        }
      }
      return games;
    }

    public bool DeleteGame(string heroName, bool quick)
    {
      var persister = container.GetInstance<JSONPersister>();
      var dir = persister.GetFilesPath(heroName, quick);
      if (Directory.Exists(dir))
      {
        Directory.Delete(dir, true);
        return true;
      }
      return false;
    }

    public GameChooseInfo GetGameInfo(Roguelike.Serialization.SavedGameInfo game)
    {
      var hero = game.HeroNameFromGameName;
      var qs = game.IsQuickSaveFromGameName;

      return GetGameInfo(game, hero, qs);
    }

    public GameChooseInfo GetGameInfo(Roguelike.Serialization.SavedGameInfo game, string hero, bool qs)
    {
      var persister = container.GetInstance<JSONPersister>();
      var fileName = persister.GetFullFilePath(FileKind.Hero, hero, qs);
      if (!File.Exists(fileName))
      {
        GetLogger().LogInfo(fileName + " not a valid game");
        return null;
      }
      var state = persister.LoadGameState(game.HeroNameFromGameName, qs);
      var info = new GameChooseInfo();
      info.HeroName = game.HeroNameFromGameName;
      info.State = state as State.GameState;
      info.SavedGameInfo = game;
      return info;
    }

    private ILogger GetLogger()
    {
      return container.GetInstance<ILogger>();
    }

    public List<string> GetSavedGamesNames()
    {
      var persister = container.GetInstance<JSONPersister>();
      var folderItems = Directory.GetDirectories(persister.GamePath);

      return folderItems.ToList();
    }
  }
}
