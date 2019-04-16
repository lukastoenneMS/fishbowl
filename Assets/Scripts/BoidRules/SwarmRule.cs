// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using DataStructures.ViliWonka.KDTree;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    [CreateAssetMenu(fileName = "SwarmRule", menuName = "Boids/SwarmRule", order = 1)]
    public class SwarmRule : BoidRule
    {
        /// <summary>
        /// Maximum distance of other boids to influence the swarm searching behavior.
        /// </summary>
        public float maxRadius = 3.0f;

        /// <summary>
        /// Minimum distance to activate swarm behavior.
        /// </summary>
        public float minRadius = 1.0f;

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
            Vector3 dir = Vector3.zero;
            foreach (int idx in queryResults)
            {
                // Skip own point
                if (idx == boidIndex)
                {
                    continue;
                }

                BoidState queryState = context.States[idx];
                Vector3 delta = queryState.position - state.position;
                float dist = delta.magnitude;

                // float weight = Mathf.Clamp(1.0f - (dist - minRadius) / deltaRadius, 0.0f, 1.0f);
                float weight = Mathf.Clamp(1.0f - dist / maxRadius, 0.0f, 1.0f);
                if (forwardAsymmetry > 0.0f)
                {
                    float fwd = Vector3.Dot(delta.normalized, state.direction);
                    float fwdFactor = 0.5f + 0.5f * fwd;
                    weight *= 1.0f - (1.0f - fwdFactor) * forwardAsymmetry;
                }

                // TODO arbitrary blending between goal behavior and direction alignment
                float dirGoalBlend = Mathf.SmoothStep(0.0f, 1.0f, (dist - 0.5f * minRadius)/minRadius);

                dir += queryState.direction * weight * (1.0f - dirGoalBlend);
                dir += delta * weight * dirGoalBlend;

                if (dbg != null)
                {
                    dbg.AddSwarmPoint(queryState.position, weight);
                }

                totweight += weight;
            }
            if (totweight > 0.0f)
            {
                dir /= totweight;
                target = new BoidTarget(dir, boid.Settings.MaxAcceleration);
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
