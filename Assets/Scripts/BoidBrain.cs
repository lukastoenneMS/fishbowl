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
                BoidTarget target = null;
                foreach (BoidRule rule in rules)
                {
                    target = ApplyRuleFuzzy(rule, boid, state);
                    if (target != null)
                    {
                        break;
                    }
                }

                boid.ApplyPhysics(target);

                if (dbg != null)
                {
                    dbg.SetTarget(target);
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
