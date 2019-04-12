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
    public class BoidSettings
    {
        public float MinSpeed = 0.0f;
        public float MaxSpeed = 10.0f;
        public float MaxAcceleration = 0.5f;
        public float MaxAngularVelocity = 0.5f;
        public float PersonalSpace = 1.0f;

        public float Banking = 1.0f;
        public float Pitch = 1.0f;

        public List<BoidRule> rules = new List<BoidRule>();
    }

    [System.Serializable]
    public class BoidBrain : MonoBehaviour
    {
        [SerializeField]
        private BoidSettings settings = new BoidSettings();
        public BoidSettings Settings => settings;

        private readonly List<BoidParticle> boids = new List<BoidParticle>();

        public BoidBrain()
        {
            settings.rules.Add(new SimpleCircleRule() {radius = 3.0f, center = new Vector3(0, 0, -4)});
        }

        private BoidTarget ApplyRuleFuzzy(BoidRule rule, BoidParticle boid, BoidState state)
        {
            BoidTarget target = rule.Evaluate(boid, state);
            return target;
        }

        public void Apply(BoidSettings settings)
        {
            int numBoids = boids.Count;

            foreach (BoidParticle boid in boids)
            {
                BoidState state = boid.GetState();
                foreach (BoidRule rule in settings.rules)
                {
                    BoidTarget target = ApplyRuleFuzzy(rule, boid, state);
                    if (target != null)
                    {
                        boid.ApplyTarget(target, settings);
                        break;
                    }
                }
            }
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
            Apply(settings);
        }
    }
}
