// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "AvoidBoidCollisionRule", menuName = "Boids/AvoidBoidCollisionRule", order = 1)]
    public class AvoidBoidCollisionRule : BoidRule
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
            bool hasCorrection = false;
            float weight = 0.0f;
            for (int iter = 0; iter < maxIterations; ++iter)
            {
                int numCollisions = 0;
                float distance = 0.0f;
                float maxDistance = 0.0f;
                Vector3 gradient = Vector3.zero;
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

                    if (GetInsidePositiveConeDistance(dir, colliderDir, minRadius, out float coneDistance, out Vector3 coneGradient, dbg))
                    {
                        // TODO find useful metric for correction weight
                        ++numCollisions;
                        distance += coneDistance;
                        maxDistance = Mathf.Max(maxDistance, coneDistance);
                        gradient += coneGradient;
                    }
                }
                if (numCollisions > 0)
                {
                    hasCorrection = true;
                    dir -= gradient * distance;
                    weight = maxDistance;
                }
                else
                {
                    break;
                }
            }

            if (hasCorrection)
            {
                target = new BoidTarget(dir, state.velocity.magnitude);
                priority = PriorityHigh * weight;
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
        /// Compute a distance field inside the positive cone, including gradient.
        /// </summary>
        private static bool GetInsidePositiveConeDistance(Vector3 dir, Vector3 coneDir, float coneRadius, out float distance, out Vector3 gradient, BoidParticleDebug dbg)
        {
            float sqrRadius = coneRadius * coneRadius;
            Vector3 normConeDir = coneDir.normalized;

            // Length to the cone side
            float sqrConeSide = coneDir.sqrMagnitude - sqrRadius;
            // Special case: inside the sphere
            if (sqrConeSide <= 0.0f)
            {
                float coneDistance = coneDir.magnitude;
                distance = 1.0f - coneDistance / coneRadius;
                gradient = normConeDir;
                return true;
            }

            // Length of dir vector in direction of the cone
            float coneDot = Vector3.Dot(dir, normConeDir);
            // Negative cone case, ignore
            if (coneDot <= 0.0f)
            {
                distance = 0.0f;
                gradient = Vector3.zero;
                return false;
            }

            // Split
            Vector3 conePart = coneDot * normConeDir;
            Vector3 orthoPart = dir - conePart;
            Debug.Assert(Mathf.Abs(Vector3.Dot(conePart, orthoPart)) < 0.01f);

            float sqrOrtho = orthoPart.sqrMagnitude;
            float sqrCone = conePart.sqrMagnitude;
            if (sqrOrtho > 0.0f && sqrCone > 0.0f)
            {
                float sqrTanConeAngle = sqrRadius / (coneDir.sqrMagnitude - sqrRadius);
                float sqrOrthoNew = sqrCone * sqrTanConeAngle;
                float sqrScale = sqrOrtho / sqrOrthoNew;
                if (sqrScale < 1.0f)
                {
                    // scale < 1: vector is inside the cone
                    float scale = Mathf.Sqrt(sqrScale);
                    distance = 1.0f - scale;
                    gradient = -orthoPart.normalized;
                    if (dbg != null)
                    {
                        dbg.AddCollisionCone(dir, coneDir, coneRadius);
                    }
                    return true;
                }
                else
                {
                    // scale >= 1: vector is outside the cone
                    distance = 0.0f;
                    gradient = Vector3.zero;
                    return false;
                }
            }
            else
            {
                /// Unlikely corner case: direction coincides exactly with cone direction, no gradient
                distance = 1.0f;
                gradient = Vector3.zero;
                return false;
            }
        }
    }
}
