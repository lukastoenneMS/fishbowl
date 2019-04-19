// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    public class BoidState
    {
        public int instanceID;

        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float roll;
        public Vector3 angularVelocity;

        public BoidState(BoidParticle boid)
        {
            instanceID = boid.GetInstanceID();
            boid.GetPhysicsState(this);
        }
    }

    public class BoidContext
    {
        private const int maxPointsPerLeafNode = 32;

        private KDTree tree;
        public KDTree Tree => tree;
        private KDQuery query;
        public KDQuery Query => query;

        private BoidState[] states = new BoidState[0];
        public BoidState[] States => states;

        private BoidParticle[] boids = new BoidParticle[0];
        public BoidParticle[] Boids => boids;

        public BoidContext()
        {
            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();
        }

        public void UpdateBoidParticles(BoidParticle[] newBoids)
        {
            boids = newBoids;

            var compareInstanceIDs = new Comparison<BoidParticle>((a, b) => a.GetInstanceID().CompareTo(b.GetInstanceID()));
            Array.Sort(boids, compareInstanceIDs);

            BoidState[] oldStates = states;
            states = new BoidState[boids.Length];

            int oldIdx = 0;
            for (int i = 0; i < boids.Length; ++i)
            {
                BoidParticle boid = boids[i];
                int instanceID = boid.GetInstanceID();

                // Discard states without boid particle
                while (oldIdx < oldStates.Length && instanceID > oldStates[oldIdx].instanceID)
                {
                    ++oldIdx;
                }

                if (oldIdx < oldStates.Length && instanceID == oldStates[oldIdx].instanceID)
                {
                    // Copy existing states for matching particles
                    states[i] = oldStates[oldIdx];
                }
                else
                {
                    // Add new states for boid particles without state
                    states[i] = new BoidState(boid);
                }
            }
        }

        public void Prepare()
        {
            tree.SetCount(boids.Length);

            for (int i = 0; i < states.Length; ++i)
            {
                boids[i].GetPhysicsState(states[i]);
                tree.Points[i] = states[i].position;
            }
            tree.Rebuild();
        }

        public void Cleanup()
        {
        }

        public bool RequestRayCast(Vector3 origin, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Physics.Raycast(origin, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }

        public bool RequestSphereCast(Vector3 origin, float radius, Vector3 direction, out RaycastHit hitInfo, float maxDistance, int layerMask, QueryTriggerInteraction queryTriggerInteraction)
        {
            return Physics.SphereCast(origin, radius, direction, out hitInfo, maxDistance, layerMask, queryTriggerInteraction);
        }
    }
}
