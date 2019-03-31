using Dungeons;
using Dungeons.Core;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Generators.TileContainers;
using Roguelike.Serialization;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.TileContainers
{
  

  //mid-size node like 100x100, part of the DungeonPit
  public class DungeonLevel : GameNode, IPersistable
  {
    public int Index { get; set; }
    public Stairs StairsUp { get => stairsUp; set => stairsUp = value; }
    public Stairs StairsDown { get => stairsDown; set => stairsDown = value; }

    Stairs stairsUp = null;
    Stairs stairsDown = null;
    public event EventHandler<GenericEventArgs<NodeRevealedParam>> NodeRevealed;

    public DungeonLevel(Container container) : base(container != null ? container : new ContainerConfigurator().Container)
    {
      Dirty = true;//TODO
    }

    public override string ToString()
    {
      return Description;
    }

    public override string Description
    {
      get { return "DungeonLevel Index: " + Index; }
    }

    public string PitName { get; set; }

    [JsonIgnore]
    public bool Dirty { get; set; }

    internal void OnGenerationDone()
    {
      HookEvents();

      Nodes[0].Reveal(true, true);
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
      foreach (var node in Nodes)
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
        if (dt.Symbol != tile.Symbol)
        {
          Logger.LogError("dt.Symbol != tile.Symbol ["+ dt.Symbol + ","+ tile.Symbol + "] " + tile.Point + " ");
        }
        if (!dt.Revealed && tile.Revealed)
          dt.Revealed = true;
      }
      
      if (NodeRevealed != null)
        NodeRevealed(sender, e);
      
    }
  }


}
