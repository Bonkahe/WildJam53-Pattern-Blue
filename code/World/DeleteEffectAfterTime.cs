using Godot;
using System;

public partial class DeleteEffectAfterTime : Node3D
{
	[Export] public float Duration;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Duration <= 0)
		{
			QueueFree();
		}
		else
		{
            Duration -= (float)delta;
        }
    }
}
