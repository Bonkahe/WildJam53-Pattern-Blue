using Godot;
using System;

public partial class BossBattleSkip : Node
{
	public void SkipToBoss()
	{
		SceneDataStatic.AssemblyCompletion = 1;
        GetTree().CallGroup("FinalBossButton", "_on_button_button_up");
    }
}
