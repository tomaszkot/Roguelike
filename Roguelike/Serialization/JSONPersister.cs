
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Roguelike;
using Roguelike.TileContainers;
using Roguelike.Tiles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Serialization
{

  class JSONPersister : IPersister
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

    public void SaveWorld(World world)
    {
      Save<World>(world, "OuadII_World.json");
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

    public World LoadWorld()
    {
      return Load<World>("OuadII_World.json");
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

    public void SaveHero(Hero hero)
    {
      Save<Hero>(hero, "OuadII_Hero.json");
    }

    public Hero LoadHero()
    {
      return Load<Hero>("OuadII_Hero.json");
    }

    const string PitPreffix = "";
    const string PitDir = "./Pits/";

    public JSONPersister()
    {
    }

    string GetStoragePitName(string pitName)
    {
      return PitPreffix + pitName;
    }

    public void SavePits(List<DungeonPit> pits)
    {
      pits.ForEach(i =>
      {
        Directory.CreateDirectory(PitDir);
        Save<DungeonPit>(i, PitDir+GetStoragePitName(i.Name)+".json");
      });
    }

    public List<DungeonPit> LoadPits()
    {
      var pits = new List<DungeonPit>();
      if (Directory.Exists(PitDir))
      {
        var files = Directory.GetFiles(PitDir, PitPreffix + "*");
        foreach (var pitFileName in files)
          pits.Add(Load<DungeonPit>(pitFileName));
      }
      return pits;
    }

    public void SaveGameState(GameState gameState)
    {
      Save<GameState>(gameState, "GameState.json");
    }

    public GameState LoadGameState()
    {
      return Load<GameState>("GameState.json");
    }
  }
}