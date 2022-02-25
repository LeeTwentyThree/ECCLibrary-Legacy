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
        landCreatureGravity.liveMixin = components.liveMixin;
        landCreatureGravity.creatureRigidbody = components.rigidbody;
        landCreatureGravity.worldForces = components.worldForces;
        landCreatureGravity.bodyCollider = prefab.GetComponent<SphereCollider>();
        landCreatureGravity.aboveWaterGravity = AboveWaterGravity;
        landCreatureGravity.underWaterGravity = UnderwaterGravity;

        var swimWalkCreatureController = prefab.EnsureComponent<SwimWalkCreatureController>();
        swimWalkCreatureController.creature = components.creature;
        swimWalkCreatureController.useRigidbody = components.rigidbody;
        swimWalkCreatureController.onSurfaceTracker = onSurfaceTracker;
        swimWalkCreatureController.walkBehaviour = walkBehaviour;
        swimWalkCreatureController.locomotion = components.locomotion;
        swimWalkCreatureController.animator = prefab.GetComponentInChildren<Animator>();
        swimWalkCreatureController.animateByVelocity = components.animateByVelocity;
        swimWalkCreatureController.landCreatureGravity = landCreatureGravity;
        swimWalkCreatureController.walkBehaviours = new Behaviour[] { walkBehaviour };
        swimWalkCreatureController.swimBehaviours = new Behaviour[] { walkBehaviour };
        swimWalkCreatureController.creatureType = TechType;
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
}
#endif