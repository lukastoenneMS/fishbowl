// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Boids
{
    public static class BoidParticleDebug
    {
        public static void UpdateDebugTarget(BoidParticle particle, BoidState state, BoidTarget target, float force)
        {
            if (GetOrCreateDebugObject(particle, "Target", PrimitiveType.Cube, out Transform debugTarget) &&
                GetOrCreateDebugObject(particle, "TargetDirection", PrimitiveType.Cube, out Transform debugTargetDirection))
            {
                if (target.position.HasValue)
                {
                    Vector3 avg = 0.5f * (target.position.Value + state.position);
                    Vector3 delta = target.position.Value - state.position;

                    debugTarget.gameObject.SetActive(true);
                    debugTargetDirection.gameObject.SetActive(true);
                    debugTarget.position = target.position.Value;
                    debugTargetDirection.position = avg;
                    debugTargetDirection.rotation = Quaternion.FromToRotation(Vector3.forward, delta);
                    debugTargetDirection.localScale = new Vector3(0.01f, 0.01f, delta.magnitude);

                    Color color = Color.white * (1.0f - force) + Color.green * force;
                    debugTarget.GetComponent<Renderer>().material.color = color;
                    debugTargetDirection.GetComponent<Renderer>().material.color = color;
                }
                else
                {
                    debugTarget.gameObject.SetActive(false);
                    debugTargetDirection.gameObject.SetActive(false);
                }
            }
        }

        private static bool GetOrCreateDebugObject(BoidParticle particle, string name, PrimitiveType prim, out Transform dbg)
        {
            var dbgParent = particle.GetDebugObjects();
            if (dbgParent)
            {
                dbg = dbgParent.Find(name);
                if (!dbg)
                {
                    dbg = GameObject.CreatePrimitive(prim).transform;
                    dbg.name = name;
                    dbg.parent = dbgParent;
                    dbg.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                }
                return true;
            }

            dbg = null;
            return false;
        }
    }
}
