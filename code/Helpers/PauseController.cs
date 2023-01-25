using Godot;
using System;

public partial class PauseController : Control
{
	private float Delay = 3;
	private bool IsDead = false;

	public override void _Ready()
	{
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }
	public override void _Process(double delta)
	{
		if (Delay > 0)
		{
			Delay -= (float)delta;
		}
    }
	public override void _Input(InputEvent @event)
	{
		if (IsDead || Delay > 0)
		{
			return;
		}
		if (@event.IsActionPressed("pause"))
		{
			if (!Visible)
			{
				Pause();
			}
			else
			{
				UnPause();
            }
		}
	}

	public void Pause()
	{
        Input.MouseMode = Input.MouseModeEnum.Visible;
        GetTree().Paused = true;
        Visible = true;
    }

    public void UnPause()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
        GetTree().Paused = false;
        Visible = false;
    }

	public void OnDeath()
	{
		IsDead = true;
    }
}
