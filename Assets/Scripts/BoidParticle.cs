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
    }

    public class BoidState
    {
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 direction;
        public float roll;
    }

    [System.Serializable]
    public class BoidParticle : MonoBehaviour
    {
        [SerializeField]
        private BoidSettings settings = new BoidSettings();
        public BoidSettings Settings => settings;

        public bool EnableDebugObjects = false;

        private GameObject debugObjects = null;

        void Awake()
        {
            if (EnableDebugObjects)
            {
                debugObjects = new GameObject("Debugging");
                debugObjects.transform.parent = transform;
            }
        }

        void OnDestroy()
        {
            if (debugObjects)
            {
                Destroy(debugObjects);
            }
        }

        public BoidState GetState()
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            Assert.IsNotNull(rb);

            BoidState state = new BoidState();
            state.position = rb.position;
            state.velocity = rb.velocity;
            state.direction = rb.transform.forward;
            Vector3 plane = Vector3.Cross(rb.transform.forward, Vector3.up).normalized;
            state.roll = (float)Math.Acos((double)Vector3.Cross(rb.transform.up, plane).magnitude);

            return state;
        }

        public void ApplyTarget(BoidTarget target)
        {
            float dtime = Time.fixedDeltaTime;
            BoidState state = GetState();

            Vector3 predictedPosition = state.position + dtime * state.velocity;
            // Debug.DrawLine(state.position, predictedPosition, Color.grey);
            Vector3 targetDelta = target.position.Value - predictedPosition;
            Vector3 targetDirection = targetDelta.normalized;
            // Debug.DrawLine(state.position, state.position + targetDirection, Color.white);

            float targetSpeed = Math.Min(targetDelta.magnitude / dtime, settings.MaxSpeed);
            float targetAccel = Math.Min(targetDelta.magnitude / (dtime*dtime), settings.MaxAcceleration);
            // Vector3 targetVelocity = targetDirection * targetSpeed;
            Vector3 targetForce = targetDirection * targetAccel;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                // rb.velocity = targetVelocity;
                rb.AddForce(targetForce * rb.mass);
            }

            UpdateDebugObjects(state, target);
        }

        private void UpdateDebugObjects(BoidState state, BoidTarget target)
        {
            if (debugObjects)
            {
                var debugTarget = GetOrCreateDebugObject("Target", PrimitiveType.Cube);
                var debugTargetDirection = GetOrCreateDebugObject("TargetDirection", PrimitiveType.Cube);

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
                }
                else
                {
                    debugTarget.gameObject.SetActive(false);
                    debugTargetDirection.gameObject.SetActive(false);
                }
            }
        }

        private Transform GetOrCreateDebugObject(string name, PrimitiveType prim)
        {
            if (debugObjects)
            {
                var ob = debugObjects.transform.Find(name);
                if (!ob)
                {
                    ob = GameObject.CreatePrimitive(prim).transform;
                    ob.name = name;
                    ob.parent = debugObjects.transform;
                    ob.localScale = new Vector3(0.02f, 0.02f, 0.02f);
                }
                return ob;
            }
            return null;
        }
    }
}
