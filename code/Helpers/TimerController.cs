using Godot;
using System;

public partial class TimerController : Label
{
    [Export] public float InitialSecondDelay = 3;
    [Export] public float GameTimeLimitMinutes = 10;
    [Export] public Label SubSecondLabel;

    private float gameTimeinSeconds;

    private bool complete = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        gameTimeinSeconds = GameTimeLimitMinutes * 60;

        SetText();
    }

    public override void _ExitTree()
    {
        GD.Print( "result: " + gameTimeinSeconds);
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (complete)
        {
            return;
        }

        if (InitialSecondDelay > 0)
        {
            InitialSecondDelay -= (float)delta;
        }
        else
        {
            gameTimeinSeconds -= (float)delta;


            if (gameTimeinSeconds <= 0)
            {
                gameTimeinSeconds = 0;
                SetText();
                GetTree().CallGroup("StageSwap", "_on_button_button_up");
                complete = true;
            }
            else
            {
                SetText();
            }
            
        }
    }

    private void SetText()
    {
        int currentMinutes = (int)(gameTimeinSeconds / 60);
        if (currentMinutes < 10)
        {
            Text = "0" + currentMinutes + ":";
        }
        else
        {
            Text = currentMinutes + ":";
        }

        int remainingSeconds = (int)gameTimeinSeconds - (currentMinutes * 60);

        if (remainingSeconds < 10)
        {
            Text += "0" + remainingSeconds;
        }
        else
        {
            Text += remainingSeconds;
        }

        int decimals = (int)((gameTimeinSeconds - (remainingSeconds + (currentMinutes * 60))) * 100);
        if (decimals < 10)
        {
            SubSecondLabel.Text = ":0" + decimals;
        }
        else
        {
            SubSecondLabel.Text = ":" + decimals;
        }
    }
}
