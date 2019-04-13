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

    public abstract class BoidRule
    {
        public abstract BoidTarget Evaluate(BoidParticle boid, BoidState state);

        public abstract void Prepare(List<BoidParticle> boids);

        public abstract void Cleanup();
    }

    [System.Serializable]
    public class SimpleCircleRule : BoidRule
    {
        public float radius = 1.0f;
        public Vector3 center = Vector3.zero;

        public override BoidTarget Evaluate(BoidParticle boid, BoidState state)
        {
            Vector3 goal = new Vector3(state.position.x, 0.0f, state.position.z);
            goal = goal.normalized * radius + center;

            return new BoidTarget(goal);
        }

        public override void Prepare(List<BoidParticle> boids)
        {
        }

        public override void Cleanup()
        {
        }
    }

    [System.Serializable]
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

        public SwarmRule()
        {
            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();
            queryResults = new List<int>();
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

        public override BoidTarget Evaluate(BoidParticle boid, BoidState state)
        {
            float deltaRadius = maxRadius - minRadius;
            if (deltaRadius <= 0.0f)
            {
                return null;
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

                goal += p * weight;
                totweight += weight;
            }
            if (totweight > 0.0f)
            {
                goal /= totweight;
                return new BoidTarget(goal);
            }
            else
            {
                return null;
            }
        }
    }
}
