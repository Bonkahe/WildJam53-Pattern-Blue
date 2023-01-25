using Godot;
using Godot.Collections;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

public partial class AIPlatoonController : Node3D
{
    [Export] public bool DebugDisable = false;

    public enum AITypes { none, ambientfire, follow, traverse, wander, chase};
	[Export] public Array<NodePath> PlatoonControllerPaths;
	[Export] public Node3D playerNode;

    [Export] public Node3D MarkerPosition;

    [Export] public AITypes ThisAIType { get; set; } = AITypes.none;
    [Export] public bool AllowEngagement = false;
    [Export] public float ChaseDistance;
    [Export] public float EngageDistance;

    [Export] public float AmbientDamageRate = 0.2f;
    [Export] public float AmbientLerpTowardsTarget = 0.5f;
    [Export] public float AmbientEffectDamage = 10;
    [Export] public float AmbientEffectRadius = 4;
    [Export] public NodePath AmbientEffectPath;
    [Export] public Vector2 WanderDelayRange;
    [Export] public float TraverseDelay = 2;
    [Export] public AIPlatoonController FollowTarget { get; set; }
    [Export] public Vector2 WanderDistanceRange;

    public Vector3 PlatoonCurPos { get; set; }

    public List<Vector3> TraversePoints { get; set; } = new List<Vector3>();
    private int traverseIndex = 0;

    private Node3D ambientEffect;
    private AudioStreamPlayer3D ambientAudio;

	private List<AITroopController> platoonControllers = new List<AITroopController>();
	private List<Vector3> relativePositions = new List<Vector3>();

    private Vector3 startingPoint;
	private Vector3 lastTargetLocation;
	private Vector3 currentMoveTarget;
    private Vector3 followOffset;

    private bool IsEngaging = false;
	private bool IsMoving = false;
	private float currentMovementDelay = 0;
    private float ambientDamageTime = 0; 

	private RandomNumberGenerator rng = new RandomNumberGenerator();

    private MeshInstance3D CurrentDebugLine;

	public override void _Ready()
	{
        if (DebugDisable)
        {
            return;
        }

        playerNode = GetTree().GetNodesInGroup("PlayerPrimary")[0] as Node3D;
        //playerNode = GetTree().Root.GetChild(0).GetNode<Node3D>("/PlayerContainer/Player");

        if (AmbientEffectPath != null)
        {
            ambientEffect = GetNode<Node3D>(AmbientEffectPath);
            ambientAudio = ambientEffect.GetNode<AudioStreamPlayer3D>("Audio");
            ambientAudio.Stop();
            ambientEffect.Visible = false;
        }


        foreach (var item in PlatoonControllerPaths)
		{
			platoonControllers.Add(GetNode<AITroopController>(item));
        }

		if (platoonControllers.Count > 0)
		{
			currentMoveTarget = GetAveragePosition();
            startingPoint = currentMoveTarget;

            relativePositions = platoonControllers.Select(x => x.GlobalPosition - currentMoveTarget).ToList();
        }

        if (FollowTarget != null)
        {

            //rng.Randomize();
            //followOffset = new Vector3(rng.RandfRange(-1, 1), 0, rng.RandfRange(-1, 1));
            //followOffset = followOffset * rng.RandfRange(WanderDistanceRange.x, WanderDistanceRange.y);
            followOffset = currentMoveTarget - FollowTarget.GlobalPosition;
        }

        currentMovementDelay = GetWanderDelay();
    }

