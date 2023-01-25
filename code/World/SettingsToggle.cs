using Godot;
using System;

public partial class SettingsToggle : CheckButton
{
	[Export] public SettingType thisType;

    public enum SettingType { Difficulty, Visuals, Music}

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		switch (thisType)
		{
			case SettingType.Difficulty:
				ButtonPressed = SceneDataStatic.EasyDifficulty;
				break;
			case SettingType.Visuals:
                ButtonPressed = SceneDataStatic.HighSettings;
                break;
            case SettingType.Music:
                ButtonPressed = SceneDataStatic.MusicEnabled;
                AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), !SceneDataStatic.MusicEnabled);
                break;
        }
	}

	public void OnButtonUp()
	{
        GD.Print("Settings changed");
        switch (thisType)
        {
            case SettingType.Difficulty:
                SceneDataStatic.EasyDifficulty = ButtonPressed;
                break;
            case SettingType.Visuals:
                SceneDataStatic.HighSettings = ButtonPressed;
                break;
            case SettingType.Music:
                SceneDataStatic.MusicEnabled = ButtonPressed;
                AudioServer.SetBusMute(AudioServer.GetBusIndex("Music"), !SceneDataStatic.MusicEnabled);
                break;
        }

        GetTree().CallGroup("Settings", "OnSettingsUpdated");
    }
}
