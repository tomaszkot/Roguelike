using Dungeons;
using Dungeons.Core;
using Dungeons.TileContainers;
using Dungeons.Tiles;
using Newtonsoft.Json;
using Roguelike.Abstract.Tiles;
using Roguelike.Core.Managers;
using Roguelike.Managers;
using Roguelike.Serialization;
using Roguelike.Tiles.Interactive;
using Roguelike.Tiles.LivingEntities;
using Roguelike.Tiles.Looting;
using SimpleInjector;
using System;
using System.Collections.Generic;
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
    public event EventHandler<NodeRevealedParam> NodeRevealed;
    public LeverSet BossRoomLeverSet 
    { 
      get => bossRoomLeverSet;
      set
      {
        bossRoomLeverSet = value;
        if(bossRoomLeverSet!=null)
          bossRoomLeverSet.StateChanged += BossRoomLeverSet_StateChanged;
      }
    }

    private void BossRoomLeverSet_StateChanged(object sender, bool e)
    {
      var message = "Levers sequence properly set!";
      if (!e)
        message = "Levers sequence not set";
      this.Container.GetInstance<EventsManager>().AppendAction(new Events.GameEvent()
      {
        Info = message,
        Level = Events.ActionLevel.Important
      }); ;
    }

    public GameLevel(Container container) : base(container)
    {
    }

    internal T SetTileAtRandomPosition<T>(int levelIndex, bool matchNodeIndex = true) where T : Tile, new()
    {
      var tile = new T();
      var inter = tile as Roguelike.Tiles.Interactive.InteractiveTile;
      if (inter != null)
        inter.Level = levelIndex;
      return SetTileAtRandomPosition(tile, matchNodeIndex) as T;
    }

    public override bool BossLeversSolved()
    {
      return BossRoomLeverSet.IsOpened();
    }

    public override void OnHeroPlaced(Hero hero)
    {
      try
      {
        if (hero.DungeonNodeIndex < Nodes.Count)
        {
          if (Nodes[hero.DungeonNodeIndex].Container == null)
            Nodes[hero.DungeonNodeIndex].Container = this.Container;//TODO WTF
          if (hero.IsFromChildIsland())
          {
            var child = Nodes.SelectMany(i => i.ChildIslands).Where(c => c.NodeIndex == hero.DungeonNodeIndex).FirstOrDefault();
            child.Reveal(true);
            //var childs = Nodes.Where(i => i.ChildIslands.Any(k => k.NodeIndex == hero.DungeonNodeIndex)).Select(i=>i.;
          }
          else
            Nodes[hero.DungeonNodeIndex].Reveal(true);
        }

      }
      catch (Exception ex)
      {
        Logger.LogError(ex);
      }
    }


    public override string ToString()
    {
      return Description;
    }

    public override string Description
    {
      get
      {
        var desc = GetType() + " Index: " + Index + " W:" + Width + ", H:" + Height;
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
      HookEvents(HookEventContext.Generaton);
      //GeneratorNodes[0].Reveal(true, true);//bad to have it here, when loaded level hero migth not be at 0 room
    }

    public override void OnLoadDone()
    {
      HookEvents(HookEventContext.Load);
    }

    enum HookEventContext { Unset, Load, Generaton }
    HookEventContext hookEventContext = HookEventContext.Unset;
    private LeverSet bossRoomLeverSet;

    private void HookEvents(HookEventContext hookEventContext)
    {
      if (this.hookEventContext != HookEventContext.Unset && this.hookEventContext != hookEventContext)
        throw new Exception("eventsHooked already hooked! " + this.hookEventContext + ", tried: " + hookEventContext);

      if (this.hookEventContext != HookEventContext.Unset)
        return;

      foreach (var node in GeneratorNodes)
      {
        node.OnRevealed += Node_OnRevealed;
        foreach (var isl in node.ChildIslands)
          isl.OnRevealed += Node_OnRevealed;
      }

      this.hookEventContext = hookEventContext;
    }

    private void Node_OnRevealed(object sender, NodeRevealedParam eventData)
    {
      var revealedTiles = eventData.Tiles;
      var dn = sender as DungeonNode;
      var dnt = dn.GetTiles();
      var nr = dnt.Where(i => !i.Revealed).ToList();

      var missing = dnt.Where(i => !revealedTiles.Contains(i)
      && (i.DungeonNodeIndex == dn.NodeIndex || !i.IsFromChildIsland()))
        .ToList();
      //when data is loaded tiles must be revelaed by maching points;
      foreach (var revealedTile in revealedTiles)
      {
        var dt = this.GetTile(revealedTile.point);
        try
        {
          if (dt == null)
            Logger.LogError("dt == null!!! tile.Symbol = [" + revealedTile.Symbol + "] " + revealedTile.point);
          else
          {
            if (dt.Symbol != revealedTile.Symbol)
            {
              if (!(dt is Wall && revealedTile is IDoor)
                   && !(dt is IDoor && revealedTile is Wall)
                )//TODO
              {
                //OnNodeRevealedTileSymbolMismatch(revealedTile, dt);TODO
              }
            }
            if (!dt.Revealed && revealedTile.Revealed)
            {
              if (dt.DungeonNodeIndex == revealedTile.DungeonNodeIndex)
                dt.Revealed = true;
            }

            var door = GetNeighborTiles(dt).Where(i => i is Tiles.Interactive.Door && !i.Revealed).FirstOrDefault();
            if (door != null)
            {
              door.Revealed = true;
              var neibs = GetNeighborTiles(door).Where(i => i != null && !i.Revealed && missing.Any(j => j.point == i.point)).ToList();
              neibs.ForEach(i => i.Revealed = true);
            }
          }
        }
        catch (Exception ex)
        {
          Logger.LogError(ex.Message);
          throw;
        }
      }


      missing.ForEach(i => i.Revealed = true);
      missing.ForEach(i => revealedTiles.Add(i));

      EnsureRevealed(eventData.NodeIndex);

      if (NodeRevealed != null)
        NodeRevealed(sender, eventData);

    }

    public virtual void OnNodeRevealedTileSymbolMismatch(Tile revealed, Tile dt)
    {
      if (dt is Hero && revealed.IsEmpty)
        return;//TODO
      if (dt is Merchant && revealed.IsEmpty)
        return;//TODO

      Logger.LogError(this + " dt.Symbol != revealed.Symbol [" + dt.Symbol + "," + revealed.Symbol + "] " + revealed.point + " dt:" + dt + " revealed:" + revealed);
      if (dt.IsEmpty && revealed is IAlly)
      {
        this.SetTile(revealed, revealed.point);//TODO!
      }

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

    public DateTime GeneratedAt { get; set; }
  }


}
