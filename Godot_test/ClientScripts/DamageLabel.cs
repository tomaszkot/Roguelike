using Godot;
using System;

public partial class DamageLabel : Label
{
	public override async void _Ready()
	{
		var t = GetTree().CreateTween();
		t.TweenProperty(this,"position",new Vector2(Position.X,Position.Y - 40),0.5);
		await ToSignal(GetTree().CreateTimer(0.5),"timeout");
		DestoryAfterAnimation();

	//t.TweenCallback(Callable.From(DestoryAfaterAnimation));
	}

	private void DestoryAfterAnimation()
	{
		if (GetParent() != null)
			CallDeferred("queue_free");
  }


}
