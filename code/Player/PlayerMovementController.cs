using Godot;
using System;

public partial class PlayerMovementController : CharacterBody3D
{
	[Export] public float SpeedBoostLength = 4;
    [Export] public Curve SpeedBoostCurve;

    [Export] public float EasyModeSpeedBoost = 2.0f;

    [Export] public float RotationSpeed = 5.0f;
    [Export] public float Speed = 5.0f;
	[Export] private Node3D CloudsMesh;

	[Export] public Node3D CameraHolder;

	//private Node3D cloudsMesh;
	//private Node3D cameraHolder;

	private Vector3 curRotation;

	private bool isDead = false;

	private float currentMoveSpeed;
	private float currentSpeedBoost = 0;

	private bool EasyMode;

	public override void _Ready()
	{
        //cloudsMesh = GetNode<Node3D>(CloudsMeshPath);
        //cameraHolder = GetNode<Node3D>(CameraHolderPath);
        curRotation = CloudsMesh.GlobalRotation;
		OnSettingsUpdated();
    }

	public override void _PhysicsProcess(double delta)
	{
		if (isDead) return;


		currentMoveSpeed = Speed;
		if (currentSpeedBoost < SpeedBoostLength)
		{
			currentSpeedBoost += (float)delta;

			currentMoveSpeed += SpeedBoostCurve.Sample(Mathf.Clamp(currentSpeedBoost / SpeedBoostLength, 0, 1));
        }

        Vector3 velocity = Velocity;
		Vector2 inputDir = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 direction = (CameraHolder.Transform.basis * new Vector3(inputDir.x, 0, inputDir.y)).Normalized();
		if (direction != Vector3.Zero)
		{
			velocity.x = direction.x * currentMoveSpeed;
			velocity.z = direction.z * currentMoveSpeed;
			if (EasyMode)
			{
				velocity *= EasyModeSpeedBoost;
			}

            CloudsMesh.RotationDegrees = new Vector3(CloudsMesh.RotationDegrees.x, Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(CloudsMesh.RotationDegrees.y), Mathf.Atan2(-direction.x, -direction.z), (float)delta * RotationSpeed)), CloudsMesh.RotationDegrees.z);
        }
		else
		{
			velocity.x = Mathf.MoveToward(Velocity.x, 0, currentMoveSpeed);
			velocity.z = Mathf.MoveToward(Velocity.z, 0, currentMoveSpeed);
		}

		Velocity = velocity;
		MoveAndSlide();
    }

	public void BeginDeath()
	{
		isDead = true;

    }

	public void SpeedBoost()
	{
		currentSpeedBoost = 0;
    }

	public void OnSettingsUpdated()
	{
        EasyMode = SceneDataStatic.EasyDifficulty;
    }
}
