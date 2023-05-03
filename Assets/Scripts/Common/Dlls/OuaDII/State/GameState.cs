using Dungeons.Core;
using OuaDII.Generators;
using OuaDII.Quests;
using OuaDII.State;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OuaDII.State
{
  public class PlaceLayout
  {
    public string PlaceName { get; set; }
    public Vector2D PlacePosition { get; set; }

    public override string ToString()
    {
      return base.ToString() + " "+ PlaceName;
    }
  }

  public class PlacesLayout
  {
    public List<PlaceLayout> Info = new List<PlaceLayout>();
  }

  public class DiscoveredClouds
  {
    /// <summary>
    /// string is: parentName_childName
    /// </summary>
    public List<string> Points = new List<string>();
  }

  public class GameState : Roguelike.State.GameState
  {
    PlacesLayout placesLayout = new PlacesLayout();
    ChanceAtGameStart chanceAtGameStart = new ChanceAtGameStart();
    DiscoveredClouds discoveredClouds = new DiscoveredClouds();
    public Roguelike.Tiles.LivingEntities.AllyBehaviour AllyBehaviour{ get; set; }
    
    public ChanceAtGameStart ChanceAtGameStart { get => chanceAtGameStart; set => chanceAtGameStart = value; }
    public DiscoveredClouds DiscoveredClouds { get => discoveredClouds; set => discoveredClouds = value; }
    public PlacesLayout PlacesLayout 
    { 
      get => placesLayout; 
      set => placesLayout = value; 
    }

    public GameState()
    {
    }

    public void AddDiscoveredCloud(string cloudId)
    {
      if(!DiscoveredClouds.Points.Contains(cloudId))
        DiscoveredClouds.Points.Add(cloudId);
    }

    public override Roguelike.State.HeroPath CreateHeroPath()
    {
      return new HeroPath();
    }
  }
}
