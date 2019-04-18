# Building an MRTK app from scratch

## Overview

The basic motivation for the app was to experiment with swarm behavior. The concept is based mostly on Blender's particle system and its "Boids" feature ([Wikipedia on Boids](https://en.wikipedia.org/wiki/Boids), [Boids in Blender](https://docs.blender.org/manual/en/latest/physics/particles/emitter/physics/boids.html)). Using this as a starting point, the system was first built in Unity and C#, initially without involving MRTK. Once the core components were working sufficiently, the MRTK package was imported and its features used to add user interaction.

## Boids

Boids are made up of a large number of similar objects that follow a set of rules to guide their movement. The rules are translated into concrete force and torque values which are applied to rigid body objects. The existing physics simulation then takes care of actually moving the objects in space.

### BoidParticle

This is a prefab for the individual objects in the boids simulation. It has a settings component that defines details of physical movement, such as limits for velocity, acceleration, and rotation. It also contains the visual elements, i.e. skinned mesh renderer and rigging objects to control animation. A collider component enables collisions in the physics simulation (not to be confused with collision avoidance behavior, wich works at longer ranges and attempts to avoid collision in the first place, see TODO)

### BoidBrain

The boid brain manages the rules for boid behavior. All child objects with a BoidParticle component participate in the boid simulation. The brain also provides a shared context for boids, which improves performance by avoiding redundant computations, e.g. for spatial lookups.

### BoidRule

Each rule defines one aspect of boid behavior, such as swarming, avoiding collisions, fleeing from predators, etc.. Rules are evaluated for each boid particle and can take into account its state, as well as the state of other boids in the simulation.

Rules are implemented as assets in Unity, i.e. each rule gets serialized into its own asset file.

### BoidGenerator

A utility class that creates boids from prefabs.

Currently supports simple randomized boids inside a given radius. Future generators TBD.

### BoidContext

Cached boid data for performance reasons. The context is provided during rule evaluation. Includes a k-d tree for fast spatial lookups, such as closest neighbors of a boid particle.

## MRTK features

## MRTK notes

1. Importing the MRTK package and configuring the scene went smoothly. This may be simply because i have done it many times and am used to it.

1. When running the app on a Hololens it initially appeared as a slate, rather than a fully fledged MR app. Solution: Build Settings > Player Settings > XR Settings > Tick "Virtual Reality Supported". Is this mentioned in docs?

1. Tried enabling "WSA Holographic Remoting Supported" in Player Settings just because it sounds nice, but that is missing some dlls and doesn't work out of the box.

1. Feature: Fish following the player camera. This was quite simple, but also because i knew about CameraCache beforehand. Rather than using a Camera property, the goal object is set at runtime by looking up CameraCache.Main.

1. Feature: Grabbing fish out of the water
    - Decided to not look at demo files this time, but try and follow the docs to see if they are sufficient.
    