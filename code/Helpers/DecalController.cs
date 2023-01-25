using Godot;
using System;
using static PhysicsExtensions;

public partial class DecalController : Decal
{
    [Export(PropertyHint.Layers3dPhysics)] public uint MovementColliderLayers;

    
    public override void _Ready()
	{
        CallDeferred("CheckPos");
    }

    private void CheckPos()
    {
        if (!RayCast(out _, GetWorld3d().DirectSpaceState, GlobalPosition + new Vector3(0, 10, 0), GlobalPosition - new Vector3(0, 10, 0), false, true, MovementColliderLayers))
        {
            Visible = false;
        }
    }
}
