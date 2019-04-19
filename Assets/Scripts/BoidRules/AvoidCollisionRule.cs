// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        /// Maximum distance at which collisions will be detected.
        /// </summary>
        public float DetectionDistance = 1.0f;

        /// <summary>
        /// Layers to include in collision detection.
        /// </summary>
        public LayerMask Layers = UnityEngine.Physics.DefaultRaycastLayers;

        private const int maxHits = 16;
        private Collider[] colliders = new Collider[maxHits];
        private RaycastHit[] results = new RaycastHit[maxHits];

        private Vector3 randomDir = Vector3.zero;
        private float lastRandomDirTime = 0.0f;
        private const float randomDirInterval = 1.5f;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            BoidSettings settings = boid.Settings;

#if false
            int numOverlaps = Physics.OverlapSphereNonAlloc(state.position, DetectionDistance, colliders, Layers, QueryTriggerInteraction.Ignore);
            if (numOverlaps > 0)
            {
                int count = 0;
                Vector3 steer = Vector3.zero;
                for (int i = 0; i < numOverlaps; ++i)
                {
                    // Vector3 closest = colliders[i].ClosestPoint(state.position);
                    Vector3 closest = colliders[i].transform.position;
                    Vector3 delta = state.position - closest;
                    float sqrDist = delta.sqrMagnitude;
                    if (sqrDist > 0.0f)
                    {
                        float weight = settings.SeparationDistance / sqrDist;

                        steer += delta * weight;
                        ++count;
                    }
                }

                if (count > 0)
                {
                    float speed = settings.MaxSpeed;
                    target = new BoidTarget(steer.normalized, speed);
                    priority = PriorityMedium;
                    return true;
                }
                // // Try to escape by chosing a random direction
                // if (Time.time >= lastRandomDirTime + randomDirInterval)
                // {
                //     randomDir = UnityEngine.Random.onUnitSphere;
                //     lastRandomDirTime = Time.time;
                // }

                // float speed = settings.MaxSpeed;
                // target = new BoidTarget(randomDir, speed);
                // priority = PriorityCritical;
                // return true;
            }
#endif

#if true
            int count = 0;
            Vector3 steer = Vector3.zero;

            // int numHits = Physics.SphereCastNonAlloc(state.position, settings.SeparationDistance, state.direction, results, DetectionDistance, Layers, QueryTriggerInteraction.Ignore);
            // if (numHits > 0)
            RaycastHit hit;
            if (Physics.SphereCast(state.position, settings.SeparationDistance, state.direction, out hit, DetectionDistance, Layers, QueryTriggerInteraction.Ignore))
            {
                // for (int i = 0; i < numHits; ++i)
                // {
                    // RaycastHit hit = results[i];
                    if (GetAvoidanceSteering(boid, state, hit, out Vector3 collSteer))
                    {
                        BoidDebug.AddCollisionPoint(boid, hit.point, collSteer);
                        steer += collSteer;
                        ++count;
                    }
                // }
            }

            // int numRayHits = Physics.RaycastNonAlloc(state.position, state.direction, results, DetectionDistance, Layers, QueryTriggerInteraction.Ignore);
            // if (numRayHits > 0)
            else if (Physics.Raycast(state.position, state.direction, out hit, DetectionDistance, Layers, QueryTriggerInteraction.Ignore))
            {
                // for (int i = 0; i < numHits; ++i)
                // {
                    // RaycastHit hit = results[i];
                    if (GetAvoidanceSteering(boid, state, hit, out Vector3 collSteer))
                    {
                        BoidDebug.AddCollisionPoint(boid, hit.point, collSteer);
                        steer += collSteer;
                        ++count;
                    }
                // }
            }

            if (count > 0)
            {
                float speed = settings.MaxSpeed;
                target = new BoidTarget(steer.normalized, speed);
                priority = PriorityMedium;
                return true;
            }
#endif

            target = null;
            priority = PriorityNone;
            return false;
        }

        private bool GetAvoidanceSteering(BoidParticle boid, BoidState state, RaycastHit hit, out Vector3 steer)
        {
            if (boid.EnableDebugObjects)
            {

            }
            if (hit.collider != null && hit.distance > 0.0f)
            {
                Vector3 delta = state.position - hit.point;
                float sqrDist = delta.sqrMagnitude;
                if (sqrDist > 0.0f)
                {
                    float weight = boid.Settings.SeparationDistance / sqrDist;
                    steer = Vector3.ProjectOnPlane(delta, state.direction).normalized;
                    if (steer == Vector3.zero)
                    {
                        steer = Vector3.ProjectOnPlane(GetRandomDirection(), state.direction).normalized;
                    }

                    steer *= weight;
                    return true;
                }
            }

            steer = Vector3.zero;
            return false;
        }

        private Vector3 GetRandomDirection()
        {
            // Try to escape by chosing a random direction
            if (Time.time >= lastRandomDirTime + randomDirInterval)
            {
                randomDir = UnityEngine.Random.onUnitSphere;
                lastRandomDirTime = Time.time;
            }

            return randomDir;
        }
    }
}
