using Godot;
using Godot.Collections;
public static class PhysicsExtensions 
{
    /// <returns>Returns all colliders within Sphere.</returns>
    public static Array<Dictionary> OverlapSphere3D(PhysicsDirectSpaceState3D worldSpace, float radius, Vector3 position, bool CollideWithAreas, bool CollideWithBodies, uint collisionLayer, int maxResults = 0)
    {
        SphereShape3D shape = new SphereShape3D
        {
            Radius = radius
        };

        PhysicsShapeQueryParameters3D query = new()
        {
            Shape = shape,
            Transform = new Transform3D(new Basis(), position),
            CollideWithAreas = CollideWithAreas,
            CollideWithBodies = CollideWithBodies,
            CollisionMask = collisionLayer
        };

        return FireOverlap3DCast(worldSpace, query, maxResults);
    }

    /// <returns>Returns all colliders within Sphere.</returns>
    public static Array<Dictionary> OverlapBox3D(PhysicsDirectSpaceState3D worldSpace, Vector3 size, Vector3 position, bool CollideWithAreas, bool CollideWithBodies, uint collisionLayer, int maxResults = 0)
    {
        BoxShape3D shape = new BoxShape3D
        {
            Size = size
        };

        PhysicsShapeQueryParameters3D query = new()
        {
            Shape = shape,
            Transform = new Transform3D(new Basis(), position),
            CollideWithAreas = CollideWithAreas,
            CollideWithBodies = CollideWithBodies,
            CollisionMask = collisionLayer
        };

        return FireOverlap3DCast(worldSpace, query, maxResults);
    }

    /// <returns>Returns all colliders within Sphere.</returns>
    public static Array<Dictionary> OverlapBox3D(PhysicsDirectSpaceState3D worldSpace, Vector3 size, Vector3 position, Basis basis, bool CollideWithAreas, bool CollideWithBodies, uint collisionLayer, int maxResults = 0)
    {
        BoxShape3D shape = new BoxShape3D
        {
            Size = size
        };

        PhysicsShapeQueryParameters3D query = new()
        {
            Shape = shape,
            Transform = new Transform3D(basis, position),
            CollideWithAreas = CollideWithAreas,
            CollideWithBodies = CollideWithBodies,
            CollisionMask = collisionLayer
        };

        return FireOverlap3DCast(worldSpace, query, maxResults);
    }

    private static Array<Dictionary> FireOverlap3DCast(PhysicsDirectSpaceState3D worldSpace, PhysicsShapeQueryParameters3D query, int maxResults)
    {
        if (maxResults != 0)
        {
            return worldSpace.IntersectShape(query, maxResults);
        }
        else
        {
            return worldSpace.IntersectShape(query);
        }
    }

    public struct RaycastHit
    {
        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public Object Collider { get; set; }
    }
    public static bool RayCast(out RaycastHit hit, PhysicsDirectSpaceState3D worldSpace, Vector3 fromvec, Vector3 tovec, bool CollideWithAreas, bool CollideWithBodies, uint collisionLayer)
    {
        PhysicsRayQueryParameters3D query = new()
        {
            From = fromvec,
            To = tovec,
            CollideWithAreas = CollideWithAreas,
            CollideWithBodies = CollideWithBodies,
            CollisionMask = collisionLayer
        };
        var hitDictionary = worldSpace.IntersectRay(query);

        if (hitDictionary.Count > 0)
        {
            hit = new RaycastHit()
            {
                Point = (Vector3)hitDictionary["position"],
                Normal = (Vector3)hitDictionary["normal"],
                Collider = (Object)hitDictionary["collider"]
            };
            return true;
        }
        else
        {
            hit = new RaycastHit();
            return false;
        }
    }
}
