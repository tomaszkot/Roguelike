﻿using Newtonsoft.Json;
using Roguelike.Managers;
using Roguelike.Tiles;
using SimpleInjector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Abstract
{
  public interface IGame
  {
    GameManager GameManager { get; set; }
    Container Container { get; set; }
    Hero Hero { get; }
    Dungeons.TileContainers.DungeonNode GenerateDungeon();
    void MakeGameTick();
  }

  public abstract class Game : IGame
  {
    public Game(Container container)
    {
      this.Container = container;
      GameManager = container.GetInstance<GameManager>();
    }

    public void MakeGameTick()
    {
      GameManager.MakeGameTick();
    }

    public GameManager GameManager
    {
      get;
      set;
    }

    [JsonIgnore]
    public Container Container { get; set; }
    public Hero Hero { get { return GameManager.Hero; } }
    public abstract Dungeons.TileContainers.DungeonNode GenerateDungeon();
    public static string Version { get; } = "0.0.6";
  }
}
