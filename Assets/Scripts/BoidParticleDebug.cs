// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Boids
{
    public class BoidParticleDebug
    {
        private BoidParticle particle = null;
        private Transform debugObjects = null;

        public BoidParticleDebug(BoidParticle particle, Transform debugObjects)
        {
            this.particle = particle;
            this.debugObjects = debugObjects;
        }

        public void SetTarget(BoidTarget target)
        {
            var state = particle.GetState();
            var debugTarget = GetOrCreate("Target", PrimitiveType.Cube);
            var debugTargetDirection = GetOrCreate("TargetDirection", PrimitiveType.Cube);

            if (target != null && target.position.HasValue)
            {
                debugTarget.gameObject.SetActive(true);
                debugTargetDirection.gameObject.SetActive(true);
                debugTarget.position = target.position.Value;
                SetTransformVector(debugTargetDirection, state.position, target.position.Value, 0.01f);

                // Color color = Color.white * (1.0f - force) + Color.green * force;
                Color color = Color.green;
                debugTarget.GetComponent<Renderer>().material.color = color;
                debugTargetDirection.GetComponent<Renderer>().material.color = color;
            }
            else
            {
                debugTarget.gameObject.SetActive(false);
                debugTargetDirection.gameObject.SetActive(false);
            }
        }

        public void AddSwarmPoint(Vector3 point, float weight)
        {
            var state = particle.GetState();
            var swarm = GetOrCreatePooled("Swarm", PrimitiveType.Cube);

            SetTransformVector(swarm, state.position, point, 0.01f);

            Color color = Color.blue * (1.0f - weight) + Color.red * weight;
            swarm.GetComponent<Renderer>().material.color = color;
        }

        public void ClearSwarm()
        {
            ClearPool("Swarm");
        }

        private Transform GetOrCreate(string name, PrimitiveType prim)
        {
            var dbg = debugObjects.Find(name);
            if (!dbg)
            {
                dbg = CreateDebugPrimitive(name, prim);
            }
            return dbg;
        }

        private Transform GetOrCreatePooled(string name, PrimitiveType prim)
        {
            for (int i = 0; i < debugObjects.childCount; ++i)
            {
                var child = debugObjects.GetChild(i);
                if (child.name == name && !child.gameObject.activeSelf)
                {
                    child.gameObject.SetActive(true);
                    return child;
                }
            }

            return CreateDebugPrimitive(name, prim);
        }

        private void ClearPool(string name)
        {
            for (int i = 0; i < debugObjects.childCount; ++i)
            {
                var child = debugObjects.GetChild(i);
                if (child.name == name)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        private static void SetTransformVector(Transform dbg, Vector3 from, Vector3 to, float size)
        {
            Vector3 direction = to - from;
            dbg.position = 0.5f * (from + to);
            dbg.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            dbg.localScale = new Vector3(size, size, direction.magnitude);
        }

        private static void SetTransformDirection(Transform dbg, Vector3 origin, Vector3 direction, float size)
        {
            dbg.position = origin + 0.5f * direction;
            dbg.rotation = Quaternion.FromToRotation(Vector3.forward, direction);
            dbg.localScale = new Vector3(size, size, direction.magnitude);
        }

        private Transform CreateDebugPrimitive(string name, PrimitiveType prim)
        {
            var dbg = GameObject.CreatePrimitive(prim).transform;
            dbg.name = name;
            dbg.parent = debugObjects;
            dbg.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            // We don't want a collider on debug objects
            GameObject.Destroy(dbg.GetComponent<Collider>());
            return dbg;
        }
    }
}
