using Godot;
using System;

public partial class InteractionDefinition : Resource
{
    [Export] public string InteractionName { get; set; }
    [Export] public Color Color { get; set; }
    [Export] public PackedScene EffectScene { get; set; }
    [Export] public int EffectSpawnCount { get; set; } = 3;
    [Export] public float EffectSpawnInterval { get; set; } = 0.5f;
}
