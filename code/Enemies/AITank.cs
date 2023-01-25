using Godot;
using System;

public partial class AITank : AITroopController
{
	[Export] public PackedScene ImpactEffect;
    [Export] public PackedScene TankFireEffect;

    [Export] public Node3D cannonShot;
    [Export] public Node3D cannonShotOrigin;

    [Export] public float CannonAimVariance;
    [Export] public Vector2 CannonReloadVariance;
	[Export] public float CannonReloadTime = 3;
	[Export] public float CannonDamage = 10;
    [Export] public float BlastRadius = 2;
    [Export] public float FlightSpeed = 100;

	private float currentReloadTime;

	//private Node3D cannonShot;
 //   private Node3D cannonShotOrigin;

    private RandomNumberGenerator rng = new RandomNumberGenerator();


	private Vector3 ArcOrigin;
	private Vector3 ArcDestination;
	private float ArcDuration;
	private float currentFiredTime;
	private bool isFiring = false;

	public override void _Ready()
	{
		base._Ready();

		//cannonShot = GetNode<Node3D>(CannonShotPath);
  //      cannonShotOrigin = GetNode<Node3D>(CannonShotOriginPath);


        SetReloadTime();
    }


	public override void _Process(double delta)
	{
		base._Process(delta);

		if (currentReloadTime > 0)
		{
			currentReloadTime -= (float)delta;
        }
		else if (EngageTarget)
		{
			FireCannon();
        }

		if (isFiring)
		{
            currentFiredTime += (float)delta;

			if (currentFiredTime > ArcDuration || cannonShot.GlobalPosition.DistanceTo(LookDirection) < CannonAimVariance)
			{
				isFiring = false;
				cannonShot.Visible = false;

				Node3D impactScene = ImpactEffect.Instantiate() as Node3D;
                AddChild(impactScene);
                impactScene.GlobalPosition = ArcDestination;

                GetTree().CallGroup("Player", "OnDamage", ArcDestination, BlastRadius, CannonDamage);
            }
			else
			{
				float currentLerp = currentFiredTime / ArcDuration;

                cannonShot.GlobalPosition = ArcOrigin.Lerp(ArcDestination, currentLerp);
            }
        }
	}

	private void FireCannon()
	{
		rng.Randomize();
		ArcOrigin = cannonShotOrigin.GlobalPosition;
		ArcDestination = LookDirection + new Vector3(rng.RandfRange(-CannonAimVariance, CannonAimVariance), rng.RandfRange(-CannonAimVariance, CannonAimVariance), rng.RandfRange(-CannonAimVariance, CannonAimVariance));

        cannonShot.GlobalPosition = ArcOrigin;
		cannonShot.LookAt(ArcDestination);

		ArcDuration = ArcOrigin.DistanceTo(ArcDestination) / FlightSpeed;
		currentFiredTime = 0;
		

        Node3D fireScene = TankFireEffect.Instantiate() as Node3D;
		AddChild(fireScene);
        fireScene.GlobalPosition = ArcOrigin;
		//fireScene.GetNode<AudioStreamPlayer3D>("Audio").Play();

        cannonShot.Visible = true;
		isFiring = true;
        SetReloadTime();
    }

	private void SetReloadTime()
	{
        rng.Randomize();
		currentReloadTime = CannonReloadTime + rng.RandfRange(CannonReloadVariance.x, CannonReloadVariance.y);

    }
}
