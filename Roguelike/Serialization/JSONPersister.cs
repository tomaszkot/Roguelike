
using Dungeons.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Roguelike.Serialization
{
  public interface IPersistable
  {
    bool Dirty { get; set; }
  }

  public class JSONPersister : IPersister
  {
    protected const string extension = ".json";
    public enum FileKind { Hero, GameLevel, GameState }
    Container container;

    public JSONPersister(Container container)
    {
      this.container = container;
      container.GetInstance<ILogger>().LogInfo("JSONPersister ctor [container]: " + this.container.GetHashCode());
    }

    public void Save<T>(T entity, string filePath)
    {
      try
      {
        //Debug.Log("Engine_JsonSerializer...");
        var path = Path.GetDirectoryName(filePath);

        Directory.CreateDirectory(path);

        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        //settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var json = JsonConvert.SerializeObject(entity, settings);
        File.WriteAllText(filePath, json);

      }
      catch (Exception ex)
      {
        this.container.GetInstance<ILogger>().LogError(ex);
        throw;
      }
    }
        
    public T Load<T>(string filePath, Container container) where T: class, IPersistable
    {
      T entity = null;

      try
      {
        var json = File.ReadAllText(filePath);
        this.container.GetInstance<ILogger>().LogInfo("Engine_JsonDeserializer...");
        
        if (!IsValidJson(json))
        {
          this.container.GetInstance<ILogger>().LogError("param json is not a valid json!");
          return null;
        }
        ITraceWriter traceWriter = null;// new MemoryTraceWriter();
        JsonSerializerSettings settings = new JsonSerializerSettings 
        { TraceWriter = traceWriter, TypeNameHandling = TypeNameHandling.All };
        //container.Options.DefaultScopedLifestyle = new ExecutionContextScopeLifestyle();
        //settings.TypeNameAssemblyFormatHandling = TypeNameAssemblyFormatHandling.Full;
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        settings.ContractResolver = new SimpleInjectorContractResolver(container);
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
        this.container.GetInstance<ILogger>().LogError(ex);
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
      get{ return RootPath /*+ GameFolder*/; }
    }

    public string GetFullFilePath(FileKind fileKind, string heroName, string fileSuffix = "")//, string fileName)
    {
      List<string> parts = new List<string>();
      var path = GetFilesPath(heroName);
      parts.Add(path);
      //if (supportManyLevels && fileKind == FileKind.GameLevel)
      if(GameLevelsFolder.Any() && fileKind == FileKind.GameLevel)
        parts.Add(GameLevelsFolder);
     
      var fileName = fileKind.ToString() + fileSuffix + extension;
      parts.Add(fileName);

      return Path.Combine(parts.ToArray());
    }

    protected virtual string GetFilesPath(string heroName)
    {
      return Path.Combine(new[] { GamePath, heroName });
    }

    protected virtual string GameLevelsFolder { get { return ""; } }

    protected static string GameName 
    { 
      get { return "Roguelike"; } 
    }

    [JsonIgnore]
    public Container Container { get => container; private set => container = value; }

    //protected virtual string GameFolder { get { return GameName; } }

    public void SaveHero(Hero hero)
    {
      var fileName = GetFullFilePath(FileKind.Hero, hero.Name);
      Save<Hero>(hero, fileName);
    }

    public Hero LoadHero(string heroName)
    {
      var fileName = GetFullFilePath(FileKind.Hero, heroName);
      return Load<Hero>(fileName, container);
    }
    
    public void SaveLevel(string heroName, GameLevel level)
    {
      var filePath = GetFullFilePath(FileKind.GameLevel, heroName, level.Index.ToString());// GetLevelFileName(level.Index));
      Save(level, filePath);
    }

    //private string GetLevelFileName(int levelIndex)
    //{
    //  return "DungeonLevel" + levelIndex + extension;
    //}

    public GameLevel LoadLevel(string heroName, int index)
    {
      var filePath = GetFullFilePath( FileKind.GameLevel, heroName, index.ToString());
      return Load<GameLevel>(filePath, container);
    }

    public void SaveGameState(string heroName, GameState gameState)
    {
      var filePath = GetFullFilePath(FileKind.GameState, heroName);
      Save<GameState>(gameState, filePath);
    }

    public GameState LoadGameState(string heroName)
    {
      var filePath = GetFullFilePath(FileKind.GameState, heroName);
      return Load<GameState>(filePath, container);
    }
  }
}