using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

public partial class AIGroupController : Node3D
{
    [Export] public Array<NodePath> SupplyPlatoonControllerPaths;

    //public List<Vector3> traversalPaths = new List<Vector3>();

    private List<AIPlatoonController> platoonControllers = new List<AIPlatoonController>();

    public override void _Ready()
    {
        foreach (var item in SupplyPlatoonControllerPaths)
        {

            platoonControllers.Add(GetNode<AIPlatoonController>(item));

        }

        //if (traversalPaths.Count > 0)
        //{
        //    UpdateTraversalPaths(traversalPaths);
        //}
    }

    public void UpdateTraversalPaths(List<Vector3> newTraverse)
    {
        foreach (var item in platoonControllers)
        {
            if (item.ThisAIType != AIPlatoonController.AITypes.follow)
            {
                item.UpdateTraversalPath(newTraverse);
            }
        }
    }

    public void OnTroopLost(AIPlatoonController controller)
    {
        if (platoonControllers.Contains(controller))
        {
            GD.Print("Called Troops Lost");
            platoonControllers.Remove(controller);
            if (platoonControllers.Count == 0)
            {
                GD.Print("out of troops");
                QueueFree();
            }
        }
    }
}
