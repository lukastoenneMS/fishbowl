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

        public void Awake()
        {
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

            foreach (BoidRule rule in rules)
            {
                rule.Prepare(boids);
            }

            foreach (BoidParticle boid in boids)
            {
                boid.GetDebug(out BoidParticleDebug dbg);

                BoidState state = boid.GetState();

                BoidTarget newTarget = null;
                int newRuleIndex = -1;
                float maxPriority = -1.0f;
                for (int i = 0; i < rules.Count; ++i)
                {
                    BoidRule rule = rules[i];
                    if (rule.Evaluate(boid, state, out BoidTarget target, out float priority))
                    {
                        if (i == boid.CurrentRuleIndex)
                        {
                            // Add bias to the current rule's importance to avoid immediate switching
                            priority += CurrentRuleBias;
                        }

                        if (priority > maxPriority)
                        {
                            newRuleIndex = i;
                            maxPriority = priority;
                            newTarget = target;
                        }
                    }
                }

                boid.ApplyPhysics(newTarget);

                if (dbg != null)
                {
                    dbg.SetTarget(newTarget);
                }
            }

            foreach (BoidRule rule in rules)
            {
                rule.Cleanup();
            }
        }
    }
}
