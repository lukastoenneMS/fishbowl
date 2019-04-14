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
            state.roll = (rb.rotation.eulerAngles.z + 180.0f) % 360.0f - 180.0f;

            return state;
        }

        public void ApplyPhysics(BoidTarget target)
        {
            float dtime = Time.fixedDeltaTime;
            BoidState state = GetState();
            Quaternion uprightRotation = Quaternion.LookRotation(state.direction, Vector3.up);
            Quaternion stateRotation = uprightRotation * Quaternion.Euler(0.0f, 0.0f, state.roll);
            Quaternion test = Quaternion.Euler(0.0f, 0.0f, state.roll);

            GetDebug(out BoidParticleDebug dbg);

            Vector3 predictedPosition = state.position + dtime * state.velocity;

            Vector3 targetForce = Vector3.zero;
            Quaternion targetRotation = uprightRotation;
            if (target != null)
            {
                Vector3 targetDelta = target.position.Value - predictedPosition;
                Vector3 targetDirection = targetDelta.normalized;
                Vector3 v = state.velocity;
                Vector3 dv = GetTargetVelocityChange(state, target);

                // Adjust velocity change to not exceed max. velocity
                if ((v + dv).sqrMagnitude > settings.MaxSpeed * settings.MaxSpeed)
                {
                    // Solves equation: ||v + lambda * dv|| = maxVelocity
                    // lambda clamped to [0, 1] to not accelerate backwards and not add more than desired velocity
                    float v_v = Vector3.Dot(v, v);
                    float dv_dv = Vector3.Dot(dv, dv);
                    float v_dv = Vector3.Dot(v, dv);
                    if (dv_dv > 0.0f)
                    {
                        float lambda = (settings.MaxSpeed * Mathf.Sqrt(v_v * dv_dv + v_dv * v_dv) - v_dv) / dv_dv;
                        dv *= Mathf.Clamp(lambda, 0.0f, 1.0f);
                    }
                }

                targetForce = dv;
                targetRotation = Quaternion.LookRotation(targetDelta, Vector3.up);
            }

            // float targetSpeed = Mathf.Clamp(projectedDelta / dtime, 0.0f, settings.MaxSpeed);
            // float targetAccel = Mathf.Clamp(projectedDelta / (dtime*dtime), 0.0f, settings.MaxAcceleration);
            // Vector3 targetVelocity = state.direction * targetSpeed;
            // Vector3 targetForce = state.direction * targetAccel;

            Quaternion deltaRotation = targetRotation * Quaternion.Inverse(stateRotation);
            deltaRotation.ToAngleAxis(out float deltaAngle, out Vector3 deltaAxis);
            deltaAngle = (deltaAngle + 180.0f) % 360.0f - 180.0f;
            if (dbg != null)
            {
                dbg.SetPhysics(deltaAngle);
            }
            float targetAngVelChange = Math.Min(Mathf.Deg2Rad * deltaAngle / dtime, settings.MaxAngularVelocity);
            Vector3 targetTorque = deltaAxis * targetAngVelChange;

            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            if (rb)
            {
                rb.AddForce(targetForce, ForceMode.VelocityChange);
                rb.AddTorque(targetTorque, ForceMode.VelocityChange);
            }
        }

        private Vector3 GetTargetVelocityChange(BoidState state, BoidTarget target)
        {
            float dtime = Time.fixedDeltaTime;
            Vector3 predictedPosition = state.position + dtime * state.velocity;
            Vector3 targetDelta = target.position.Value - predictedPosition;
            Vector3 targetDirection = targetDelta.normalized;

            float projectedDelta = Vector3.Dot(targetDelta, state.direction);
            float velocityChange = Mathf.Clamp(projectedDelta / dtime, 0.0f, settings.MaxAcceleration * dtime);

            return velocityChange * state.direction;
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
