using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Serialization;
using Roguelike.Tiles;
using Roguelike.Tiles.Interactive;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Roguelike.TileContainers
{
  //mid-size node like 100x100, part of the DungeonPit
  public class GameLevel : AbstractGameLevel, IPersistable
  {
    
    public Stairs StairsUp { get => stairsUp; set => stairsUp = value; }
    public Stairs StairsDown { get => stairsDown; set => stairsDown = value; }

    Stairs stairsUp = null;
    Stairs stairsDown = null;
    public event EventHandler<GenericEventArgs<NodeRevealedParam>> NodeRevealed;

    public GameLevel() : base(new ContainerConfigurator().Container)
    {

    }

    public GameLevel(Container container) : base(container != null ? container : new ContainerConfigurator().Container)
    {
      Dirty = true;//TODO
    }

    internal T SetTileAtRandomPosition<T>(int levelIndex, bool matchNodeIndex = true) where T : Tile, new()
    {
      var tile = new T();
      var inter = tile as Roguelike.Tiles.InteractiveTile;
      if(inter!=null)
        inter.Level = levelIndex;
      return SetTileAtRandomPosition(tile, matchNodeIndex) as T;
    }

    public override string ToString()
    {
      return Description;
    }

    public override string Description
    {
      get
      {
        var desc = GetType() + " Index: " + Index;
        if (PitName.Any())
          desc += " [" + PitName + "] ";
        return desc;
      }
    }

    public string pitName = "";
    //public QuestKind QuestKind { get; private set; }

    public string PitName
    {
      get { return pitName; }
      set
      {
        pitName = value;
      }
    }

    [JsonIgnore]
    public bool Dirty { get; set; }

    public override void OnGenerationDone()
    {
      if (!GeneratorNodes.Any())
        return;
      HookEvents();
      GeneratorNodes[0].Reveal(true, true);
    }

    public void OnLoadDone()
    {
      HookEvents();
    }

    bool eventsHooked = false;
    private void HookEvents()
    {
      if (eventsHooked)
        throw new Exception("eventsHooked already hooked!");
      foreach (var node in GeneratorNodes)
      {
        node.OnRevealed += Node_OnRevealed;
        foreach (var isl in node.ChildIslands)
          isl.OnRevealed += Node_OnRevealed;
      }

      eventsHooked = true;
    }

    private void Node_OnRevealed(object sender, GenericEventArgs<NodeRevealedParam> e)
    {
      var nodeTiles = e.EventData.Tiles;

      //when data is loaded tiles must be revelaed by maching points;
      foreach (var tile in nodeTiles)
      {
        var dt = this.GetTile(tile.Point);
        try
        {
          if (dt == null)
            Logger.LogError("dt == null!!! tile.Symbol = [" + tile.Symbol + "] " + tile.Point + " ");
          else
          {
            if (dt.Symbol != tile.Symbol)
            {
              if (!(dt.Symbol == Constants.SymbolWall && tile.Symbol == Constants.SymbolDoor))//TODO
              {
                Logger.LogError("dt.Symbol != tile.Symbol [" + dt.Symbol + "," + tile.Symbol + "] " + tile.Point + " ");
              }
            }
            if (!dt.Revealed && tile.Revealed)
              dt.Revealed = true;
          }
        }
        catch (Exception ex)
        {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
      
      if (NodeRevealed != null)
        NodeRevealed(sender, e);
      
    }

    [JsonIgnore]
    public override List<DungeonNode> ChildIslands
    {
      get
      {
        if (!Nodes.Any())
          return null;
        return Nodes.Single().ChildIslands;
      }
    }
  }


}
