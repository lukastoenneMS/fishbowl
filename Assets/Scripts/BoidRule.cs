// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Boids
{
    // Result of evaluating a boid rule for a given particle
    public class BoidTarget : IEquatable<BoidTarget>
    {
        public readonly Vector3? position;
        public readonly Vector3? direction;
        public readonly float? speed;

        public bool valid
        {
            get
            {
                return position.HasValue || direction.HasValue;
            }
        }

        public bool Equals(BoidTarget other)
        {
            return position == other.position || (direction == other.direction && speed == other.speed);
        }

        public BoidTarget(Vector3 _position)
        {
            position = _position;
        }

        public BoidTarget(Vector3 _direction, float _speed)
        {
            direction = _direction;
            speed = _speed;
        }

        public BoidTarget transformed(Transform transform)
        {
            if (position.HasValue)
            {
                return new BoidTarget(transform.TransformPoint(position.Value));
            }
            else if (direction.HasValue && speed.HasValue)
            {
                return new BoidTarget(transform.TransformDirection(direction.Value), speed.Value);
            }
            else
            {
                return null;
            }
        }
    }

    public abstract class BoidRule
    {
        public abstract BoidTarget Evaluate(BoidParticle boid, BoidState state);
    }

    [System.Serializable]
    public class SimpleCircleRule : BoidRule
    {
        public float radius = 1.0f;
        public Vector3 center = Vector3.zero;

        public override BoidTarget Evaluate(BoidParticle boid, BoidState state)
        {
            Vector3 goal = new Vector3(state.position.x, 0.0f, state.position.z);
            goal = goal.normalized * radius + center;

            return new BoidTarget(goal);
        }
    }
}
