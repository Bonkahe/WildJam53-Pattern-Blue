using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class RainControl : Node3D
{
    [Export] public AnimationTree IconAnimationTree;

    [Export] public Node3D WaterIconPickup;
    [Export] public Node3D OceanIconPickup;
    [Export] public Node3D SnowIconPickup;

    [Export] public Node3D WaterIconHeld;
    [Export] public Node3D OceanIconHeld;
    [Export] public Node3D SnowIconHeld;

    [Export] public Node3D PickupBlocked;

    [Export] public float PickupCooldown = 2;
    [Export] public float EffectRadius = 8;

	[Export] public float LerpDuration = 0.5f;
	[Export] public string AnimationPathInflated;

    [Export] public Resource CloudMatRef;
    [Export] public Resource VolumetricMatRef;

    [Export] public Array<Resource> InteractionDefinitions;
    [Export] public NodePath AnimationTreePath;
    [Export] public NodePath GridPath;
    [Export] public NodePath OceanGridPath;

    [Export] public NodePath EffectContainerPath;

	private AnimationTree animationTree;
	private GridMap grid;
    private GridMap oceanGrid;

    private Color baseCloudColor;
    private Color baseCloudEmission;
    private ShaderMaterial cloudMat;

    private Color baseVolColor;
    private Color baseVolEmission;
    private FogMaterial volumetricMat;

    private List<InteractionDefinition> interactionDefinitions;

	private InteractionDefinition currentlyInteracted;

    private Node effectContainer;

    private Color currentOriginColor;
	private float currentColorLerp;
    private float currentPickupCooldown;
    private bool lockColor = false;

    private bool isDead = false;
    private bool HasEffectFalling = false;
    private bool StopEffect = false;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        animationTree = GetNode<AnimationTree>(AnimationTreePath);
        grid = GetNode<GridMap>(GridPath);
        oceanGrid = GetNode<GridMap>(OceanGridPath);

        cloudMat = CloudMatRef as ShaderMaterial;
        baseCloudColor = cloudMat.GetShaderParameter("Albedo").AsColor();
        baseCloudEmission = cloudMat.GetShaderParameter("Emission").AsColor();

        volumetricMat = VolumetricMatRef as FogMaterial;
        baseVolColor = volumetricMat.Albedo;
		baseVolEmission = volumetricMat.Emission;

        effectContainer = GetNode<Node>(EffectContainerPath);

        //volumetrics = VolumetricsPaths.Select(x => GetNode<FogVolume>(x)).ToList();
        //foreach (var item in volumetrics)
        //{
        //	if (item.Material is ShaderMaterial shaderMat)
        //	{

        //	}
        //}
        //volumetricsBaseColors = volumetrics.Select(x => x.Material is ShaderMaterial newmat ? newmat.)


        interactionDefinitions = InteractionDefinitions.Select(x => x as InteractionDefinition).ToList();
    }

    public void SetColorLock(bool newLockColor)
    {
        lockColor = newLockColor;
    }

	public override void _Input(InputEvent inputEvent)
	{
        if (isDead) return;

		if (inputEvent.IsActionPressed("interact"))
		{
			if (currentlyInteracted == null)
			{
                if (currentPickupCooldown > 0)
                {
                    AnimationNodeStateMachinePlayback stateMachine = (AnimationNodeStateMachinePlayback)IconAnimationTree.Get("parameters/playback");
                    stateMachine.Travel("Wobble");
                }
                else
                {
                    InteractionDefinition newInteracted = GetDefinitionAtPos();

                    if (newInteracted != null)
                    {
                        currentlyInteracted = newInteracted;

                        animationTree.Set(AnimationPathInflated, true);
                        
                        if (currentlyInteracted.InteractionName == "FreshWaterBlock")
                        {
                            GetTree().CallGroup("Player", "OnShieldChange", true);
                        }
                    }
                }
            }
        }
		if (inputEvent.IsActionPressed("release"))
		{
			if (currentlyInteracted != null)
			{
                if (currentlyInteracted.EffectScene != null)
                {
                    GetTree().CallGroup("Player", "SpeedBoost");

                    if (HasEffectFalling)
                    {
                        StopEffect = true;
                    }
                    CallDeferred("DropEffect", currentlyInteracted.EffectScene, currentlyInteracted.EffectSpawnCount, currentlyInteracted.EffectSpawnInterval, currentlyInteracted.InteractionName == "OceanBlock", 1);

                }
                currentOriginColor = currentlyInteracted.Color;
                currentlyInteracted = null;
                currentPickupCooldown = PickupCooldown;

                GetTree().CallGroup("Player", "OnShieldChange", false);
            }
        }
	}

    private InteractionDefinition GetDefinitionAtPos()
    {
        Vector3 localPos = grid.ToLocal(GlobalPosition);
        localPos.y = 0;
        string currentTile = "OceanBlock";
        int id = grid.GetCellItem(grid.LocalToMap(localPos));
        if (id != -1)
        {
            currentTile = grid.MeshLibrary.GetItemName(id);
        }

        //Handle off land and seawall blocks
        if (currentTile == "OceanBlock" || currentTile == "SeaWall")
        {
            id = oceanGrid.GetCellItem(oceanGrid.LocalToMap(localPos));

            if (id != -1)
            {
                currentTile = oceanGrid.MeshLibrary.GetItemName(id);
            }
        }

        if (currentTile == "freshwaterPlaceholder")
        {
            currentTile = "FreshWaterBlock";
        }

        return interactionDefinitions.Where(x => x.InteractionName == currentTile).FirstOrDefault();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (isDead) return;

        if (currentPickupCooldown > 0)
        {
            WaterIconHeld.Visible = false;
            OceanIconHeld.Visible = false;
            SnowIconHeld.Visible = false;
            WaterIconPickup.Visible = false;
            OceanIconPickup.Visible = false;
            SnowIconPickup.Visible = false;

            PickupBlocked.Visible = true;

        }
        else
        {
            PickupBlocked.Visible = false;

            if (currentlyInteracted == null)
            {
                WaterIconHeld.Visible = false;
                OceanIconHeld.Visible = false;
                SnowIconHeld.Visible = false;
                InteractionDefinition newInteracted = GetDefinitionAtPos();
                if (newInteracted != null)
                {
                    switch (newInteracted.InteractionName)
                    {
                        case "OceanBlock":
                            WaterIconPickup.Visible = false;
                            OceanIconPickup.Visible = true;
                            SnowIconPickup.Visible = false;
                            break;
                        case "FreshWaterBlock":
                            WaterIconPickup.Visible = true;
                            OceanIconPickup.Visible = false;
                            SnowIconPickup.Visible = false;
                            break;
                        case "MountainsBlock":
                            WaterIconPickup.Visible = false;
                            OceanIconPickup.Visible = false;
                            SnowIconPickup.Visible = true;
                            break;
                    }
                }
                else
                {
                    WaterIconPickup.Visible = false;
                    OceanIconPickup.Visible = false;
                    SnowIconPickup.Visible = false;
                }
            }
            else
            {
                WaterIconPickup.Visible = false;
                OceanIconPickup.Visible = false;
                SnowIconPickup.Visible = false;
                switch (currentlyInteracted.InteractionName)
                {
                    case "OceanBlock":
                        WaterIconHeld.Visible = false;
                        OceanIconHeld.Visible = true;
                        SnowIconHeld.Visible = false;
                        break;
                    case "FreshWaterBlock":
                        WaterIconHeld.Visible = true;
                        OceanIconHeld.Visible = false;
                        SnowIconHeld.Visible = false;
                        break;
                    case "MountainsBlock":
                        WaterIconHeld.Visible = false;
                        OceanIconHeld.Visible = false;
                        SnowIconHeld.Visible = true;
                        break;
                }
            }
        }
    }



    public override void _Process(double delta)
	{
        if (isDead) return;

        if (currentPickupCooldown > 0)
        {
            currentPickupCooldown -= (float)delta;
        }

        if (!lockColor)
        {
            if (currentlyInteracted != null)
            {
                if (currentColorLerp < LerpDuration)
                {
                    currentColorLerp = Mathf.Min(currentColorLerp + (float)delta, LerpDuration);

                    volumetricMat.Albedo = baseVolColor.Lerp(currentlyInteracted.Color, currentColorLerp / LerpDuration);
                    volumetricMat.Emission = baseVolEmission.Lerp(currentlyInteracted.Color, currentColorLerp / LerpDuration);
                    cloudMat.SetShaderParameter("Albedo", baseCloudColor.Lerp(currentlyInteracted.Color, currentColorLerp / LerpDuration));
                    cloudMat.SetShaderParameter("Emission", baseCloudEmission.Lerp(currentlyInteracted.Color, currentColorLerp / LerpDuration));
                }
            }
            else
            {
                if (currentColorLerp > 0)
                {
                    currentColorLerp = Mathf.Max(currentColorLerp - (float)delta, 0);

                    volumetricMat.Albedo = baseVolColor.Lerp(currentOriginColor, currentColorLerp / LerpDuration);
                    volumetricMat.Emission = baseVolEmission.Lerp(currentOriginColor, currentColorLerp / LerpDuration);
                    cloudMat.SetShaderParameter("Albedo", baseCloudColor.Lerp(currentOriginColor, currentColorLerp / LerpDuration));
                    cloudMat.SetShaderParameter("Emission", baseCloudEmission.Lerp(currentOriginColor, currentColorLerp / LerpDuration));
                }
            }

            animationTree.Set(AnimationPathInflated, Mathf.Clamp(currentColorLerp / LerpDuration, 0, 1));
        }
    }

    private async void DropEffect(PackedScene instanceScene, int count, float interval, bool causeDamage, float minDistance = 1)
    {
        //waits for the last one to stop.
        while (HasEffectFalling)
        {
            await Task.Delay((int)(interval * 1000));
        }
        HasEffectFalling = true;
        Vector3 lastPosition = GlobalPosition;
        bool isFirst = true;
        //GD.Print(count);
        while (count > 0)
        {
            if (StopEffect == true)
            {
                GD.Print("Stopped effect");
                StopEffect = false;//clears up for the next one.
                HasEffectFalling = false;
                break;
            }

            count--;

            if (isFirst || lastPosition.DistanceTo(GlobalPosition) > minDistance)
            {
                //GD.Print("iterate");
                isFirst = false;
                Node3D resultingEffect = instanceScene.Instantiate() as Node3D;
                effectContainer.AddChild(resultingEffect);
                resultingEffect.GlobalPosition = new Vector3(GlobalPosition.x, 0, GlobalPosition.z);
                if (causeDamage)
                {
                    //CheckDamage
                    GetTree().CallGroup("Enemies", "OnDamage", resultingEffect.GlobalPosition, EffectRadius);
                    GetTree().CallGroup("EvaEnemy", "OnDamage", resultingEffect.GlobalPosition, EffectRadius);
                }
            }
            await Task.Delay((int)(interval * 1000));
        }
        HasEffectFalling = false;
    }

    public void BeginDeath()
    {
        isDead = true;
    }

    public void DeathComplete()
    {
        volumetricMat.Albedo = baseVolColor;
        volumetricMat.Emission = baseVolEmission;
        cloudMat.SetShaderParameter("Albedo", baseCloudColor);
        cloudMat.SetShaderParameter("Emission", baseCloudEmission);
    }
}
