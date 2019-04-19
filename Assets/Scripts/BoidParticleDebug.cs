// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Assertions;

namespace Boids
{
    public class BoidDebug
    {
        private bool enableTarget = false;
        private bool enablePhysics = false;
        private bool enableCollision = true;
        private bool enableSwarm = false;
        private bool enableBoidCollision = false;

        private Transform debugObjects
        {
            get
            {
                if (!debugObjects)
                {
                    debugObjects = new GameObject("Debugging");
                }
            }
        }

        public void SetTarget(BoidParticle particle, BoidTarget target)
        {
            if (!enableTarget || !particle.EnableDebugObjects)
            {
                return;
            }

            BoidState state = particle.GetState();
            var debugTarget = GetOrCreate("Target", PrimitiveType.Cube);
            var debugTargetDirection = GetOrCreate("TargetDirection", PrimitiveType.Cube);

            if (target != null)
            {
                debugTarget.gameObject.SetActive(true);
                debugTargetDirection.gameObject.SetActive(true);
                debugTarget.position = target.direction;
                SetTransformVector(debugTargetDirection, state.position, target.direction, 0.01f);

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

        public void SetPhysics(BoidParticle particle)
        {
            if (!enablePhysics || !particle.EnableDebugObjects)
            {
                return;
            }

            var state = particle.GetState();

            // {
            //     var dbgRoll = GetOrCreate("Roll", PrimitiveType.Cube);
            //     // float mix = state.roll / 360.0f;
            //     float mix = deltaRoll / 360.0f;
            //     SetTransformDirection(dbgRoll, state.position, Vector3.up * mix, 0.01f);
            //     Color color = Color.red * (1.0f - mix) + Color.yellow * mix;
            //     dbgRoll.GetComponent<Renderer>().material.color = color;
            // }
        }

        public void AddSwarmPoint(BoidParticle particle, Vector3 point, float weight)
        {
            if (!enableSwarm || !particle.EnableDebugObjects)
            {
                return;
            }

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

        public void AddCollisionPoint(BoidParticle particle, Vector3 hitPoint, Vector3 hitNormal)
        {
            if (!enableCollision || !particle.EnableDebugObjects)
            {
                return;
            }

            var state = particle.GetState();
            var collisionPoint = GetOrCreatePooled("CollisionPoint", PrimitiveType.Cube);
            var collisionNormal = GetOrCreatePooled("CollisionNormal", PrimitiveType.Cube);

            SetTransformVector(collisionPoint, state.position, hitPoint, 0.005f);
            SetTransformDirection(collisionNormal, hitPoint, hitNormal, 0.01f);

            collisionPoint.GetComponent<Renderer>().material.color = Color.cyan;
            collisionNormal.GetComponent<Renderer>().material.color = Color.yellow;
        }

        public void ClearCollision()
        {
            ClearPool("CollisionPoint");
            ClearPool("CollisionNormal");
        }

        public void AddBoidCollisionCone(BoidParticle particle, Vector3 dir, Vector3 colliderDir, float radius)
        {
            if (!enableBoidCollision || !particle.EnableDebugObjects)
            {
                return;
            }

            var state = particle.GetState();
            var collisionFwd = GetOrCreatePooled("CollisionFwd", PrimitiveType.Cube);
            var collisionDir = GetOrCreatePooled("CollisionDir", PrimitiveType.Cube);
            var collisionSphere = GetOrCreatePooled("CollisionSphere", PrimitiveType.Sphere);

            SetTransformDirection(collisionFwd, state.position, dir, 0.01f);
            SetTransformDirection(collisionDir, state.position, colliderDir, 0.005f);
            collisionSphere.position = state.position + colliderDir;
            collisionSphere.localScale = Vector3.one * radius * 2.0f;

            collisionFwd.GetComponent<Renderer>().material.color = Color.cyan;
            collisionDir.GetComponent<Renderer>().sharedMaterial = (Material)Resources.Load("DebugCollision", typeof(Material));
            collisionSphere.GetComponent<Renderer>().sharedMaterial = (Material)Resources.Load("DebugCollision", typeof(Material));
        }

        public void ClearBoidCollision()
        {
            ClearPool("CollisionFwd");
            ClearPool("CollisionDir");
            ClearPool("CollisionSphere");
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
            // We don't want a collider on debug objects
            dbg.GetComponent<Collider>().enabled = false;
            dbg.name = name;
            dbg.parent = debugObjects;
            dbg.localScale = new Vector3(0.02f, 0.02f, 0.02f);
            return dbg;
        }
    }
}
