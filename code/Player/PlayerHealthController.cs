using Godot;
using System;

public partial class PlayerHealthController : Node3D
{
    [Export] public ProgressBar HealthBarA;
    [Export] public ProgressBar HealthBarB;

    [Export] public Resource CloudMatRef;
    [Export] public Resource CloudCollider;

	[Export] public float MaxHealth = 1000;
	[Export] public float RegenerationDelay = 3;
    [Export] public float RegenerationRate = 100;

    [Export] public float ShieldMultiplier = 0.2f;
    [Export] public float EasyMultiplier = 0.5f;

    [Export] public float ColorFlashReturnTime = 0.2f;

    [Export] public Gradient DamageColor;
    [Export] public Color DeathColor;

    private float currentHealth;
	private float currentRegenerationDelay;

	private float currentFlashTime = 0;

	private Color flashedColor;
	private Color returnColor;
    private ShaderMaterial cloudMat;

    private SphereShape3D sphereShape;

    private bool SentDeath = false;
    private bool currentShield = false;

    private bool EasyMode;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
		currentHealth = MaxHealth;
        cloudMat = CloudMatRef as ShaderMaterial;
        SetHealthBars();

        sphereShape = CloudCollider as SphereShape3D;

        OnSettingsUpdated();
    }

	

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
        if (currentHealth == 0)
        {
            if (currentFlashTime > 0)
            {
                currentFlashTime -= (float)delta;
                cloudMat.SetShaderParameter("Emission", returnColor.Lerp(flashedColor, currentFlashTime / ColorFlashReturnTime));
                cloudMat.SetShaderParameter("Albedo", returnColor.Lerp(flashedColor, currentFlashTime / ColorFlashReturnTime));
                if (currentFlashTime < 1 && !SentDeath) //let transition start.
                {
                    GetTree().CallGroup("Death", "_on_death");
                }

                if (currentFlashTime <= 0)
                {
                    cloudMat.SetShaderParameter("Emission", returnColor);
                    cloudMat.SetShaderParameter("Albedo", returnColor);
                    GetTree().CallGroup("PlayerDeathComplete", "DeathComplete");
                }
            }
        }
        else 
        { 

            if (currentRegenerationDelay > 0)
            {
                currentRegenerationDelay -= (float)delta;
            }
            else if (currentHealth < MaxHealth)
            {
                currentHealth = Mathf.Min(currentHealth + ((float)delta * RegenerationRate), MaxHealth);
                SetHealthBars();
            }

            if (currentFlashTime > 0)
            {
                currentFlashTime -= (float)delta;
                cloudMat.SetShaderParameter("Emission", returnColor.Lerp(flashedColor, currentFlashTime / ColorFlashReturnTime));
                if (currentFlashTime <= 0)
                {
                    cloudMat.SetShaderParameter("Emission", returnColor);
                    GetTree().CallGroup("Player", "SetColorLock", false);
                }
            }
        }
    }

    public void OnShieldChange(bool newShield) 
    {
        currentShield = newShield;
    }

	public void OnDamage(Vector3 damageOrigin, float damageColRadius, float Damage)
	{
        if (currentShield)
        {
            Damage = Damage * ShieldMultiplier;
        }

        if (EasyMode)
        {
            Damage = Damage * EasyMultiplier;
        }

        if (currentHealth != 0 && damageOrigin.DistanceTo(GlobalPosition) < sphereShape.Radius + damageColRadius)
        {
            currentHealth = Mathf.Max(currentHealth - Damage, 0);
            if (currentHealth == 0)
            {
                flashedColor = cloudMat.GetShaderParameter("Emission").AsColor();
                returnColor = DeathColor;
                currentFlashTime = ColorFlashReturnTime * 3;
                GetTree().CallGroup("Death", "BeginDeath");
                return;
            }

            SetHealthBars();
            if (currentShield)
            {
                currentRegenerationDelay = RegenerationDelay * ShieldMultiplier;
            }
            else
            {
                currentRegenerationDelay = RegenerationDelay;
            }

            GetTree().CallGroup("Player", "SetColorLock", true);

            if (currentFlashTime <= 0)
            {
                returnColor = cloudMat.GetShaderParameter("Emission").AsColor();
            }

            flashedColor = DamageColor.Sample(Mathf.Clamp(Damage / currentHealth, 0, 1));
            cloudMat.SetShaderParameter("Emission", flashedColor); //Color is based on percentage of remaining health.

            currentFlashTime = ColorFlashReturnTime;
        }
    }

    private void SetHealthBars()
    {
        HealthBarA.Value = (currentHealth / MaxHealth) * 100;
        HealthBarB.Value = HealthBarA.Value;
    }

    public void UpdateAssemblyValue(float curCompletion)
    {
        GD.Print("Current completionPercentage :" + curCompletion);
    }

    public void OnSettingsUpdated()
    {
        EasyMode = SceneDataStatic.EasyDifficulty;
    }
}
