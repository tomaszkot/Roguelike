using Dungeons.ASCIIDisplay.Presenters;
using Roguelike.Events;
using System;
using System.Collections.Generic;

namespace Roguelike.InfoScreens
{
  public class LastActions
  {
    private List<GameEvent> actions = new List<GameEvent>();
    const int MaxSize = 6;
    bool reverse = false;
    int actionsCount = 0;

    public LastActions()
    {
      for (int i = 0; i < MaxSize; i++)
      {
        Add("");
      }
    }

    public List<GameEvent> Actions
    {
      get
      {
        return actions;
      }
    }

    internal void Add(GameEvent ac)
    {
      if (ac.Info.Trim() == string.Empty)
        return;

      if (ac is LivingEntityAction leac)
      {
        if (leac.Kind == LivingEntityActionKind.Moved)
          return;
        if (leac.Kind == LivingEntityActionKind.StateChanged)
          return;
      }

      if (ac is InventoryAction ia && ia.Kind == InventoryActionKind.DragDropDone)
      {
        return;
      }


      if (reverse)
        actions.Insert(0, ac);
      else
        actions.Add(ac);
      ac.Index = ++actionsCount;
      //ac.Info = actionsCount.ToString() + ") " + ac.Info;
      if (actions.Count > MaxSize)
      {
        if (reverse)
          actions.RemoveAt(actions.Count - 1);
        else
          actions.RemoveAt(0);
      }
    }

    internal void Add(string action, ActionLevel level = ActionLevel.Normal)
    {
      var ac = new GameEvent() { Info = action, Level = level };
      Add(ac);
    }

    public List<ListItem> ToASCIIList()
    {
      var lines = new List<ListItem>();
      foreach (var ac in Actions)
      {
        lines.Add(new ListItem()
        {
          Color = ac.Level == ActionLevel.Normal ? ConsoleColor.White : ConsoleColor.DarkMagenta,
          Text = ac.Info
        });
      }

      return lines;
    }
  }



}
