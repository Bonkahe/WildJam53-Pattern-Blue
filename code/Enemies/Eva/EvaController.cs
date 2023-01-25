using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using Array = Godot.Collections.Array;

public partial class EvaController : Node3D
{
    [Export] public Label3D stateLabel;

    [Export] public Timer TimerNode;
	[Export] public Node3D PlayerNode;
	[Export] public Node EffectContainerNode;
    [Export] public Node3D EvaHead;

    [Export] public Resource EvaMatRef;
    [Export] public float ShaderEmissionMaxOnHit = 3.16f;

    [Export] public PackedScene DamagedEffect;
    [Export] public PackedScene TeleportEffect;
    [Export] public PackedScene PunchEffect;
    [Export] public PackedScene LaserEffect;
    [Export] public PackedScene BeserkLaserEffect;


    [Export] public Vector2 TeleportTimeRange;
    [Export] public float TeleportJumpRate = 0.1f;

    [Export] public float BeserkFireRate = 0.1f;
    [Export] public float BeserkEnableHealth = 400;


    [Export] public float PunchJumpDistance = 5;
    [Export] public float PunchEngageRange = 15;
    

    [Export] public float MinStartingHealth = 300;
    [Export] public float TotalHealth = 1000;
    [Export] public float RecievedDamageAmount = 45;
    [Export] public float EasyModeRecievedDamageAmount = 95;

    [Export] public float ColliderRadius = 10;
    [Export] public float MovementSpeed = 5;

    

    [Export] public float RotationSpeed = 5;
    [Export] public float RepositionDuration;
	[Export] public float WanderRandomRange;
	[Export] public Vector2 EngagementRange = new Vector2(40, 60);

	[Export] public AnimationTree AnimationTree;

	[Export] public EvaAIState evaAIState = EvaAIState.Reposition;

    public enum EvaAIState { Reposition, Punch, Laser, Damaged, Beserk, Dead};

	public float CurrentHealth;
	private Vector3 currentMoveTarget;

    private RandomNumberGenerator rng = new RandomNumberGenerator();

    private bool timerIsComplete;
    private float shortTimer = 0;

    private StandardMaterial3D evaShader;

    private bool EasyMode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        OnSettingsUpdated();

        evaShader = EvaMatRef as StandardMaterial3D;
        evaShader.EmissionEnergyMultiplier = 0;

        CurrentHealth = Mathf.Lerp(MinStartingHealth, TotalHealth, SceneDataStatic.AssemblyCompletion);
        GetTree().CallGroup("Player", "UpdateEvaHealth", CurrentHealth / TotalHealth);

