using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Godot.TextServer;
using static PhysicsExtensions;


public partial class AITroopController : Node3D
{
    [Export] public bool DebugDisable = false;

    [Export(PropertyHint.Layers3dPhysics)] public uint MovementColliderLayers;

    [Export(PropertyHint.Layers3dPhysics)] public uint EffectRegionColliderLayers;
    [Export] public float EffectCheckRate = 0.5f;
    [Export] public float EffectCheckRadius = 1;

    [Export] public PackedScene DeathEffect;

    [Export] public NodePath VisualEffectPath;

    [Export] public NodePath NavAgentPath;

    [Export] public Node3D Visuals;

    [Export] public float MovementSpeed = 1;
    [Export] public float SlowedMovementSpeed = 0.2f;
    [Export] public float RotationSpeed = 1;

    public Vector3 LookDirection { get; set; }
    public bool EngageTarget { get; set; } = false;
    public bool IsMoving { get; private set; } = false;


    private Node3D VisualEffect;

    private NavigationAgent3D navAgent;
    private Vector3 lookDir;

    private Vector3 currentTarget { get; set; }
    private Vector3 lastMoveTarget;

    private Vector3 Velocity = new Vector3();

    private float myUniqueDelay;
    private float currentDelay = 0;
    private float curMovementSpeed;

    private float currentEffectCheckRate;

    //private MeshInstance3D lastDebug;

    public override void _Ready()
    {
        if (DebugDisable)
        {
            return;
        }

        RandomNumberGenerator rng = new RandomNumberGenerator();
        rng.Randomize();
        myUniqueDelay = rng.RandfRange(0, 2);

        if (VisualEffectPath != null)
        {
            VisualEffect = GetNode<Node3D>(VisualEffectPath);
            VisualEffect.Visible = false;
        }

        navAgent = GetNode<NavigationAgent3D>(NavAgentPath);

        if (RayCast(out RaycastHit raycasthit, GetWorld3d().DirectSpaceState, GlobalPosition + new Vector3(0, 20, 0), GlobalPosition - new Vector3(0, 20, 0), false, true, MovementColliderLayers))
        {
            GlobalPosition = raycasthit.Point;
        }
        //navAgent.TargetLocation = GlobalPosition;
        curMovementSpeed = MovementSpeed;
    }

    

    public override void _Process(double delta)
    {
        if (DebugDisable)
        {
            return;
        }

        if (VisualEffect != null)
        {
            if (EngageTarget)
            {
                VisualEffect.Visible = true;
            }
            else
            {
                VisualEffect.Visible = false;
            }
        }

        if (!IsMoving || EngageTarget)
        {
            lookDir = (LookDirection - GlobalPosition).Normalized();
            RotationDegrees = new Vector3(RotationDegrees.x, Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(RotationDegrees.y), Mathf.Atan2(-lookDir.x, -lookDir.z), (float)delta * RotationSpeed)), RotationDegrees.z);
        }
        else
        {
            currentEffectCheckRate -= (float)delta;
            if (currentEffectCheckRate <= 0)
            {
                currentEffectCheckRate += EffectCheckRate;

                CheckEffects();
            }

            lookDir = Velocity.Normalized().Rotated(Vector3.Up, 90);
            RotationDegrees = new Vector3(RotationDegrees.x, Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(RotationDegrees.y), Mathf.Atan2(-lookDir.x, -lookDir.z), (float)delta * RotationSpeed)), RotationDegrees.z);
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (DebugDisable)
        {
            return;
        }

        if (currentDelay > 0)
        {
            currentDelay -= (float)delta;
            if (currentDelay <= 0)
            {
                SetTargetMovement();
            }
        }

        if (GlobalPosition.DistanceTo(navAgent.GetNextLocation()) > 0.1)
        {
            if (!EngageTarget)
            {
                Velocity = (navAgent.GetNextLocation() - GlobalPosition).Normalized() * curMovementSpeed;
                GlobalPosition += Velocity * (float)delta;

                IsMoving = true;
            }
        }
        else
        {
            IsMoving = false;
        }

        if (IsMoving)
        {
            if (RayCast(out RaycastHit raycasthit, GetWorld3d().DirectSpaceState, GlobalPosition + new Vector3(0, 2, 0), GlobalPosition - new Vector3(0, 2, 0), false, true, MovementColliderLayers))
            {
                Visuals.GlobalPosition = raycasthit.Point;
                //if (raycasthit.Normal == Vector3.Zero)
                //{
                //    raycasthit.Normal = Vector3.Up;
                //}
                //Visuals.LookAt(raycasthit.Point + raycasthit.Normal);
            }
        }
    }

    public virtual void UpdateTargetPosition(Vector3 newTarget)
    {
        currentTarget = newTarget;
        currentDelay = myUniqueDelay;
    }

    public virtual void DestroyTroops()
    {
        Node3D deathEffect = DeathEffect.Instantiate() as Node3D;

        GetTree().Root.GetChild(0).AddChild(deathEffect);
        CallDeferred("WaitDeath", deathEffect);
    }



    private void WaitDeath(Node3D deathEffect)
    {
        deathEffect.GlobalPosition = GlobalPosition;
        QueueFree();
    }

    private void CheckEffects()
    {
        var Results = OverlapSphere3D(GetWorld3d().DirectSpaceState, EffectCheckRadius, GlobalPosition, true, false, EffectRegionColliderLayers, 1);
        curMovementSpeed = MovementSpeed;
        if (Results.Count > 0)
        {
            EffectController foundEffect = (EffectController)Results[0]["collider"];
            if (foundEffect != null && foundEffect.effectType == EffectController.EffectType.Snow)
            {
                curMovementSpeed = SlowedMovementSpeed;
            }
        }
    }

    private void SetTargetMovement()
    {
        if (lastMoveTarget.DistanceTo(currentTarget) > 3)
        {
            //if (lastDebug != null)
            //{
            //    lastDebug.QueueFree();
            //}
            //lastDebug = null;

            if (RayCast(out RaycastHit hitPoint, GetWorld3d().DirectSpaceState, currentTarget + new Vector3(0,20,0), currentTarget - new Vector3(0,20,0),false,true, MovementColliderLayers))
            {
                //lastDebug = Debug.DrawLine(this, currentTarget + new Vector3(0, 20, 0), currentTarget - new Vector3(0, 20, 0), new Color(1, 0, 0));
                //GD.Print("hit, Distance: " + navAgent.TargetLocation.DistanceTo(hitPoint.Point) + " Name: " + Name);
                navAgent.TargetLocation = hitPoint.Point;
                
            }
            //else
            //{
            //    lastDebug = Debug.DrawLine(this, currentTarget + new Vector3(0, 20, 0), currentTarget - new Vector3(0, 20, 0), new Color(1, 1, 1));
            //}

            lastMoveTarget = currentTarget;
        }
    }
}

