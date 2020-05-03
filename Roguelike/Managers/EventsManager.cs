using Dungeons.Core;
using Roguelike.Events;
using Roguelike.InfoScreens;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roguelike.Managers
{
  public class EventsManager
  {
    LastActions lastActions = new LastActions();

    public LastActions LastActions { get => lastActions; set => lastActions = value; }

    public event EventHandler<GameAction> ActionAppended;

    public EventsManager()
    {

    }

    public void Assert(bool check, string desc)
    {
      if (!check)
      {
        Debug.Assert(false);
        AppendAction(new GameStateAction() { Type = GameStateAction.ActionType.Assert, Info = desc });
      }
    }

    public void AppendAction(GameAction ac)
    {
      LastActions.Add(ac);
      // //Debug.WriteLine(ac);//slow
      if (ActionAppended != null)//send it to listeners as logic of game depends on it
      {
        ActionAppended(this, ac);
      }
    }
  }
}
