#if BZ
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ECCLibrary;

/// <summary>
/// Base class for creatures that can walk. This class is exclusive to Below Zero.
/// </summary>
public abstract class AmphibiousCreatureAsset : CreatureAsset
{
    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="classId"></param>
    /// <param name="friendlyName"></param>
    /// <param name="description"></param>
    /// <param name="model">Requires a sphere collider.</param>
    /// <param name="spriteTexture"></param>
    protected AmphibiousCreatureAsset(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description, model, spriteTexture)
    {
    }

    internal override void ApplyInternalChanges(CreatureComponents components)
    {
        var onSurfaceTracker = prefab.EnsureComponent<OnSurfaceTracker>();
        onSurfaceTracker.maxSurfaceAngle = MaxSurfaceAngle;

        var onSurfaceMovement = prefab.EnsureComponent<OnSurfaceMovement>();
        onSurfaceMovement.onSurfaceTracker = onSurfaceTracker;
        onSurfaceMovement.locomotion = components.locomotion;

        var walkBehaviour = components.swimBehaviour as WalkBehaviour;
        walkBehaviour.onSurfaceMovement = onSurfaceMovement;
        walkBehaviour.onSurfaceTracker = onSurfaceTracker;
        walkBehaviour.allowSwimming = AllowSwimming;

        var landCreatureGravity = prefab.EnsureComponent<LandCreatureGravity>();
        landCreatureGravity.onSurfaceTracker = onSurfaceTracker;
        landCreatureGravity.forceLandMode = !AllowSwimming;
        landCreatureGravity.canGoInStasisUnderwater = WalkUnderwater;
        landCreatureGravity.applyDownforceUnderwater = WalkUnderwater;
        if (WalkUnderwater)
        {
            landCreatureGravity.underWaterGravity = 3f;
        }
        landCreatureGravity.liveMixin = components.liveMixin;
        landCreatureGravity.creatureRigidbody = components.rigidbody;
        landCreatureGravity.worldForces = components.worldForces;
        landCreatureGravity.bodyCollider = prefab.GetComponent<SphereCollider>();
        landCreatureGravity.aboveWaterGravity = AboveWaterGravity;
        landCreatureGravity.underWaterGravity = UnderwaterGravity;

        MoveOnSurface moveOnSurface = null;
        if (MoveOnSurfaceSettings.evaluatePriority > 0f)
        {
            moveOnSurface = prefab.AddComponent<MoveOnSurface>();
            moveOnSurface.evaluatePriority = MoveOnSurfaceSettings.evaluatePriority;
            moveOnSurface.updateTargetInterval = MoveOnSurfaceSettings.updateTargetInterval;
            moveOnSurface.updateTargetRandomInterval = MoveOnSurfaceSettings.updateTargetRandomInterval;
            moveOnSurface.moveVelocity = MoveOnSurfaceSettings.moveVelocity;
            moveOnSurface.moveRadius = MoveOnSurfaceSettings.moveRadius;
            moveOnSurface.moveOnWalls = MoveOnSurfaceSettings.moveOnWalls;
            moveOnSurface.onSurfaceTracker = onSurfaceTracker;
            moveOnSurface.walkBehaviour = walkBehaviour;
            moveOnSurface.onSurfaceMovement = onSurfaceMovement;
        }

        #region SwimWalkCreatureController
        var swimWalkCreatureController = prefab.EnsureComponent<SwimWalkCreatureController>();
        swimWalkCreatureController.creature = components.creature;
        swimWalkCreatureController.useRigidbody = components.rigidbody;
        swimWalkCreatureController.onSurfaceTracker = onSurfaceTracker;
        swimWalkCreatureController.walkBehaviour = walkBehaviour;
        swimWalkCreatureController.locomotion = components.locomotion;
        swimWalkCreatureController.animator = prefab.GetComponentInChildren<Animator>();
        swimWalkCreatureController.animateByVelocity = components.animateByVelocity;
        swimWalkCreatureController.landCreatureGravity = landCreatureGravity;

        // walking or swimming specific components
        var walkBehaviours = this.walkBehaviours;
        if (moveOnSurface != null)
        {
            walkBehaviours.Add(moveOnSurface);
        }
        swimWalkCreatureController.walkBehaviours = walkBehaviours.ToArray();

        var swimBehaviours = this.swimBehaviours;
        if (components.swimRandom != null)
        {
            swimBehaviours.Add(components.swimRandom);
        }
        var avoidObstacles = prefab.GetComponent<AvoidObstacles>();
        if (avoidObstacles != null)
        {
            swimBehaviours.Add(avoidObstacles);
        }
        swimWalkCreatureController.swimBehaviours = swimBehaviours.ToArray();

        swimWalkCreatureController.creatureType = TechType;
        #endregion

        components.locomotion.canWalkOnSurface = true;
    }

    /// <summary>
    /// No need to override this.
    /// </summary>
    public sealed override bool UseWalkBehaviour => true;

    /// <summary>
    /// If set to true, the creature will swim and walk. If set to false, the creature will be forced to walk.
    /// </summary>
    public abstract bool AllowSwimming { get; }

    /// <summary>
    /// Can the creature walk underwater?
    /// </summary>
    public abstract bool WalkUnderwater { get; }

    /// <summary>
    /// Settings related to moving on surfaces.
    /// </summary>
    public abstract MoveOnSurfaceSettings MoveOnSurfaceSettings { get; }

    /// <summary>
    /// The maximum angle (in degrees) for something to be considered a surface. Values close to 90° result in the creature being forced to stand upright and never being able to walk. (0-180)
    /// </summary>
    public abstract float MaxSurfaceAngle { get; }

    /// <summary>
    /// Forces <paramref name="behaviour"/> to only work while on land.
    /// </summary>
    protected void AddWalkBehaviour(Behaviour behaviour)
    {
        walkBehaviours.Add(behaviour);
    }

    /// <summary>
    /// Forces <paramref name="behaviour"/> to only work while in water.
    /// </summary>
    protected void AddSwimBehaviour(Behaviour behaviour)
    {
        swimBehaviours.Add(behaviour);
    }

    private List<Behaviour> walkBehaviours = new List<Behaviour>();

    private List<Behaviour> swimBehaviours = new List<Behaviour>();
}
#endif