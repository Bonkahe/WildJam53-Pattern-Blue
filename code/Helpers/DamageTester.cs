using Godot;
using System;

public partial class DamageTester : Node
{
	public int iterations = 15;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        GetTree().CallGroup("Player", "OnDamage", new Vector3(), 10, 10);
		iterations--;
		if (iterations == 0)
		{
			QueueFree();
		}
    }
}
