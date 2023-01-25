using Godot;
using System;
using static PhysicsExtensions;

public partial class LaserController : Node3D
{
    [Export(PropertyHint.Layers3dPhysics)] public uint LaserHitCollisionMask;

    [Export] public PackedScene ImpactEffect;

    [Export] public float SwipeRandomizationDistance;
    [Export] public float SwipeTime;
    [Export] public float SwipeDistance;
    [Export] public float LaserLength = 1000;
    [Export] public float Damage;

    private Node3D PlayerTarget;

	private Vector3 SwipeOrigin;
    private Vector3 Destination;

    private Vector3 CurrentLaserPos;

    private float CurrentSwipeTime;

    private MeshInstance3D CurrentLaserMesh;
    private bool DealtDamage = false;
    private bool EffectEnded = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        PlayerTarget = GetTree().GetFirstNodeInGroup("PlayerPrimary") as Node3D;
		if (PlayerTarget == null)
		{
            GD.Print("Player is null");
        }

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();

        SwipeOrigin = PlayerTarget.GlobalPosition + new Vector3(rng.RandfRange(-1, 1), rng.RandfRange(-1, 0), rng.RandfRange(-1, 1)) * SwipeRandomizationDistance;

        Destination = SwipeOrigin + ((PlayerTarget.GlobalPosition - SwipeOrigin) * 2) + (new Vector3(rng.RandfRange(-1, 1), rng.RandfRange(0, 1f), rng.RandfRange(-1, 1)) * (SwipeRandomizationDistance / 2));

        CurrentLaserPos = SwipeOrigin;
    }

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        CurrentSwipeTime += (float)delta;

        if (CurrentSwipeTime >= SwipeTime)
        {
            if (!EffectEnded)
            {
                EffectEnded = true;

                if (CurrentLaserMesh != null)
                {
                    CurrentLaserMesh.QueueFree();
                }
            }
            return;
        }

        CurrentLaserPos = SwipeOrigin.Slerp(Destination, Mathf.Clamp(CurrentSwipeTime / SwipeTime, 0, 1));

        Vector3 currentLaserPoint = GlobalPosition + (CurrentLaserPos - GlobalPosition).Normalized() * LaserLength;

        if (CurrentLaserMesh != null)
        {
            CurrentLaserMesh.QueueFree();
        }
        CurrentLaserMesh = null;

        if (RayCast(out RaycastHit raycasthit, GetWorld3d().DirectSpaceState, GlobalPosition, currentLaserPoint, false, true, LaserHitCollisionMask))
        {
            CurrentLaserMesh = Debug.DrawLine(this, GlobalPosition, raycasthit.Point, new Color(1, 1, 1));

            Node3D effect = ImpactEffect.Instantiate() as Node3D;
            AddChild(effect);
            effect.GlobalPosition = raycasthit.Point;
            GetTree().CallGroup("Enemies", "OnDamage", raycasthit.Point, 10);

            if (!DealtDamage)
            {
                if (raycasthit.Collider is PlayerMovementController movementController)
                {
                    GetTree().CallGroup("Player", "OnDamage", raycasthit.Point, 1000, Damage);
                    DealtDamage = true;
                }
            }
        }
        else
        {
            CurrentLaserMesh = Debug.DrawLine(this, GlobalPosition, currentLaserPoint, new Color(1, 1, 1));
        }
    }
}
