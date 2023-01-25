using Godot;
using Godot.Collections;
using System;

public partial class EnemiesFinalScaler : Node3D
{

    public override void _Ready()
    {
        Array<Node> children = GetChildren();
        int totalChildren = children.Count;

        float totalAllowed = SceneDataStatic.AssemblyCompletion;

        int allowedChildren = (int)((float)totalChildren * totalAllowed);

        GD.Print("Current Children :" + totalChildren);
        GD.Print("Allowed Children :" + allowedChildren);

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();

        while (children.Count > allowedChildren)
        {
            int child = rng.RandiRange(0, children.Count - 1);
            children[child].QueueFree();
            children.RemoveAt(child);
        }
    }
}
