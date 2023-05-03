
using Dungeons.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Roguelike.Settings;
using Roguelike.State;
using Roguelike.TileContainers;
using Roguelike.Tiles.LivingEntities;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Roguelike.Serialization
{

  public interface IPersistable
  {
    //bool Dirty { get; set; }
  }

  public class SavedGameInfo
  {
    DirectoryInfo DirectoryInfo { get; set; }

    public SavedGameInfo(DirectoryInfo directoryInfo)
    {
      DirectoryInfo = directoryInfo;
    }

    public string Name 
    {
      get { return this.DirectoryInfo.Name; }
    }
    public bool IsQuickSaveFromGameName 
    {
      get { return IsQuickSaveFromGameNameAsParam(Name); }
    }

    public static bool IsQuickSaveFromGameNameAsParam(string gameName)
    {
      return gameName.EndsWith(JSONPersister.QuickSave); 
    }

    public string HeroNameFromGameName
    {
      get
      {
        return GetHeroNameFromGameName(Name, IsQuickSaveFromGameName);
      }
    }

    public static string GetHeroNameFromGameName(string gameName, bool quick)
    {
      var heroName = gameName;
      if (quick)
        heroName = heroName.Remove(heroName.LastIndexOf(JSONPersister.QuickSave));
      return heroName.Trim();
    }
  }

  public class JSONPersister : IPersister
  {
    public const string QuickSave = "(quick save)";
    protected const string extension = ".json";
    public enum FileKind { Hero, GameLevel, GameState, Allies, Options }

    public JSONPersister(Container container)
    {
      this.Container = container;
      container.GetInstance<ILogger>().LogInfo("JSONPersister ctor [container]: " + this.Container.GetHashCode());
    }

    public void Save<T>(T entity, string filePath)
    {
      try
      {
        //Debug.Log("Engine_JsonSerializer...");
        var json = MakeJson<T>(entity);
        Save(json, filePath);
      }
      catch (Exception ex)
      {
        this.Container.GetInstance<ILogger>().LogError(ex);
        throw;
      }
    }

    public void Save(string json, string filePath)
    {
      try
      {
        var path = Path.GetDirectoryName(filePath);
        Directory.CreateDirectory(path);
        File.WriteAllText(filePath, json);

      }
      catch (Exception ex)
      {
        this.Container.GetInstance<ILogger>().LogError(ex);
        throw;
      }
    }

    public string MakeHero(Hero hero)//;//, bool quick)
    {
      return MakeJson<Hero>(hero);
    }

    public string MakeJson<T>(T entity)
    {
      try
      {
        var tr = new TimeTracker();
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        //settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        settings.Error += (object sender, Newtonsoft.Json.Serialization.ErrorEventArgs args) =>
        {
          // only log an error once
          if (args.CurrentObject == args.ErrorContext.OriginalObject)
          {
            this.Container.GetInstance<ILogger>().LogError(args.ErrorContext.Error.Message);
            throw new Exception(args.ErrorContext.Error.Message);
          }
        };

        var json = JsonConvert.SerializeObject(entity, settings);
        if (tr.TotalSeconds > 0.3)
        {
          this.Container.GetInstance<ILogger>().LogInfo("MakeJson " + entity + ", tr: " + tr.TotalSeconds);
        }
        return json;
      }
      catch (Exception ex)
      {
        this.Container.GetInstance<ILogger>().LogError(ex);
        throw;
      }
    }


    public T Load<T>(string filePath, Container container) where T : class, IPersistable
    {
      T entity = null;

      try
      {
        var json = File.ReadAllText(filePath);
        //this.container.GetInstance<ILogger>().LogInfo("Engine_JsonDeserializer...");

        if (!IsValidJson(json))
        {
          this.Container.GetInstance<ILogger>().LogError("param json is not a valid json!");
          return null;
        }
        ITraceWriter traceWriter = null;// new MemoryTraceWriter();
        JsonSerializerSettings settings = new JsonSerializerSettings
        { TraceWriter = traceWriter, TypeNameHandling = TypeNameHandling.All };
        //container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
        //settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        settings.ContractResolver = new SimpleInjectorContractResolver(container);
        settings.ObjectCreationHandling = ObjectCreationHandling.Replace;
        entity = JsonConvert.DeserializeObject<T>(json, settings);

        //string outdata = traceWriter.ToString();
        //Console.WriteLine(outdata);
        //if (siFromJson.Hero.CurrentEquipment.ContainsKey(EquipmentKind.Weapon))
        //  Debug.Log("Engine_JsonDeserializer weapon = " + siFromJson.Hero.CurrentEquipment[EquipmentKind.Weapon]);
        //else
        //  Debug.Log("Engine_JsonDeserializer no weapon ");
      }
      catch (Exception ex)
      {
        this.Container.GetInstance<ILogger>().LogError(ex);
        throw;
      }

      return entity;
    }
        
    private static bool IsValidJson(string strInput)
    {
      strInput = strInput.Trim();
      if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
          (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
      {
        try
        {
          JToken.Parse(strInput);
          return true;
        }
        catch (JsonReaderException)
        {
          //Exception in parsing json
          return false;
        }
        catch (Exception) //some other exception
        {
          return false;
        }
      }
      else
        return false;
    }

    //Application.persistentDataPath
    public static string RootPath { get; set; } = Path.Combine(Path.GetTempPath(), GameName);

    public string GamePath
    {
      get { return RootPath /*+ GameFolder*/; }
    }

    public string GetFullFilePath(FileKind fileKind, string heroName, bool quick, string fileSuffix = "")
    {
      if (fileKind == FileKind.Options)
        return GamePath;
      List<string> parts = new List<string>();
      var path = GetFilesPath(heroName, quick);
      parts.Add(path);
      //if (supportManyLevels && fileKind == FileKind.GameLevel)
      if (GameLevelsFolder.Any() && fileKind == FileKind.GameLevel)
        parts.Add(GameLevelsFolder);

      var fileName = fileKind.ToString() + fileSuffix + extension;
      parts.Add(fileName);

      return Path.Combine(parts.ToArray());
    }

    public static string GetQuickSaveGameName(string heroName)
    {
      return heroName + " " + QuickSave;
    }

    public static string GetGameName(string heroName, bool quick)
    {
      var heroNameToUse = heroName;
      if (quick)
        heroNameToUse = GetQuickSaveGameName(heroNameToUse);
      return heroNameToUse;
    }

    public virtual string GetFilesPath(string heroName, bool quick)
    {
      var gn = GetGameName(heroName,  quick);
      return Path.Combine(new[] { GamePath, gn });
    }

    protected virtual string GameLevelsFolder { get { return ""; } }

    protected static string GameName
    {
      get { return "Roguelike"; }
    }

    [JsonIgnore]
    public Container Container { get; private set; }

    //protected virtual string GameFolder { get { return GameName; } }

    public void SaveAllies(string hero, AlliesStore allies, bool quick)
    {
      var fileName = GetFullFilePath(FileKind.Allies, hero, quick);
      Save<AlliesStore>(allies, fileName);
    }

    public void SaveHero(Hero hero, bool quick)
    {
      var fileName = GetFullFilePath(FileKind.Hero, hero.Name, quick);
      Save<Hero>(hero, fileName);
    }

    public void SaveHero(string json, string heroName, bool quick)
    {
      var fileName = GetFullFilePath(FileKind.Hero, heroName, quick);
      Save(json, fileName);
    }

    public AlliesStore LoadAllies(string hero, bool quick)
    {
      var fileName = GetFullFilePath(FileKind.Allies, hero, quick);
      return Load<AlliesStore>(fileName, Container);
    }

    public Hero LoadHero(string heroName, bool quick)
    {
      var fileName = GetFullFilePath(FileKind.Hero, heroName, quick);
      return Load<Hero>(fileName, Container);
    }

    public virtual void DeleteGame(string heroName, bool quick)
    {
      var srcFilePath = GetFullFilePath(FileKind.Hero, heroName, quick);
      var tmpPath = Path.GetTempPath();
      var destPath = Path.Combine(tmpPath, heroName + "_" + DateTime.Now.ToString("YYMMDDhhmmss") + ".json");
      File.Move(srcFilePath, destPath);
    }

    public void SaveLevel(string heroName, GameLevel level, bool quick)
    {
      var filePath = GetFullFilePath(FileKind.GameLevel, heroName, quick, level.Index.ToString());// GetLevelFileName(level.Index));
      Save(level, filePath);
    }

    //private string GetLevelFileName(int levelIndex)
    //{
    //  return "DungeonLevel" + levelIndex + extension;
    //}

    public GameLevel LoadLevel(string heroName, int index, bool quick)
    {
      var filePath = GetFullFilePath(FileKind.GameLevel, heroName, quick, index.ToString());
      return Load<GameLevel>(filePath, Container);
    }

    public void SaveGameState(string heroName, GameState gameState, bool quick)
    {
      var filePath = GetFullFilePath(FileKind.GameState, heroName, quick);
      Save<GameState>(gameState, filePath);
      Container.GetInstance<ILogger>().LogInfo("JSONPersister SaveGameState done, path : " + filePath);
    }

    public GameState LoadGameState(string heroName, bool quick)
    {
      var filePath = GetFullFilePath(FileKind.GameState, heroName, quick);
      return Load<GameState>(filePath, Container);
    }

    public void SaveOptions(Options opt)
    {
      var filePath = Path.Combine(GamePath, "Options"+ extension);
      Save<Options>(opt, filePath);
    }

    public Options LoadOptions()
    {
      var filePath = Path.Combine(GamePath, "Options" + extension);
      if (File.Exists(filePath))
      {
        try
        {
          return Load<Options>(filePath, Container);
        }
        catch (Exception ex)
        {
          Container.GetInstance<ILogger>().LogError(ex);
        }
      }
      return new Options();
    }

    
  }
}