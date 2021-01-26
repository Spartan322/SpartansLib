using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

namespace SpartansLib.Extensions
{
    public class Physics2DSpaceQueryResult : Resource
    {
        public CollisionObject2D CollidingObject;
        public int ColliderId;
        public object Metadata;
        public RID IntersectingRid;
        public int ShapeIndex;
        internal Physics2DSpaceQueryResult(Dictionary dict)
        {
            CollidingObject = (CollisionObject2D)dict["collider"];
            ColliderId = (int)dict["collider_id"];
            Metadata = dict["metadata"];
            IntersectingRid = (RID)dict["rid"];
            ShapeIndex = (int)dict["shape"];
        }
    }

    public class Physics2DSpaceQueryRayResult : Physics2DSpaceQueryResult
    {
        public Vector2 Position;
        public Vector2 Normal;
        internal Physics2DSpaceQueryRayResult(Dictionary dict) : base(dict)
        {
            Position = (Vector2)dict["position"];
            Normal = (Vector2)dict["normal"];
        }
    }

    public static class PhysicsDirectSpaceStateExtensions
    {
        public static (float? furthestMove, float? collisionPoint) CastMotionTuple(
            this Physics2DDirectSpaceState state,
            Physics2DShapeQueryParameters param)
        {
            var motion = state.CastMotion(param);
            if (motion.Count == 0) return (null, null);
            return ((float)motion[0], (float)motion[1]);
        }

        public static Array<Vector2> CollideShapeGeneric(
            this Physics2DDirectSpaceState state,
            Physics2DShapeQueryParameters param,
            int maxResults = 32)
            => new Array<Vector2>(state.CollideShape(param, maxResults));

        public static Array<Physics2DSpaceQueryResult> IntersectPointStructured(
            this Physics2DDirectSpaceState state,
            Vector2 point,
            int maxResults = 32,
            Godot.Collections.Array exclude = null,
            uint collisionLayer = 2147483647,
            bool bodyCollide = true,
            bool areaCollide = false
        )
        {
            var intersect = state.IntersectPoint(point, maxResults, exclude, collisionLayer, bodyCollide, areaCollide);
            var result = new Array<Physics2DSpaceQueryResult>();
            result.Resize(intersect.Count);
            for (int i = 0; i < intersect.Count; i++)
                result[i] = new Physics2DSpaceQueryResult((Dictionary)intersect[i]);
            return result;
        }

        public static Physics2DSpaceQueryRayResult IntersectPointStructured(
            this Physics2DDirectSpaceState state,
            Vector2 from,
            Vector2 to,
            Godot.Collections.Array exclude = null,
            uint collision_layer = 2147483647,
            bool collide_with_bodies = true,
            bool collide_with_areas = false
        )
            => new Physics2DSpaceQueryRayResult(state.IntersectRay(from, to, exclude, collision_layer, collide_with_bodies, collide_with_areas));
    }
}
