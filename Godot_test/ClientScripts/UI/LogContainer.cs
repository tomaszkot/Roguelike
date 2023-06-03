using Godot;

namespace UI
{
	public partial class LogContainer : VBoxContainer
	{
		private int logIndex = -1;
		public void showNewLog()
		{
			var numberOfActions = Game.GameManager.EventsManager.LastActions.Actions.Count;
			if (numberOfActions > 0)
			{
				var action = Game.GameManager.EventsManager.LastActions.Actions[numberOfActions - 1].ToString();
				if (GetChildCount() > 0)
				{
					var label = (Label)GetChild(GetChildCount() - 1);
					var previousAction = label.Text;
					if ((action == previousAction && logIndex == numberOfActions))
						return;
				}
				if (action.Contains("GameEvent")) //doesn't show GameEvent messages 
					return;
				var logScene = (PackedScene)ResourceLoader.Load("uid://bgjldhojgf8no");
				var log = (Label)logScene.Instantiate();
				log.Text = action;
				logIndex = numberOfActions;
				AddChild(log);
			}

			if (GetChildCount() > 0)
			{
				float transparency = 1;
				var childs = GetChildren();
				childs.Reverse();
				foreach (Label l in childs)
				{
					l.SelfModulate = new Color(1, 1, 1, transparency);
					transparency -= 0.2f;
					if (l.SelfModulate.A <= 0)
						l.QueueFree();
				}
			}
		}
	}
}