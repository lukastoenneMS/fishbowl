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

        public override bool Evaluate(BoidContext context, BoidParticle boid, int boidIndex, BoidState state, out BoidTarget target, out float priority)
        {
            if (goal)
            {
                Vector3 delta = goal.transform.position - state.position;
                float distance = delta.magnitude;
                if (distance < minDistance)
                {
                    target = new BoidTarget(-delta.normalized, boid.Settings.MaxSpeed);
                    priority = PriorityHigh;
                    return true;
                }
                else if (distance > maxDistance)
                {
                    target = new BoidTarget(delta.normalized, boid.Settings.MaxSpeed);
                    priority = PriorityHigh;
                    return true;
                }
            }

            target = null;
            priority = PriorityNone;
            return false;
        }
    }
}
