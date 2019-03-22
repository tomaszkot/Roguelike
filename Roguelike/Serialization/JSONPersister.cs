
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Diagnostics;
using System.IO;

namespace Roguelike.Serialization
{
  public class JSONPersister : IPersister
  {
    public void Save<T>(T entity, string fileName)
    {
      try
      {
        //Debug.Log("Engine_JsonSerializer...");
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        var json = JsonConvert.SerializeObject(entity, settings);
        File.WriteAllText(fileName, json);
        //return json;

      }
      catch (Exception ex)
      {
        Debug.WriteLine(ex);
        throw;
      }
    }

    public T Load<T>(string fileName) where T: class
    {
      T entity = null;

      try
      {
        var json = File.ReadAllText(fileName);
        Debug.WriteLine("Engine_JsonDeserializer...");
        if (!IsValidJson(json))
        {
          Debug.WriteLine("param not a valid json!");
          return null;
        }
        JsonSerializerSettings settings = new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };
        settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
        entity = JsonConvert.DeserializeObject<T>(json, settings);
        //if (siFromJson.Hero.CurrentEquipment.ContainsKey(EquipmentKind.Weapon))
        //  Debug.Log("Engine_JsonDeserializer weapon = " + siFromJson.Hero.CurrentEquipment[EquipmentKind.Weapon]);
        //else
        //  Debug.Log("Engine_JsonDeserializer no weapon ");
      }
      catch (Exception)
      {
        throw;
        //Debug.LogError(ex);
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
      {
        return false;
      }
    }

    string GetFullFilePath(string fileName)
    {
      return "c:/tmp/"+ fileName;
    }

    public void SaveHero(Hero hero)
    {
      Save<Hero>(hero, GetFullFilePath("OuadII_Hero.json"));
    }

    public Hero LoadHero()
    {
      return Load<Hero>(GetFullFilePath("OuadII_Hero.json"));
    }
    
    public void SaveLevel(DungeonLevel level)
    {
      Save(level, GetFullFilePath(GetLevelFileName(level.Index)));
    }

    private string GetLevelFileName(int levelIndex)
    {
      return "DungeonLevel" + levelIndex + ".json";
    }

    public DungeonLevel LoadLevel(int index)
    {
      return Load<DungeonLevel>(GetFullFilePath(GetLevelFileName(index)));
    }

    public void SaveGameState(GameState gameState)
    {
      Save<GameState>(gameState, GetFullFilePath("GameState.json"));
    }

    public GameState LoadGameState()
    {
      return Load<GameState>(GetFullFilePath("GameState.json"));
    }
  }
}