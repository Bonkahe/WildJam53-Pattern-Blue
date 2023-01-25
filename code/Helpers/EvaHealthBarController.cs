using Godot;
using System;

public partial class EvaHealthBarController : ProgressBar
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

    public void UpdateEvaHealth(float lerp)
    {
        Value = Mathf.Lerp((float)MinValue, (float)MaxValue, lerp);
    }
}