	public override void _Process(double delta)
	{
        if (DebugDisable || !IsInstanceValid(playerNode))
        {
            return;
        }
        PlatoonCurPos = GetAveragePosition();

        //DebugPos
        //if (CurrentDebugLine != null)
        //    CurrentDebugLine.QueueFree();
        //CurrentDebugLine = null;
        

        //EndDebug


        UpdateTargetLocation(playerNode.GlobalPosition);

        if (IsMoving)
        {
            if (platoonControllers.Where(x => !x.IsMoving || x.EngageTarget).Count() > platoonControllers.Count / 2) //more than half are stopped.
            {
                IsMoving = false;
            }
        }

        if(AllowEngagement)
        {
            bool newIsEngaging = PlatoonCurPos.DistanceTo(playerNode.GlobalPosition) < EngageDistance;
            if (newIsEngaging != IsEngaging)
            {
                EngagementOrders(newIsEngaging);
            }
        }

        //if (IsEngaging)
        //{
        //    CurrentDebugLine = Debug.DrawLine(this, PlatoonCurPos, playerNode.GlobalPosition, new Color(0, 1, 0));
        //}
        //else
        //{
        //    CurrentDebugLine = Debug.DrawLine(this, PlatoonCurPos, playerNode.GlobalPosition, new Color(1, 0, 0));
        //}

        switch (ThisAIType)
        {
            case AITypes.none:

                break;
            case AITypes.ambientfire:
                if (IsEngaging)
                {

                    Vector3 FireEffectPos = PlatoonCurPos;
                    FireEffectPos.y = playerNode.GlobalPosition.y;
                    ambientEffect.GlobalPosition = FireEffectPos;
                    ambientEffect.GlobalPosition += (playerNode.GlobalPosition - ambientEffect.GlobalPosition) * AmbientLerpTowardsTarget;

                    if (!ambientEffect.Visible)
                    {
                        ambientAudio.Play();
                        ambientEffect.Visible = true;
                    }

                    ambientDamageTime -= (float)delta;
                    if (ambientDamageTime <= 0)
                    {
                        ambientDamageTime += AmbientDamageRate;

                        GetTree().CallGroup("Player", "OnDamage", ambientEffect.GlobalPosition, AmbientEffectRadius, AmbientEffectDamage);
                    }
                }
                else
                {
                    if (ambientEffect.Visible)
                    {
                        ambientAudio.Stop();
                        ambientEffect.Visible = false;
                    }
                }
                //handled by the general movement and allow engagement rules
                break;
            case AITypes.traverse:
                if (!IsMoving && !IsEngaging)
                {
                    if (currentMovementDelay > 0)
                    {
                        currentMovementDelay -= (float)delta;
                    }
                    else
                    {
                        currentMovementDelay = TraverseDelay;

                        //MovePlatoon(TraversePoints[traverseIndex]);

                        if (PlatoonCurPos.DistanceTo(TraversePoints[traverseIndex]) < 5)
                        {
                            traverseIndex++;
                            if (traverseIndex == TraversePoints.Count)
                            {
                                traverseIndex = 0;
                                GetTree().CallGroup("EnemyCommander", "TraversalComplete");
                            }
                            MovePlatoon(TraversePoints[traverseIndex]);
                        }
                    }
                }
                break;
            case AITypes.follow:
                if (FollowTarget != null) {

                    if (currentMovementDelay > 0)
                    {
                        currentMovementDelay -= (float)delta;
                    }
                    else
                    {
                        currentMovementDelay = GetWanderDelay();


                        MovePlatoon(FollowTarget.PlatoonCurPos + followOffset);
                    }
                }
                break;
            case AITypes.wander:
                if (!IsMoving)
                {
                    if (currentMovementDelay > 0)
                    {
                        currentMovementDelay -= (float)delta;
                    }
                    else
                    {
                        currentMovementDelay = GetWanderDelay();
                        rng.Randomize();

                        Vector3 newTarget = new Vector3(rng.RandfRange(-1, 1), 0, rng.RandfRange(-1, 1));
                        newTarget = newTarget * rng.RandfRange(WanderDistanceRange.x, WanderDistanceRange.y);
                        newTarget = startingPoint + newTarget;

                        MovePlatoon(newTarget);
                    }
                }
                break;
            case AITypes.chase:
                if (currentMovementDelay > 0)
                {
                    currentMovementDelay -= (float)delta;
                }
                else
                {
                    if (PlatoonCurPos.DistanceTo(playerNode.GlobalPosition) < ChaseDistance)
                    {
                        currentMovementDelay = GetWanderDelay();

                        rng.Randomize();
                        Vector3 newTarget = new Vector3(rng.RandfRange(-1, 1), 0, rng.RandfRange(-1, 1));
                        newTarget = newTarget * rng.RandfRange(WanderDistanceRange.x, WanderDistanceRange.y);
                        newTarget = lastTargetLocation + newTarget;

                        MovePlatoon(newTarget);
                    }
                    else
                    {
                        if (FollowTarget != null)
                        {
                            ThisAIType = AITypes.follow;
                        }
                    }
                }


                if (ambientEffect != null)
                {
                    if (IsEngaging)
                    {
                        Vector3 FireEffectPos = PlatoonCurPos;
                        FireEffectPos.y = playerNode.GlobalPosition.y;
                        ambientEffect.GlobalPosition = FireEffectPos;
                        ambientEffect.GlobalPosition += (playerNode.GlobalPosition - ambientEffect.GlobalPosition) * AmbientLerpTowardsTarget;

                        if (!ambientEffect.Visible)
                        {
                            ambientAudio.Play();
                            ambientEffect.Visible = true;
                        }

                        ambientDamageTime -= (float)delta;
                        if (ambientDamageTime <= 0)
                        {
                            ambientDamageTime += AmbientDamageRate;

                            GetTree().CallGroup("Player", "OnDamage", ambientEffect.GlobalPosition, AmbientEffectRadius, AmbientEffectDamage);
                        }
                    }
                    else
                    {
                        if (ambientEffect.Visible)
                        {
                            ambientAudio.Stop();
                            ambientEffect.Visible = false;
                        }
                    }
                }

                break;
        }

	}

