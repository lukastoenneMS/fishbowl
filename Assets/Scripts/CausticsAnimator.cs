// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;

public class CausticsAnimator : MonoBehaviour
{
    public Material Material;
    public string TimePropertyName = "_WavePhase";
    public float Speed = 1.0f;

    // private MaterialPropertyBlock props;
    private int timePropId;
    private float time;

    void Start()
    {
        if (!Material)
        {
            enabled = false;
            return;
        }

        // props = new MaterialPropertyBlock();
        timePropId = Shader.PropertyToID(TimePropertyName);
        time = 0.0f;
    }

    void Update()
    {
        time += Speed * Time.deltaTime;

        // props.SetFloat(timePropId, time);
        Material.SetFloat(timePropId, time);
    }
}
