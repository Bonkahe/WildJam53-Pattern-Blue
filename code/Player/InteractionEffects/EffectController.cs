using Godot;
using Godot.Collections;
using System;

public partial class EffectController : Node3D
{
	public enum EffectType {Ocean, Snow, FreshWater, Mud};
	[Export] public EffectType effectType;

	[Export] public NodePath EffectContainerPath;

	[Export] public float PauseDeletionTime = 3;

	[Export] public Vector3 TileSize = new Vector3(10, 10, 10);

    [Export(PropertyHint.Layers3dPhysics)] public uint EffectCollisionLayers;

    private Node EffectContainer;

	private Array<Dictionary> QueryResults;

	private bool FirstIteration = true;
	private float pauseDeletionTime;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		EffectContainer = GetNode<Node>(EffectContainerPath);
        pauseDeletionTime = PauseDeletionTime;
    }

	public override void _PhysicsProcess(double delta)
	{
		if (FirstIteration)
		{
			FirstIteration = false;
            CheckArea();
        }
	}

	public override void _Process(double delta)
	{
        if (pauseDeletionTime > 0)
        {
            pauseDeletionTime -= (float)delta;

        }
    }

	public void CheckArea()
	{
        QueryResults = PhysicsExtensions.OverlapBox3D(GetWorld3d().DirectSpaceState, TileSize, GlobalPosition, Transform.basis, true, false, EffectCollisionLayers);

        foreach (var item in QueryResults)
        {
            EffectController itemAsEffect = (EffectController)item["collider"];

			if (itemAsEffect != null && itemAsEffect != this)
			{
                itemAsEffect.Destroy();
			}
		}
    }

	public void Destroy()
	{
		if (pauseDeletionTime <= 0)
		{
			EffectContainer.QueueFree();
		}
    }

	public EffectType GetEffectType()
	{
		return effectType;
	}
}
