// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "AvoidCollisionRule", menuName = "Boids/AvoidCollisionRule", order = 1)]
    public class AvoidCollisionRule : BoidRule
    {
        /// <summary>
        /// Maximum collision detection distance.
        /// </summary>
        public float maxRadius = 1.0f;

        /// <summary>
        /// Minimum distance allowed between particles.
        /// </summary>
        public float minRadius = 0.1f;

        /// <summary>
        /// Maximum number of iterations per step to try and find a non-colliding direction.
        /// </summary>
        public int maxIterations = 5;

        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            boid.GetDebug(out var dbg);
            if (dbg != null)
            {
                dbg.ClearCollision();
            }

            float deltaRadius = maxRadius - minRadius;
            if (deltaRadius <= 0.0f)
            {
                target = null;
                priority = PriorityNone;
                return false;
            }

            queryResults.Clear();
            context.Query.Radius(context.Tree, state.position, maxRadius, queryResults);

            Vector3 dir = state.direction;
            float maxweight = 0.0f;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    Debug.Assert(context.Tree.Points[idx] == state.position);
                    continue;
                }

                Vector3 colliderPos = context.Tree.Points[idx];
                Vector3 colliderDir = colliderPos - state.position;

                if (GetInsidePositiveConeProjection(dir, colliderDir, minRadius, out Vector3 newDir, dbg))
                {
                    dir = newDir;
                    // TODO find useful metric for correction weight
                    maxweight = 1.0f;
                }
            }
            if (maxweight > 0.0f)
            {
                target = new BoidTarget(dir, state.velocity.magnitude);
                priority = PriorityHigh;
                return true;
            }
            else
            {
                target = null;
                priority = PriorityNone;
                return false;
            }
        }

        /// <summary>
        /// Project vectors inside the positive cone onto it, leave vectors outside the cone unchanged.
        /// </summary>
        private static bool GetInsidePositiveConeProjection(Vector3 dir, Vector3 coneDir, float radius, out Vector3 result, BoidParticleDebug dbg)
        {
            float sqrRadius = radius * radius;
            Vector3 normConeDir = coneDir.normalized;

            // Distance to the closest valid point on the sphere
            float sqrConeSide = coneDir.sqrMagnitude - sqrRadius;
            // Special case: inside the sphere, move straight away from the collider
            if (sqrConeSide <= 0.0f)
            {
                result = -normConeDir * (radius - coneDir.magnitude);
                return true;
            }

            // Length of dir vector in direction of the cone
            float coneDot = Vector3.Dot(dir, normConeDir);
            // Negative cone case, ignore
            if (coneDot <= 0.0f)
            {
                result = dir;
                return false;
            }

            // Split
            Vector3 conePart = coneDot * normConeDir;
            Vector3 orthoPart = dir - conePart;
            Debug.Assert(Mathf.Abs(Vector3.Dot(conePart, orthoPart)) < 0.01f);

            float sqrOrtho = orthoPart.sqrMagnitude;
            if (sqrOrtho > 0.0f)
            {
                float sqrTanConeAngle = sqrRadius / (coneDir.sqrMagnitude - sqrRadius);

                float sqrCone = conePart.sqrMagnitude;
                float sqrScale = sqrCone / sqrOrtho * sqrTanConeAngle;
                // scale >= 1 means the vector is outside the cone
                if (sqrScale < 1.0f)
                {
                    result = conePart + orthoPart * Mathf.Sqrt(sqrScale);
                    if (dbg != null)
                    {
                        dbg.AddCollisionCone(conePart, orthoPart, coneDir, radius);
                    }
                    return true;
                }
                else
                {
                    result = dir;
                    return false;
                }
            }
            result = dir;
            return false;
        }
    }
}
