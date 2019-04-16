// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Boids
{
    [System.Serializable]
    public class BoidBrain : MonoBehaviour
    {
        [SerializeField]
        private List<BoidRule> rules = new List<BoidRule>();
        public List<BoidRule> Rules => rules;

        /// Priority offset for the current rule to prevent immediate switching
        public float CurrentRuleBias = 0.0f;

        private readonly List<BoidParticle> boids = new List<BoidParticle>();
        private readonly List<BoidTarget> ruleTargets = new List<BoidTarget>();
        private readonly List<float> rulePriorities = new List<float>();

        private BoidContext context;

        public void Awake()
        {
            context = new BoidContext();

            GetComponentsInChildren<BoidParticle>(boids);

            foreach (BoidRule rule in rules)
            {
                rule.OnAwake();
            }
        }

        public void OnDestroy()
        {
            foreach (BoidRule rule in rules)
            {
                rule.OnDestroy();
            }
        }

        public void OnTransformChildrenChanged()
        {
            GetComponentsInChildren<BoidParticle>(boids);
        }

        void FixedUpdate()
        {
            ApplyRules();
        }

        private void ApplyRules()
        {
            int numBoids = boids.Count;

            context.Prepare(boids);
            ruleTargets.Clear();
            rulePriorities.Clear();
            ruleTargets.Capacity = rules.Count;
            rulePriorities.Capacity = rules.Count;
            foreach (BoidRule rule in rules)
            {
                rule.Prepare();
                ruleTargets.Add(null);
                rulePriorities.Add(-1.0f);
            }

            for (int boidIndex = 0; boidIndex < boids.Count; ++boidIndex)
            {
                BoidParticle boid = boids[boidIndex];
                boid.GetDebug(out BoidParticleDebug dbg);

                BoidState state = context.States[boidIndex];

                for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
                {
                    BoidRule rule = rules[ruleIndex];
                    if (rule.Evaluate(context, boid, boidIndex, state, out BoidTarget target, out float priority))
                    {
                        ruleTargets[ruleIndex] = target;
                        rulePriorities[ruleIndex] = priority;
                    }
                    else
                    {
                        ruleTargets[ruleIndex] = null;
                        rulePriorities[ruleIndex] = -1.0f;
                    }
                }

                // BoidTarget newTarget = SelectTargetByPriority(boid.CurrentRuleIndex);
                BoidTarget newTarget = SelectTargetByAverage(state, boid.CurrentRuleIndex);

                boid.ApplyPhysics(state, newTarget);

                if (dbg != null)
                {
                    dbg.SetTarget(newTarget);
                }
            }

            context.Cleanup();
            foreach (BoidRule rule in rules)
            {
                rule.Cleanup();
            }
        }

        private BoidTarget SelectTargetByPriority(int currentRuleIndex)
        {
            BoidTarget newTarget = null;
            float maxPriority = -1.0f;
            for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
            {
                BoidTarget target = ruleTargets[ruleIndex];
                if (target != null)
                {
                    float priority = rulePriorities[ruleIndex];
                    if (ruleIndex == currentRuleIndex)
                    {
                        // Add bias to the current rule's importance to avoid immediate switching
                        priority += CurrentRuleBias;
                    }

                    if (priority > maxPriority)
                    {
                        maxPriority = priority;
                        newTarget = target;
                    }
                }
            }

            return newTarget;
        }

        private BoidTarget SelectTargetByAverage(BoidState state, int currentRuleIndex)
        {
            BoidTarget newTarget = null;
            float totweight = 0.0f;
            for (int ruleIndex = 0; ruleIndex < rules.Count; ++ruleIndex)
            {
                BoidTarget target = ruleTargets[ruleIndex];
                if (target != null)
                {
                    float priority = rulePriorities[ruleIndex];
                    if (ruleIndex == currentRuleIndex)
                    {
                        // Add bias to the current rule's importance to avoid immediate switching
                        priority += CurrentRuleBias;
                    }

                    totweight += priority;
                    if (newTarget == null)
                    {
                        newTarget = target;
                    }
                    else
                    {
                        newTarget.direction += target.direction * priority;
                        newTarget.speed += target.speed * priority;
                    }
                }
            }
            if (totweight > 0.0f)
            {
                newTarget.direction.Normalize();
                newTarget.speed /= totweight;
            }

            return newTarget;
        }
    }
}
