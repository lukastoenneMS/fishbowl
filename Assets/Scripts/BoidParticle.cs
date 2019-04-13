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
            Quaternion stateRotation = Quaternion.LookRotation(state.direction) * Quaternion.AngleAxis(state.roll, Vector3.forward);

            Vector3 predictedPosition = state.position + dtime * state.velocity;
            // Debug.DrawLine(state.position, predictedPosition, Color.grey);
            Vector3 targetDelta = target.position.Value - predictedPosition;
            Vector3 targetDirection = targetDelta.normalized;
            // Debug.DrawLine(state.position, state.position + targetDirection, Color.white);

            float targetSpeed = Math.Min(targetDelta.magnitude / dtime, settings.MaxSpeed);
            float targetAccel = Math.Min(targetDelta.magnitude / (dtime*dtime), settings.MaxAcceleration);
            // Vector3 targetVelocity = targetDirection * targetSpeed;
            Vector3 targetForce = targetDirection * targetAccel;

            Quaternion deltaRotation = Quaternion.LookRotation(targetDelta) * Quaternion.Inverse(stateRotation);
            deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 deltaAxis);
            float targetAngVelChange = Math.Min(Mathf.Deg2Rad * deltaAngle / (dtime*dtime), settings.MaxAngularVelocity);
            Vector3 targetTorque = deltaAxis * targetAngVelChange;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                // rb.velocity = targetVelocity;
                rb.AddForce(targetForce, ForceMode.Acceleration);
                rb.AddTorque(targetTorque, ForceMode.VelocityChange);
            }
        }

        public bool GetDebug(out BoidParticleDebug dbg)
        {
            if (EnableDebugObjects)
            {
                if (!debugObjects)
                {
                    debugObjects = new GameObject("Debugging");
                    debugObjects.transform.parent = transform;
                }

                dbg = new BoidParticleDebug(this, debugObjects.transform);
                return true;
            }
            else
            {
                if (debugObjects)
                {
                    Destroy(debugObjects);
                }

                dbg = null;
                return false;
            }
        }
    }
}
