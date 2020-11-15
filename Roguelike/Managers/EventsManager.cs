using Roguelike.Events;
using Roguelike.InfoScreens;
using System;
using System.Diagnostics;

namespace Roguelike.Managers
{
  public class EventsManager
  {
    LastActions lastActions = new LastActions();

    public LastActions LastActions { get => lastActions; set => lastActions = value; }
    public GameManager GameManager { get => gameManager; set => gameManager = value; }

    public event EventHandler<GameAction> ActionAppended;
    GameManager gameManager;

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
      if (!GameManager.Hero.Alive && GameManager.Context.HeroDeadReported)
        return;
      LastActions.Add(ac);
      if (ActionAppended != null)//send it to listeners as logic of game depends on it
      {
        ActionAppended(this, ac);
      }
    }
  }
}
