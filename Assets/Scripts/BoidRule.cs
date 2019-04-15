// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    // Result of evaluating a boid rule for a given particle
    public class BoidTarget : IEquatable<BoidTarget>
    {
        public Vector3 direction;
        public float speed;

        public Vector3 GetVelocity()
        {
            return direction.normalized * speed;
        }

        public bool Equals(BoidTarget other)
        {
            return direction == other.direction && speed == other.speed;
        }

        public BoidTarget(Vector3 _direction, float _speed)
        {
            direction = _direction;
            speed = _speed;
        }
    }

    public class BoidContext
    {
        private const int maxPointsPerLeafNode = 32;

        private KDTree tree;
        public KDTree Tree => tree;
        private KDQuery query;
        public KDQuery Query => query;

        public BoidContext()
        {
            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();
        }

        public void Prepare(List<BoidParticle> boids)
        {
            tree.SetCount(boids.Count);
            for (int i = 0; i < boids.Count; ++i)
            {
                BoidState state = boids[i].GetState();
                tree.Points[i] = state.position;
            }
            tree.Rebuild();
        }

        public void Cleanup()
        {
        }
    }

    public class BoidRule : ScriptableObject
    {
        public const float PriorityNone = 0.0f;
        public const float PriorityLow = 1.0f;
        public const float PriorityMedium = 2.0f;
        public const float PriorityHigh = 3.0f;
        public const float PriorityCritical = 4.0f;

        public virtual void OnAwake()
        {
        }

        public virtual void OnDestroy()
        {
        }

        public virtual void Prepare()
        {
        }

        public virtual void Cleanup()
        {
        }

        public virtual bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            target = null;
            priority = 0.0f;
            return false;
        }
    }

    [CreateAssetMenu(fileName = "SimpleCircleRule", menuName = "Boids/SimpleCircleRule", order = 1)]
    public class SimpleCircleRule : BoidRule
    {
        public float radius = 1.0f;
        public Vector3 center = Vector3.zero;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            Vector3 localPos = state.position - center;
            Vector3 goal = new Vector3(localPos.x, 0.0f, localPos.z);
            goal = goal.normalized * radius + center;

            target = new BoidTarget(goal - state.position, boid.Settings.MaxSpeed);
            priority = PriorityLow;
            return true;
        }
    }

    [CreateAssetMenu(fileName = "SwarmRule", menuName = "Boids/SwarmRule", order = 1)]
    public class SwarmRule : BoidRule
    {
        /// <summary>
        /// Maximum distance of other boids to influence the swarm searching behavior.
        /// </summary>
        public float maxRadius = 1.0f;

        /// <summary>
        /// Minimum distance to activate swarm behavior.
        /// </summary>
        public float minRadius = 0.1f;

        /// <summary>
        /// Weight preference of boids in the front vs. boids in the back
        /// </summary>
        [Range(0.0f, 1.0f)]
        public float forwardAsymmetry = 0.5f;

        private readonly List<int> queryResults = new List<int>();

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            boid.GetDebug(out var dbg);
            if (dbg != null)
            {
                dbg.ClearSwarm();
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

            float totweight = 0.0f;
            Vector3 goal = Vector3.zero;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                Vector3 p = context.Tree.Points[idx];
                Vector3 d = p - state.position;
                float dist = d.magnitude;

                // Ignore results inside the min. radius
                if (dist < minRadius)
                {
                    continue;
                }

                float weight = 1.0f - (dist - minRadius) / deltaRadius;
                if (forwardAsymmetry > 0.0f)
                {
                    float fwd = Vector3.Dot(d.normalized, state.direction);
                    float fwdFactor = 0.5f + 0.5f * fwd;
                    weight *= 1.0f - (1.0f - fwdFactor) * forwardAsymmetry;
                }

                if (dbg != null)
                {
                    dbg.AddSwarmPoint(p, weight);
                }

                goal += p * weight;
                totweight += weight;
            }
            if (totweight > 0.0f)
            {
                goal /= totweight;
                target = new BoidTarget(goal - state.position, boid.Settings.MaxAcceleration);
                priority = PriorityMedium;
                return true;
            }
            else
            {
                target = null;
                priority = PriorityNone;
                return false;
            }
        }
    }

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

            if (dbg != null)
            {
                float mag = dir.magnitude;
                float radiusFactor = mag > 0.0f ? coneDot / mag : 0.0f;
                // float radiusFactor = mag > 0.0f ? mag : 0.0f;
                dbg.AddCollisionCone(Vector3.one*0.0001f, Vector3.one*0.0001f, coneDir, radius * radiusFactor);
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

    /// <summary>
    /// Rule to keep moving in the current direction at minimum speed.
    /// </summary>
    [CreateAssetMenu(fileName = "KeepMovingRule", menuName = "Boids/KeepMovingRule", order = 1)]
    public class KeepMovingRule : BoidRule
    {
        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            if (state.direction == Vector3.zero)
            {
                target = null;
                priority = PriorityNone;
                return false;
            }

            target = new BoidTarget(state.direction, boid.Settings.MinSpeed);
            priority = PriorityLow;
            return true;
        }
    }
}
