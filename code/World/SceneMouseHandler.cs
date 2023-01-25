using Godot;
using System;

public partial class SceneMouseHandler : Node3D
{
	[Export] public bool fastClose = true;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Input.MouseMode = Input.MouseModeEnum.Captured;
		if (!OS.IsDebugBuild())
		{
			fastClose = false;
		}

		if (fastClose)
		{
			GD.Print("** Fast Close enabled in the scene script **");
            GD.Print("** 'Esc' to close 'F1' to release mouse **");
        }

		SetProcessInput(fastClose);
	}

	public override void _Input(InputEvent inputEvent)
	{
		//if (inputEvent.IsActionPressed("ui_cancel"))
		//{
		//	GetTree().Quit();
		//}
		if (inputEvent.IsActionPressed("change_mouse_input"))
		{
			if (Input.MouseMode == Input.MouseModeEnum.Captured)
			{
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
			else
			{
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
		}
	}

}
