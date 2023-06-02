using Godot;
using System;

	namespace Entities
	{
		public partial class DamageLabel : Entity
		{
			public async void StartAnimation()
			{
				var label = (Label)GetChild(0);
				var t = GetTree().CreateTween();
				t.TweenProperty(label, "position", new Vector2(label.Position.X, label.Position.Y - 40), 0.5);
				await ToSignal(GetTree().CreateTimer(0.5), "timeout");
				DestoryAfterAnimation();

				//t.TweenCallback(Callable.From(DestoryAfaterAnimation));
			}

			private void DestoryAfterAnimation()
			{
				if (GetParent() != null)
					CallDeferred("queue_free");
			}

		}
	}
