using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class FpsLabel : Label
{
	[Export] public int FrameAvgCount = 5;

	private Queue<double> lastframes = new Queue<double>();
	
	public override void _Process(double delta)
	{
		lastframes.Enqueue(Engine.GetFramesPerSecond());

        if (lastframes.Count > FrameAvgCount)
		{
			lastframes.Dequeue();
		}

		Text = "FPS:" + lastframes.Average().ToString();
	}
}
