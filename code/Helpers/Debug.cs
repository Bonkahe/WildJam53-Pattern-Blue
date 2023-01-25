using Godot;
using System;

public static class Debug
{
    public static MeshInstance3D DrawLine(Node reference, Vector3 pos1, Vector3 pos2, Color color)
    {
        MeshInstance3D mesh_instance = new();

        ImmediateMesh immediate_mesh = new();

        ORMMaterial3D material = new();

        mesh_instance.Mesh = immediate_mesh;
        mesh_instance.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;


        immediate_mesh.SurfaceBegin(Mesh.PrimitiveType.Lines, material);
        immediate_mesh.SurfaceAddVertex(pos1);
        immediate_mesh.SurfaceAddVertex(pos2);
        immediate_mesh.SurfaceEnd();

        material.ShadingMode = BaseMaterial3D.ShadingModeEnum.Unshaded;

        material.AlbedoColor = color;


        reference.GetTree().Root.GetChild(0).AddChild(mesh_instance);

        return mesh_instance;
    }
}
