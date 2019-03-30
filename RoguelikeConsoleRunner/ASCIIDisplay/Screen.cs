using Dungeons;
using Dungeons.ASCIIDisplay;
using Dungeons.ASCIIDisplay.Presenters;
using Roguelike.Abstract;
using Roguelike.Managers;
using Roguelike.TileContainers;
using System;
using System.Diagnostics;

namespace RoguelikeConsoleRunner.ASCIIDisplay
{
  public class Screen : Dungeons.ASCIIDisplay.Screen
  {
    int panelsWidth = 40;
    ListPresenter lastActionsPrinter;
    ListPresenter inventoryPresenter;
    IGameManagerProvider gameManagerProvider;

    public Screen(IDrawingEngine drawingEngine, IGameManagerProvider gameManagerProvider) : base(drawingEngine)
    {
      this.gameManagerProvider = gameManagerProvider;
    }

    private int DungeonBottom
    {
      get { return OriginY + DungeonY + Dungeon.Height; }
    }

    GameManager GameManager
    {
      get { return gameManagerProvider.GameManager; }
    }

    public override DungeonNode Dungeon { get { return GameManager.Context.CurrentNode; } }


    protected override void UpdateItem(Item i)
    {
      Debug.Assert(GameManager.Context.CurrentNode == Dungeon);
      if (i == DungeonDesc)
      {
        var desc = "";
        //TODO
        //var world = GameManager.GetCurrentNode<World>();
        //if (world != null)
        //  desc += world.ToString();
        if (GameManager.Context.CurrentNode is Roguelike.TileContainers.DungeonLevel)
          desc += " "+GameManager.Context.CurrentNode.ToString();
        DungeonDesc.Text = desc;// GameManager.GetCurrentDungeonDesc();
      }
    }

    protected override void CreateLists()
    {
      base.CreateLists();
      Lists[UsageListName].Items.Add(new ListItem("S - Save"));
      Lists[UsageListName].Items.Add(new ListItem("L - Load"));

      var panelLeft = Console.WindowWidth - panelsWidth*2 - OriginX;

      lastActionsPrinter = new ListPresenter("Last Actions", panelLeft, DungeonBottom, panelsWidth);
      Lists.Add(lastActionsPrinter.Caption, lastActionsPrinter);

      inventoryPresenter = new ListPresenter("Inventory", panelLeft, OriginY, panelsWidth);
      Lists.Add(inventoryPresenter.Caption, inventoryPresenter);
    }

    public override void RedrawLists()
    {
      base.RedrawLists();
    }

    public override void UpdateList(ListPresenter list)
    {
      //if (gameManager == null)
      //  return;
      if (list == inventoryPresenter)
        inventoryPresenter.Items = GameManager.Hero.Inventory.ToASCIIList();
      else if (list == lastActionsPrinter)
        lastActionsPrinter.Items = GameManager.EventsManager.LastActions.ToASCIIList();
    }
  }
}

