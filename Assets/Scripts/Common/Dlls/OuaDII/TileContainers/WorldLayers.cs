//using Dungeons;
//using Dungeons.Core;
//using Dungeons.Tiles;
//using OuaDII.TileContainers;
//using Roguelike.TileContainers;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Drawing;

//namespace OuaDII.TileContainers
//{
//  public enum Layer
//  {
//    Main
//  }


//  public class WorldLayers
//  {
//    Dictionary<string, World> worlds;
//    int sizeX;
//    int sizeY;

//    public WorldLayers(World defaultNode) : this()
//    {
//      //var name = defaultNode.Name;
//      //if (string.IsNullOrEmpty(name))
//      //  name = Guid.NewGuid().ToString();
//      worlds.Add(defaultNode.Name, defaultNode);
//    }

//    public WorldLayers()
//    {
//      worlds = new Dictionary<string, World>();
//    }

//    public WorldLayers(int sizeX, int sizeY) : this()
//    {
//      this.sizeX = sizeX;
//      this.sizeY = sizeY;
//    }

//    public bool HasLayer(string layerName)
//    {
//      return worlds.ContainsKey(layerName);
//    }

//    public void AddLayer(World layer)
//    {
//      worlds[layer.Name] = layer;
//      if (sizeX == 0)
//        sizeX = layer.Width;
//      if (sizeY == 0)
//        sizeY = layer.Height;
//    }

//    public World AddLayer(string layerName)
//    {
//      worlds[layerName] = new World(null);//TODO
//      worlds[layerName].Create(sizeX, sizeY);
//      return worlds[layerName];
//    }

//    public World this[string layerName]
//    {
//      get { return worlds[layerName]; }
//    }

//    public Tile GetTile(Point pt, string layer)
//    {
//      var tiles = this[layer.ToString()];
//      return tiles.GetTile(pt);
//    }

//    public bool SetTile(Point pt, Tile tile, string layer)
//    {
//      var tiles = this[layer.ToString()];
//      return tiles.SetTile(tile, pt);
//    }

//  }
//}
