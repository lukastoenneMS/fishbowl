// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    public class BoidContext
    {
        private const int maxPointsPerLeafNode = 32;

        private KDTree tree;
        public KDTree Tree => tree;
        private KDQuery query;
        public KDQuery Query => query;

        private readonly List<BoidState> states = new List<BoidState>();
        public List<BoidState> States => states;

        public BoidContext()
        {
            int maxPointsPerLeafNode = 32;
            tree = new KDTree(maxPointsPerLeafNode);
            query = new KDQuery();
        }

        public void Prepare(List<BoidParticle> boids)
        {
            states.Capacity = boids.Count;
            states.Clear();
            tree.SetCount(boids.Count);

            for (int i = 0; i < boids.Count; ++i)
            {
                BoidState state = new BoidState();
                states.Add(state);
                boids[i].GetPhysicsState(state);
                tree.Points[i] = state.position;
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
