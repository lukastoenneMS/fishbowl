// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Boids;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Utilities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[RequireComponent(typeof(BoidBrain))]
public class FishBowl : MonoBehaviour
{
    public GameObject BoidPrefab = null;
    public int Count = 10;

    private BoidBrain brain = null;

    void Awake()
    {
        brain = GetComponent<BoidBrain>();
        if (!brain)
        {
            Debug.LogWarning("No boid brain component found");
            enabled = false;
        }

        foreach (var rule in brain.Rules)
        {
            var goalRule = rule as GoalRule;
            if (goalRule != null)
            {
                goalRule.goal = CameraCache.Main.gameObject;
            }
        }
    }

    void Start()
    {
        if (BoidPrefab)
        {
            BoidGenerator generator = new BoidGenerator();
            generator.CreateBoids(brain.transform, BoidPrefab, Count);
        }
    }
}
