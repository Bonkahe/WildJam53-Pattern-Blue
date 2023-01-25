using Godot;
using System;

public partial class AnimationStarter : Node3D
{
	[Export] public NodePath AnimationPlayerPath;
	[Export] public string InitializationAnimationName;

	private AnimationPlayer animationPlayer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		animationPlayer = GetNode<AnimationPlayer>(AnimationPlayerPath);
		animationPlayer.Play(InitializationAnimationName);
	}

}