        TransitionState(evaAIState);
    }

	public override void _PhysicsProcess(double delta)
	{
        switch (evaAIState)
        {
            case EvaAIState.Reposition:
            case EvaAIState.Laser:
                GlobalPosition += (currentMoveTarget - GlobalPosition).Normalized() * (MovementSpeed * (float)delta);
                float distanceToTarget = currentMoveTarget.DistanceTo(PlayerNode.GlobalPosition);

                if (GlobalPosition.DistanceTo(currentMoveTarget) <= 1 || distanceToTarget > EngagementRange.y || distanceToTarget < EngagementRange.x)
                {
                    GetNewTarget();
                }
                break;
            default:
                break;
        }
	}

    private void GetNewTarget(bool teleportPos = false)
    {
        rng.Randomize();

        currentMoveTarget = GlobalPosition + new Vector3(rng.RandfRange(-WanderRandomRange, WanderRandomRange), 0, rng.RandfRange(-WanderRandomRange, WanderRandomRange));

        float clampedDistance = 0;

        if (teleportPos)
        {
            clampedDistance = Mathf.Clamp(PlayerNode.GlobalPosition.DistanceTo(currentMoveTarget), EngagementRange.y, EngagementRange.y + 10);
        }
        else
        {
            clampedDistance = Mathf.Clamp(PlayerNode.GlobalPosition.DistanceTo(currentMoveTarget), EngagementRange.x, EngagementRange.y);
        }

        currentMoveTarget = PlayerNode.GlobalPosition + ((currentMoveTarget - PlayerNode.GlobalPosition).Normalized() * clampedDistance * 0.8f);

        currentMoveTarget.y = 0;

    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
	{
        if (evaAIState != EvaAIState.Dead)
        {
            Vector3 lookLocation = PlayerNode.GlobalPosition;
            lookLocation.y = GlobalPosition.y;
            Vector3 lookDir = (lookLocation - GlobalPosition).Normalized();

            RotationDegrees = new Vector3(RotationDegrees.x, Mathf.RadToDeg(Mathf.LerpAngle(Mathf.DegToRad(RotationDegrees.y), Mathf.Atan2(-lookDir.x, -lookDir.z), (float)delta * RotationSpeed)), RotationDegrees.z);
        }
        switch (evaAIState)
        {
            case EvaAIState.Dead:

                break;
            case EvaAIState.Reposition:
                stateLabel.Text = "Reposition";
                if (timerIsComplete)
                {
                    AttackDefault();
                }
                else if (GlobalPosition.DistanceTo(PlayerNode.GlobalPosition) < PunchEngageRange)
                {
                    TransitionState(EvaAIState.Punch);
                }
                break;
            case EvaAIState.Laser:
                stateLabel.Text = "Laser";
                if (timerIsComplete)
                {
                    TransitionState(EvaAIState.Reposition);
                }
                break;
            case EvaAIState.Punch:
                stateLabel.Text = "Punch";
                if (timerIsComplete)
                {
                    TransitionState(EvaAIState.Reposition);
                }
                break;
            case EvaAIState.Beserk:
                stateLabel.Text = "Beserk";
                shortTimer -= (float)delta;
                if (shortTimer <= 0)
                {
                    SpawnEffect(BeserkLaserEffect, EvaHead.GlobalPosition, EvaHead.GlobalRotation);
                    shortTimer += BeserkFireRate;
                }

                evaShader.EmissionEnergyMultiplier = Mathf.Lerp(0, ShaderEmissionMaxOnHit, Mathf.Clamp((float)(TimerNode.TimeLeft / TimerNode.WaitTime), 0, 1));
                if (timerIsComplete)
                {
                    evaShader.EmissionEnergyMultiplier = 0;
                    TransitionState(EvaAIState.Reposition);
                }
                break;
            case EvaAIState.Damaged:
                stateLabel.Text = "Damaged";
                shortTimer -= (float)delta;
                if (shortTimer <= 0)
                {
                    SpawnEffect(TeleportEffect, GlobalPosition, GlobalRotation);
                    CallDeferred("DefferredTeleport");
                    shortTimer += TeleportJumpRate;
                }
                evaShader.EmissionEnergyMultiplier = Mathf.Lerp(0, ShaderEmissionMaxOnHit, Mathf.Clamp((float)(TimerNode.TimeLeft / TimerNode.WaitTime), 0, 1));


                if (timerIsComplete)
                {
                    evaShader.EmissionEnergyMultiplier = 0;
                    if (CurrentHealth <= BeserkEnableHealth)
                    {
                        TransitionState(EvaAIState.Beserk);
                    }
                    else
                    {
                        AttackDefault();
                    }
                }
                break;
        }
        timerIsComplete = false;
    }

    //lets it finish the frame so the effect begins.
    private void DefferredTeleport()
    {
        GetNewTarget(true);
        GlobalPosition = currentMoveTarget;
    }

    private void AttackDefault()
    {
        if (GlobalPosition.DistanceTo(PlayerNode.GlobalPosition) > PunchEngageRange * 1.5)
        {
            TransitionState(EvaAIState.Laser);
        }
        else
        {
            TransitionState(EvaAIState.Punch);
        }
    }

    private void TransitionState(EvaAIState newState)
    {
        GD.Print("Switch State:" + newState.ToString());


        AnimationNodeStateMachinePlayback stateMachine;
        rng.Randomize();
        switch (newState)
        {
            case EvaAIState.Dead:
                evaShader.EmissionEnergyMultiplier = ShaderEmissionMaxOnHit;
                SpawnEffect(DamagedEffect, EvaHead.GlobalPosition, EvaHead.GlobalRotation);

                stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
                stateMachine.Travel("DeathBegin");
                break;
            case EvaAIState.Reposition:
                TimerNode.Start(RepositionDuration);
                break;
            case EvaAIState.Laser:
                stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
                stateMachine.Travel("LaserAttack");
                break;
            case EvaAIState.Punch:
                stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
                stateMachine.Travel("GroundPound");
                break;
            case EvaAIState.Beserk:
                shortTimer = 1;
                evaShader.EmissionEnergyMultiplier = ShaderEmissionMaxOnHit;
                stateMachine = (AnimationNodeStateMachinePlayback)AnimationTree.Get("parameters/playback");
                stateMachine.Travel("BeserkLaser");
                TimerNode.Start(rng.RandfRange(TeleportTimeRange.x, TeleportTimeRange.y));
                break;
            case EvaAIState.Damaged:
                evaShader.EmissionEnergyMultiplier = ShaderEmissionMaxOnHit;
                SpawnEffect(DamagedEffect, EvaHead.GlobalPosition, EvaHead.GlobalRotation);

                TimerNode.Start(rng.RandfRange(TeleportTimeRange.x, TeleportTimeRange.y));
                break;
        }

        evaAIState = newState;
    }

    public void OnDamage(Vector3 targetPosition, float damageRadius)
    {
        if (evaAIState == EvaAIState.Dead || evaAIState == EvaAIState.Damaged || evaAIState == EvaAIState.Beserk)
        {
            return;
        }

        if (GlobalPosition.DistanceTo(targetPosition) - damageRadius - ColliderRadius <= 0)
        {
            if (EasyMode)
            {
                CurrentHealth -= EasyModeRecievedDamageAmount;
            }
            else
            {
                CurrentHealth -= RecievedDamageAmount;
            }

            GetTree().CallGroup("Player", "UpdateEvaHealth", CurrentHealth / TotalHealth);

            if (CurrentHealth <= 0)
            {
                TransitionState(EvaAIState.Dead);
            }
            else
            {
                TransitionState(EvaAIState.Damaged);
            }
        }
    }

    public void AnimationEvent(string EventName)
    {
        if (evaAIState == EvaAIState.Dead)
        {
            return;
        }

        GD.Print("Animation Event:" + EventName);

        switch (EventName)
        {
            case "Complete":
                timerIsComplete = true;
                break;
            case "Punch":
                float distanceToPlayer = PlayerNode.GlobalPosition.DistanceTo(GlobalPosition);
                
                Vector3 normalizedPosition = (PlayerNode.GlobalPosition - GlobalPosition).Normalized();
                normalizedPosition.y = 0;

                GlobalPosition += normalizedPosition * Mathf.Min(PunchJumpDistance, distanceToPlayer); //moves towards player.
                SpawnEffect(PunchEffect, GlobalPosition, GlobalRotation);
                break;
            case "Laser":
                SpawnEffect(LaserEffect, EvaHead.GlobalPosition, EvaHead.GlobalRotation);
                break;
            default:
                GD.Print("Not handled.");
                break;
        }
    }

    public void SpawnEffect(PackedScene effect, Vector3 position, Vector3 rotation)
    {
        if (effect == null || evaAIState == EvaAIState.Dead)
        {
            return;
        }

        Node3D newEffect = effect.Instantiate() as Node3D;

        EffectContainerNode.AddChild(newEffect);
        newEffect.GlobalPosition = position;
        newEffect.Rotation = rotation;
    }

    public void TimerComplete()
    {
        //TimerNode.Stop();
        GD.Print("TimerComplete");
        timerIsComplete = true;
    }

    public void OnSettingsUpdated()
    {
        EasyMode = SceneDataStatic.EasyDifficulty;
    }
}
