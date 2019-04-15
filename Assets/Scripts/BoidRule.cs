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
        public readonly Vector3? position;
        public readonly Vector3? direction;
        public readonly float? speed;

        public bool valid
        {
            get
            {
                return position.HasValue || direction.HasValue;
            }
        }

        public bool Equals(BoidTarget other)
        {
            return position == other.position || (direction == other.direction && speed == other.speed);
        }

        public BoidTarget(Vector3 _position)
        {
            position = _position;
        }

        public BoidTarget(Vector3 _direction, float _speed)
        {
            direction = _direction;
            speed = _speed;
        }

        public BoidTarget transformed(Transform transform)
        {
            if (position.HasValue)
            {
                return new BoidTarget(transform.TransformPoint(position.Value));
            }
            else if (direction.HasValue && speed.HasValue)
            {
                return new BoidTarget(transform.TransformDirection(direction.Value), speed.Value);
            }
            else
            {
                return null;
            }
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

        public virtual void Prepare(List<BoidParticle> boids)
        {
        }

        public virtual void Cleanup()
        {
        }

        public virtual bool Evaluate(BoidParticle boid, BoidState state, out BoidTarget target, out float priority)
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

        public override bool Evaluate(BoidParticle boid, BoidState state, out BoidTarget target, out float priority)
        {
            Vector3 localPos = state.position - center;
            Vector3 goal = new Vector3(localPos.x, 0.0f, localPos.z);
            goal = goal.normalized * radius + center;

            target = new BoidTarget(goal);
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

        private KDTree tree;
        private KDQuery query;
        private List<int> queryResults;

        public override void OnAwake()
        {
            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();
            queryResults = new List<int>();
        }

        public override void OnDestroy()
        {
            tree = null;
            query = null;
            queryResults = null;
        }

        public override void Prepare(List<BoidParticle> boids)
        {
            tree.SetCount(boids.Count);
            for (int i = 0; i < boids.Count; ++i)
            {
                BoidState state = boids[i].GetState();
                tree.Points[i] = state.position;
            }
            tree.Rebuild();
        }

        public override void Cleanup()
        {
            queryResults.Clear();
        }

        public override bool Evaluate(BoidParticle boid, BoidState state, out BoidTarget target, out float priority)
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
            query.Radius(tree, state.position, maxRadius, queryResults);

            float totweight = 0.0f;
            Vector3 goal = Vector3.zero;
            foreach (int idx in queryResults)
            {
                Vector3 p = tree.Points[idx];
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
                    float fwd = Vector3.Dot(d, state.direction);
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
                target = new BoidTarget(goal);
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
}
