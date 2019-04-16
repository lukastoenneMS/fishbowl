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
}
