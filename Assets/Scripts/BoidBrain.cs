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

        private readonly List<BoidParticle> boids = new List<BoidParticle>();

        public BoidBrain()
        {
            // rules.Add(new SimpleCircleRule() {radius = 3.0f, center = new Vector3(0, 0, -4)});
            rules.Add(new SwarmRule() {searchRadius = 1.5f});
        }

        public void Awake()
        {
            GetComponentsInChildren<BoidParticle>(boids);
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
                BoidState state = boid.GetState();
                foreach (BoidRule rule in rules)
                {
                    BoidTarget target = ApplyRuleFuzzy(rule, boid, state);
                    if (target != null)
                    {
                        boid.ApplyTarget(target);
                        break;
                    }
                }
            }

            foreach (BoidRule rule in rules)
            {
                rule.Cleanup();
            }
        }

        private BoidTarget ApplyRuleFuzzy(BoidRule rule, BoidParticle boid, BoidState state)
        {
            BoidTarget target = rule.Evaluate(boid, state);
            return target;
        }
    }
}
