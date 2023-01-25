using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using static System.Formats.Asn1.AsnWriter;

//struct TransportGroup
//{
//	public AIGroupController aIGroupController { get; set; }
//	public int spawnPointIndex { get; set; }
//}

public partial class AICommandController : Node3D
{
	[Export] public bool DebugDisable = false;

	[Export] public Array<NodePath> SpawnPointPaths;

    [Export] public Node3D dropOffPoint;
    [Export] public Node3D playerNode;

	[Export] public PackedScene TransportGroup;
	[Export] public PackedScene KillerGroup;

	[Export] public float SpawnRate = 2;
	[Export] public float UpdateRate = 0.5f;
	[Export] public int IdealKillerGroupCount = 3;
	[Export] public int IdealTransportGroupCount = 4;
	[Export] public float ResourcePerTraversal = 10;
	[Export] public float TotalRequiredResources = 200;

	private List<AIGroupController> currentTransportGroups = new List<AIGroupController>();
    private List<AIGroupController> currentKillerGroups = new List<AIGroupController>();

    private List<Node3D> spawnPoints;

	private int currentUpdateCount = 0;

    private float currentResources;
	private float currentUpdateRate;
    private float currentSpawnRate;
	private bool isKillerTurn = false;

	private RandomNumberGenerator rng = new();
    public override void _Ready()
	{

		spawnPoints = SpawnPointPaths.Select(x => GetNode<Node3D>(x)).ToList();
		//dropOffPoint = GetNode<Node3D>(ResourceDropoffPointPath);
  //      playerNode = GetNode<Node3D>(PlayerNodePath);
        currentUpdateRate = UpdateRate;

		GD.Print("Previous completion : " +  SceneDataStatic.AssemblyCompletion);
		SceneDataStatic.AssemblyCompletion = 0;
    }

	public override void _Process(double delta)
	{
        if (DebugDisable)
        {
			return;
        }

		if (currentSpawnRate > 0)
		{
			currentSpawnRate -= (float)delta;
        }

        currentUpdateRate += (float)delta;
		if (currentUpdateRate >= UpdateRate)
		{
            //currentUpdateCount = 0;
            currentUpdateRate -= UpdateRate;

			if (currentSpawnRate <= 0)
			{
				if (isKillerTurn)
				{
                    CycleKillerGroups();
                    CycleTransportGroups();
                }
				else
				{
                    CycleTransportGroups();
                    CycleKillerGroups();
                }
				isKillerTurn = !isKillerTurn;
            }
        }
    }

	private void CycleTransportGroups()
	{
		currentTransportGroups = currentTransportGroups.Where(x => IsInstanceValid(x)).ToList();
		if (currentTransportGroups.Count < IdealTransportGroupCount)
		{
			GD.Print(currentTransportGroups.Count);
			for (int i = 0; i < IdealTransportGroupCount - currentTransportGroups.Count; i++)
			{
                AIGroupController aIGroupController = TransportGroup.Instantiate() as AIGroupController;
                
                AddChild(aIGroupController);

                rng.Randomize();
                Node3D currentClosest  = spawnPoints[rng.RandiRange(0, spawnPoints.Count - 1)];
                aIGroupController.GlobalPosition = currentClosest.GlobalPosition;
                aIGroupController.GlobalRotation = currentClosest.GlobalRotation;
                aIGroupController.UpdateTraversalPaths(new List<Vector3>() { aIGroupController.GlobalPosition, dropOffPoint.GlobalPosition });

                currentTransportGroups.Add(aIGroupController);
				currentSpawnRate = SpawnRate;
            }
		}
    }

    private void CycleKillerGroups()
    {
        currentKillerGroups = currentKillerGroups.Where(x => IsInstanceValid(x)).ToList();
        if (currentKillerGroups.Count < IdealKillerGroupCount)
        {
            for (int i = 0; i < IdealKillerGroupCount - currentKillerGroups.Count; i++)
            {

                AIGroupController newKiller = KillerGroup.Instantiate() as AIGroupController;

                AddChild(newKiller);
                rng.Randomize();
                Node3D currentClosest = spawnPoints[rng.RandiRange(0, spawnPoints.Count - 1)];
                newKiller.GlobalPosition = currentClosest.GlobalPosition;
                newKiller.GlobalRotation = currentClosest.GlobalRotation;

                currentKillerGroups.Add(newKiller);
                currentSpawnRate = SpawnRate;
            }
        }
    }

	public void TraversalComplete()
	{
		currentResources += ResourcePerTraversal;

        currentResources = Mathf.Min(currentResources, TotalRequiredResources);

        SceneDataStatic.AssemblyCompletion = currentResources / TotalRequiredResources;
        

        GetTree().CallGroup("Player", "UpdateAssemblyValue", currentResources / TotalRequiredResources);

        if (currentResources >= TotalRequiredResources)
		{
            GetTree().CallGroup("StageSwap", "_on_button_button_up");
        }
        
    }
}
