// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(fileName = "GoalRule", menuName = "Boids/GoalRule", order = 1)]
    public class GoalRule : BoidRule
    {
        public float minDistance = 1.0f;
        public float maxDistance = 3.0f;
        public GameObject goal = null;
        public Vector3 goalVector = Vector3.zero;

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            Vector3 currentGoal = goalVector;
            if (goal)
            {
                currentGoal = goal.transform.position;
            }

            Vector3 delta = currentGoal - state.position;
            float distance = delta.magnitude;
            float blendRange = (maxDistance - minDistance) * 0.25f;
            float mid = (minDistance + maxDistance) * 0.5f;
            if (distance < minDistance + blendRange)
            {
                target = new BoidTarget(-delta.normalized, boid.Settings.MaxSpeed);
                float weight = Mathf.Clamp((mid - distance) / blendRange, 0.0f, 1.0f);
                priority = PriorityHigh * weight;
                return true;
            }
            else if (distance > maxDistance - blendRange)
            {
                target = new BoidTarget(delta.normalized, boid.Settings.MaxSpeed);
                float weight = Mathf.Clamp((distance - mid) / blendRange, 0.0f, 1.0f);
                priority = PriorityHigh * weight;
                return true;
            }

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
