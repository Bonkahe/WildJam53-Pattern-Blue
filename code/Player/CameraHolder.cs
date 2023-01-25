using Godot;
using System;

public partial class CameraHolder : Node3D
{
	[Export] public NodePath PlayerObjectPath;
	[Export] public float SmoothSpeed = 5;
    [Export] public float MouseSensitivity = 0.4f;

	[Export] public float VerticalAngleLimit = 90;

	private float curAngleLimit;
    private Node3D playerObject;
	private Vector3 currotation = new Vector3();
	private bool isDead = false;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		curAngleLimit = Mathf.DegToRad(VerticalAngleLimit);


        playerObject = GetNode<Node3D>(PlayerObjectPath);
    }

	public override void _Input(InputEvent inputEvent)
	{
		if (isDead) return;

		if (inputEvent is InputEventMouseMotion mouseMotion)
		{
			currotation = Rotation;
			currotation.y -= mouseMotion.Relative.x * MouseSensitivity;

			currotation.x = Mathf.Clamp(currotation.x - mouseMotion.Relative.y * MouseSensitivity, -curAngleLimit, curAngleLimit);
            Rotation = currotation;
        }
		else 
		{
            Vector2 inputDir = Input.GetVector("look_left", "look_right", "look_forward", "look_back");
			if (inputDir != Vector2.Zero)
			{
                currotation = Rotation;
                currotation.y -= inputDir.x * MouseSensitivity;

                currotation.x = Mathf.Clamp(currotation.x - inputDir.y * MouseSensitivity, -curAngleLimit, curAngleLimit);
                Rotation = currotation;
            }
        }
	}

	public override void _PhysicsProcess(double delta)
	{
        if (isDead) return;

        if (GlobalPosition.DistanceTo(playerObject.GlobalPosition) > 0.1f)
		{
			Vector3 currentPosition = GlobalPosition;
			GlobalPosition = currentPosition.Slerp(playerObject.GlobalPosition, (float)delta * SmoothSpeed);
		}
    }



    public void BeginDeath()
    {
        isDead = true;
    }

}
