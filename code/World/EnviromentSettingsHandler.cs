using Godot;
using System;

public partial class EnviromentSettingsHandler : WorldEnvironment
{
	[Export] public Godot.Environment HighEnviroment;
    [Export] public Godot.Environment LowEnviroment;


    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		Environment = SceneDataStatic.HighSettings ? HighEnviroment : LowEnviroment;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public void OnSettingsUpdated()
	{
        Environment = SceneDataStatic.HighSettings ? HighEnviroment : LowEnviroment;
    }
}
