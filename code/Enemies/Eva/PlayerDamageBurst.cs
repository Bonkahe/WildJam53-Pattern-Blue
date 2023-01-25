using Godot;
using System;
using static PhysicsExtensions;

public partial class PlayerDamageBurst : Node3D
{
	[Export] public float EffectDuration;

	[Export] public float DamageRadius;
	[Export] public float DamageAmount;

	private bool HasEnded = false;
	private bool HasFired = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		EffectDuration -= (float)delta;
		if (EffectDuration <= 0)
		{
			if (!HasEnded)
			{
				HasEnded = true;
				GetParent().QueueFree();
            }
		}

		if (!HasFired)
		{
            GetTree().CallGroup("Player", "OnDamage", GlobalPosition, DamageRadius, DamageAmount);
            GetTree().CallGroup("Enemies", "OnDamage", GlobalPosition, DamageRadius);
			HasFired = true;
        }
    }
}
