using Godot;

namespace UI
{
	public partial class GUI : CanvasLayer
	{
		public StatsPanel statsPanel;
		public UIButtons uibuttons;

		public void ShowOptionMenu()
		{
			var options = (Control)GetNode("Options");
			if (options.Visible)
			{
				GetTree().Paused = false;
				options.Visible = false;
			}
			else
			{
				options.Visible = true;
				GetTree().Paused = true;
			}
		}

		public void showGUI()
		{
			var heroStats = (Control)GetNode("StatsPanel");
			heroStats.Visible = true;
			statsPanel = (StatsPanel)GetNode("StatsPanel");
			statsPanel.Visible = true;
			statsPanel.UpdateStats();
			uibuttons = (UIButtons)GetNode("UIButtons");
			uibuttons.Visible = true;
		}

		public void ShowDeathScreen()
		{
			var deathScreen = (Control)GetNode("DeathScreen");
			deathScreen.Visible = true;
		}

		private async void _on_quit_pressed()
		{
			Game.tryAgain = false;
			var game = (Game)GetNode("/root/Game");
			var button = (Button)GetNode("Options/Quit");
			button.Disabled = true;
			var anim = (AnimationPlayer)GetNode("Options/Quit/AnimationPlayer");
			anim.Play("clicked");
			game.SaveGame();
			GetTree().Paused = false;
			await ToSignal(GetTree().CreateTimer(2), "timeout");
			GetTree().ReloadCurrentScene();
		}

		public override void _Input(InputEvent @event)
		{
			base._Input(@event);
			if (@event is InputEventKey k)
			{
				if (k.IsPressed() && k.Keycode == Key.Escape && !k.IsEcho() && Game.isGameStarted)
				{
					ShowOptionMenu();
				}
			}
		}

		private async void _on_try_again_pressed()
		{
			var anim = (AnimationPlayer)GetNode("DeathScreen/TryAgain/AnimationPlayer");
			anim.Play("clicked");
			await ToSignal(GetTree().CreateTimer(0.4), "timeout");
			Game.tryAgain = true;
			GetTree().ReloadCurrentScene();
		}
		private async void _on_main_menu_pressed()
		{
			Game.tryAgain = false;
			var anim = (AnimationPlayer)GetNode("DeathScreen/MainMenu/AnimationPlayer");
			anim.Play("clicked");
			await ToSignal(GetTree().CreateTimer(0.4), "timeout");
			GetTree().ReloadCurrentScene();
		}

	}
}