    public void UpdateTraversalPath(List<Vector3> newTraversalPaths)
    {
        TraversePoints = newTraversalPaths;
        traverseIndex = 0;
        for (int i = 0; i < TraversePoints.Count; i++)
        {
            if (TraversePoints[i].DistanceTo(PlatoonCurPos) < TraversePoints[traverseIndex].DistanceTo(PlatoonCurPos))
            {
                traverseIndex = i; //set to the closest point.
            }
        }

        ThisAIType = AITypes.traverse; //sets to traverse if it isn't.
        MovePlatoon(TraversePoints[traverseIndex]);
    }

    public void EngagementOrders(bool newEgagementOrders)
    {
        if (newEgagementOrders && ThisAIType == AITypes.follow)
        {
            ThisAIType = AITypes.chase;
        }

        IsEngaging = newEgagementOrders;
        foreach (var item in platoonControllers)
        {
            item.EngageTarget = newEgagementOrders;
        }
    }

	public void UpdateTargetLocation(Vector3 newTargetLoc)
	{
		if (newTargetLoc.DistanceTo(lastTargetLocation) > 0.5f)
		{
			lastTargetLocation = newTargetLoc;
			foreach (var item in platoonControllers)
			{
				item.LookDirection = newTargetLoc;
            }
		}
    }

    public void MovePlatoon(Vector3 targetPosition)
    {
        targetPosition.y = PlatoonCurPos.y;

        if (currentMoveTarget.DistanceTo(targetPosition) > 3)
        {
            currentMoveTarget = targetPosition;

            IsMoving = true;
            for (int i = 0; i < platoonControllers.Count; i++)
            {
                platoonControllers[i].UpdateTargetPosition(targetPosition + relativePositions[i]);
            }
        }
    }

    public void OnDamage(Vector3 targetPosition, float damageRadius)
    {
        if (PlatoonCurPos.DistanceTo(targetPosition) - damageRadius <= 0)
        {
            foreach (var item in platoonControllers)
            {
                item.DestroyTroops();
            }
            CallDeferred("WaitDeath");
        }
        
    }

    private void WaitDeath()
    {
        QueueFree();
        GetTree().CallGroup("EnemyCommand", "OnTroopLost", this);
    }



    private Vector3 GetAveragePosition()
	{
       Vector3 average = new Vector3(
                platoonControllers.Average(x => x.GlobalPosition.x),
                platoonControllers.Average(x => x.GlobalPosition.y),
                platoonControllers.Average(x => x.GlobalPosition.z));

        if (MarkerPosition != null)
        {
            MarkerPosition.GlobalPosition = average;
        }
		return average;
    }

    private float GetWanderDelay()
    {
        rng.Randomize();
        return rng.RandfRange(WanderDelayRange.x, WanderDelayRange.y);
    }
}
