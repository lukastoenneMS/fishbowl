// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Boids;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[RequireComponent(typeof(BoidBrain))]
public class FishBowl : MonoBehaviour
{
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
    }

    void Start()
    {
        BoidGenerator generator = new BoidGenerator();
        generator.CreateBoids(brain.transform, brain.Settings, Count);
    }
}
