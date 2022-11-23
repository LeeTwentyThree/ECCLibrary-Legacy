using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Handlers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HarmonyLib;
using ECCLibrary.Internal;
using UWE;
#if BZ
using UnityEngine.AddressableAssets;
#endif
#if SN1
using Sprite = Atlas.Sprite;
#endif

namespace ECCLibrary;

/// <summary>
/// The base class for all ECCLibrary creatures.
/// </summary>
public abstract class CreatureAsset : Spawnable
{
    private GameObject model;
    private Sprite sprite;

    /// <summary>
    /// The prefab for this Creature. Edit this from the AddCustomBehaviour override.
    /// </summary>
    protected GameObject prefab;

    static GameObject electricalDamagePrefab;

    static GameObject damageEffectPrefab;

    static GameObject deathEffectPrefab;
#if BZ
        private WaterParkCreatureData myWaterParkData;
#endif

    /// <summary>
    /// Creates a new instance of a CreatureAsset.
    /// </summary>
    /// <param name="classId">The ClassID / TechType. Example: 'ReaperLeviathan'. Should not match an existing creature.</param>
    /// <param name="friendlyName">The name seen in-game. Example: 'Reaper Leviathan'.</param>
    /// <param name="description">The description/tooltip seen in the inventory.</param>
    /// <param name="model">The GameObject to be converted to a Creature.</param>
    /// <param name="spriteTexture">The image seen in the inventory/</param>
    public CreatureAsset(string classId, string friendlyName, string description, GameObject model, Texture2D spriteTexture) : base(classId, friendlyName, description)
    {
        this.model = model;
        if (spriteTexture != null)
        {
            sprite = ImageUtils.LoadSpriteFromTexture(spriteTexture);
        }
        OnFinishedPatching += () =>
        {
#if SN1
                WaterParkCreature.waterParkCreatureParameters.Add(TechType, WaterParkParameters);
#else
                myWaterParkData = ScriptableObject.CreateInstance<WaterParkCreatureData>();
                myWaterParkData.initialSize = WaterParkParameters.initialSize;
                myWaterParkData.maxSize = WaterParkParameters.maxSize;
                myWaterParkData.outsideSize = WaterParkParameters.outsideSize;
                myWaterParkData.daysToGrow = WaterParkParameters.daysToGrow;
                myWaterParkData.canBreed = WaterParkParameters.canBreed;
                myWaterParkData.isPickupableOutside = WaterParkParameters.isPickupableOutside;
                myWaterParkData.eggOrChildPrefab = WaterParkParameters.eggOrChildPrefab;
                myWaterParkData.adultPrefab = WaterParkParameters.adultPrefab;
#endif
                BioReactorHandler.SetBioReactorCharge(TechType, BioReactorCharge);
            ECCHelpers.PatchBehaviorType(TechType, BehaviourType);
            if (Pickupable)
            {
                ECCHelpers.PatchEquipmentType(TechType, EquipmentType.Hand);
            }
            if (AcidImmune)
            {
                DamageSystem.acidImmune.AddItem(TechType);
            }
            ScannableSettings.AttemptPatch(this, GetEncyTitle, GetEncyDesc);
#if SN1
                ECCHelpers.PatchItemSounds(TechType, ItemSounds);
#endif
                LanguageHandler.SetLanguageLine(string.Format("{0}_DiscoverMessage", ClassID), "NEW LIFEFORM DISCOVERED");
            PostPatch();
        };
    }

#if SN1
    private static void ValidatePrefabs()
    {
        if (electricalDamagePrefab != null && damageEffectPrefab != null && deathEffectPrefab != null)
        {
            return;
        }
        GameObject reaperLeviathan = Resources.Load<GameObject>("WorldEntities/Creatures/ReaperLeviathan");
        if (reaperLeviathan == null)
        {
            return;
        }
        electricalDamagePrefab = reaperLeviathan.GetComponent<LiveMixin>().data.electricalDamageEffect;
        damageEffectPrefab = reaperLeviathan.GetComponent<LiveMixin>().data.damageEffect;
        deathEffectPrefab = reaperLeviathan.GetComponent<LiveMixin>().data.deathEffect;
    }
#endif

#if BZ
    private static void ValidatePrefabs()
    {
        if (electricalDamagePrefab != null && damageEffectPrefab != null && deathEffectPrefab != null)
        {
            return;
        }
        //logic for getting the prefabs goes here
    }
#endif

#if SN1
    /// <summary>
    /// Do not override this method unless necessary. Override 'AddCustomBehaviour' instead.
    /// </summary>
    /// <returns></returns>
    public override GameObject GetGameObject()
    {
        if (!prefabPropertiesCached)
        {
            CachePrefabProperties();
        }
        if (prefab == null)
        {
            SetupPrefab(out CreatureComponents components);
            AddCustomBehaviour(components);
            CompletePrefab(components);
        }
        return prefab;
    }
#endif
    /// <summary>
    /// Do not override this method unless necessary. Override 'AddCustomBehaviour' instead.
    /// </summary>
    /// <returns></returns>
    public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
    {
        if (!prefabPropertiesCached)
        {
            CachePrefabProperties();
        }
        if (prefab == null)
        {
            SetupPrefab(out CreatureComponents components);
            ApplyInternalChanges(components);
            AddCustomBehaviour(components);
#if BZ
                foreach (CreatureAction action in prefab.GetComponentsInChildren<CreatureAction>())
                {
                    action.creature = components.creature;
                    action.swimBehaviour = components.swimBehaviour;
                }
#endif
            
            var task = PrefabDatabase.GetPrefabAsync("d8c7c300-cf01-448e-9bea-829b67ddfbbc");
            yield return task;
            task.TryGetPrefab(out var respawnerPrefab);
            components.creatureDeath.respawnerPrefab = respawnerPrefab;
            
            CompletePrefab(components);
        }
        yield return null;
        gameObject.Set(prefab);
    }
    private void SetupPrefab(out CreatureComponents creatureComponents)
    {
        EnsurePrefabSetupCorrectly(model, ClassID);
        prefab = GameObject.Instantiate(model);
        prefab.tag = "Creature";
        prefab.SetActive(false);
        creatureComponents = SetupNecessaryComponents();
        ECCHelpers.ApplySNShaders(prefab, MaterialSettings);
    }
    static void EnsurePrefabSetupCorrectly(GameObject model, string name)
    {
        if (model == null)
        {
            ECCLog.AddMessage("ECC Warning: No model for creature {0} found.", name);
        }
        else
        {
            if (model.GetComponentInChildren<Animator>() == null)
            {
                ECCLog.AddMessage("ECC Warning: Model for creature {0} needs an Animator somewhere in its hierarchy.", name);
            }
            if (model.GetComponentInChildren<Collider>() == null)
            {
                ECCLog.AddMessage("ECC Warning: Model for creature {0} has no collider.", name);
            }
        }
    }
    private void CompletePrefab(CreatureComponents components)
    {

    }
    internal virtual void ApplyInternalChanges(CreatureComponents components)
    {

    }
    /// <summary>
    /// This is by default the 'sprite' defined in the constructor.
    /// </summary>
    /// <returns></returns>
    protected override Sprite GetItemSprite()
    {
        return sprite;
    }
    /// <summary>
    /// The settings based around spawning. Do not override this unless you have to.
    /// </summary>
    public override WorldEntityInfo EntityInfo => new WorldEntityInfo()
    {
        cellLevel = CellLevel,
        classId = ClassID,
        slotType = EntitySlot.Type.Creature,
        techType = TechType,
        localScale = Vector3.one
    };
    /// <summary>
    /// Any changes to be done after the AssetClass is patched.
    /// </summary>
    protected virtual void PostPatch()
    {

    }
    /// <summary>
    /// Adds a melee attack.
    /// </summary>
    /// <param name="mouth">The mouth object (should have a trigger collider).</param>
    /// <param name="biteInterval">Min time between attacks.</param>
    /// <param name="damage">Damage of the attack.</param>
    /// <param name="biteSoundPrefix">Creates a clip pool that can use ANY mod audio clips that start with this text.</param>
    /// <param name="consumeWholeHealthThreshold">If the creature has this much health or less, it is deleted from existence when chewed on.</param>
    /// <param name="regurgitateLater">If true, the creature will spit out anything swallowed whole.</param>
    /// <param name="components"></param>
    /// <returns></returns>
    public MeleeAttack_New AddMeleeAttack(GameObject mouth, float biteInterval, float damage, string biteSoundPrefix, float consumeWholeHealthThreshold, bool regurgitateLater, CreatureComponents components)
    {
        OnTouch onTouch = mouth.EnsureComponent<OnTouch>();
        onTouch.gameObject.EnsureComponent<Rigidbody>().isKinematic = true;
        MeleeAttack_New meleeAttack = prefab.AddComponent<MeleeAttack_New>();
        meleeAttack.mouth = mouth;
        meleeAttack.creature = components.creature;
        meleeAttack.liveMixin = components.liveMixin;
        meleeAttack.animator = components.creature.GetAnimator();
        meleeAttack.biteInterval = biteInterval;
        meleeAttack.lastTarget = components.lastTarget;
        meleeAttack.biteDamage = damage;
        meleeAttack.eatHappyIncrement = 0f;
        meleeAttack.eatHungerDecrement = 0.4f;
        meleeAttack.biteAggressionThreshold = 0f;
        meleeAttack.biteAggressionDecrement = 0.2f;
        meleeAttack.onTouch = onTouch;
        meleeAttack.biteSoundPrefix = biteSoundPrefix;
        meleeAttack.consumeWholeHealthThreshold = consumeWholeHealthThreshold;
        meleeAttack.regurgitate = regurgitateLater;
        return meleeAttack;
    }

    /// <summary>
    /// A messy method that does most of the work.
    /// </summary>
    /// <returns></returns>
    private CreatureComponents SetupNecessaryComponents()
    {
        ValidatePrefabs();

        CreatureComponents components = new CreatureComponents();
        components.prefabIdentifier = prefab.EnsureComponent<PrefabIdentifier>();
        components.prefabIdentifier.ClassId = ClassID;

        components.techTag = prefab.EnsureComponent<TechTag>();
        components.techTag.type = TechType;

        components.largeWorldEntity = prefab.EnsureComponent<LargeWorldEntity>();
        components.largeWorldEntity.cellLevel = _cellLevel;
#if BZ
        components.largeWorldEntity.fadeRenderers = new List<Renderer>(prefab.GetComponentsInChildren<Renderer>());
#endif

        components.entityTag = prefab.EnsureComponent<EntityTag>();
        components.entityTag.slotType = EntitySlot.Type.Creature;

        components.skyApplier = prefab.AddComponent<SkyApplier>();
        components.skyApplier.renderers = prefab.GetComponentsInChildren<Renderer>(true);

        components.ecoTarget = prefab.AddComponent<EcoTarget>();
        components.ecoTarget.type = _ecoTargetType;

        components.vfxSurface = prefab.EnsureComponent<VFXSurface>();
        components.vfxSurface.surfaceType = _surfaceType;

        var physicMaterial = _physicMaterial;
        if (physicMaterial != null)
        {
            var colliders = prefab.GetComponentsInChildren<Collider>();
            foreach (var collider in colliders)
            {
                collider.sharedMaterial = physicMaterial;
            }
        }

        components.behaviourLOD = prefab.EnsureComponent<BehaviourLOD>();
        components.behaviourLOD.veryCloseThreshold = _behaviourLODSettings.Close;
        components.behaviourLOD.closeThreshold = _behaviourLODSettings.Close;
        components.behaviourLOD.farThreshold = _behaviourLODSettings.Far;

        components.rigidbody = prefab.EnsureComponent<Rigidbody>();
        components.rigidbody.useGravity = false;
        components.rigidbody.mass = _mass;

        components.locomotion = prefab.AddComponent<Locomotion>();
        components.locomotion.useRigidbody = components.rigidbody;
        components.locomotion.forwardRotationSpeed = _turnSpeedHorizontal;
        components.locomotion.upRotationSpeed = _turnSpeedVertical;
#if BZ
        components.locomotion.levelOfDetail = components.behaviourLOD;
#endif
        components.splineFollowing = prefab.AddComponent<SplineFollowing>();
        components.splineFollowing.respectLOD = false;
        components.splineFollowing.locomotion = components.locomotion;
#if BZ
        components.splineFollowing.useRigidbody = components.rigidbody;
        components.splineFollowing.locomotion = components.locomotion;
#endif
        if (UseWalkBehaviour)
        {
            prefab.AddComponent<OnSurfaceMovement>();
        }
        components.swimBehaviour = UseWalkBehaviour ? prefab.AddComponent<WalkBehaviour>() : prefab.AddComponent<SwimBehaviour>();
        components.swimBehaviour.splineFollowing = components.splineFollowing;
        components.swimBehaviour.turnSpeed = TurnSpeed;

#if SN1
        components.lastScarePosition = prefab.AddComponent<LastScarePosition>();
#endif
        components.worldForces = prefab.EnsureComponent<WorldForces>();
        components.worldForces.useRigidbody = components.rigidbody;
        components.worldForces.handleGravity = true;
        components.worldForces.underwaterGravity = _underwaterGravity;
        components.worldForces.aboveWaterGravity = _aboveWaterGravity;
        components.worldForces.underwaterDrag = _underwaterDrag;

        components.liveMixin = prefab.EnsureComponent<LiveMixin>();
        components.liveMixin.data = ECCHelpers.CreateNewLiveMixinData();
        components.liveMixin.data.electricalDamageEffect = electricalDamagePrefab;
        if (_useBloodEffects)
        {
            components.liveMixin.data.damageEffect = damageEffectPrefab;
            components.liveMixin.data.deathEffect = deathEffectPrefab;
        }
        SetLiveMixinData(ref components.liveMixin.data);
        var maxHealth = components.liveMixin.data.maxHealth;
        if (maxHealth <= 0f)
        {
            ECCLog.AddMessage("Warning: Creatures should not have a max health of zero or below.");
        }
        components.liveMixin.health = maxHealth;

#if BZ
        components.liveMixin.tempDamage = -1f;
#endif

        components.creature = prefab.AddComponent<Creature>();

        components.creature.Aggression = new AggressionCreatureTrait() { value = 0, falloff = _traitsSettings.AggressionDecreaseRate };
        components.creature.Hunger = new CreatureTrait() { value = 0, falloff = -_traitsSettings.HungerIncreaseRate };
        components.creature.Scared = new CreatureTrait() { value = 0, falloff = _traitsSettings.ScaredDecreaseRate };

        components.creature.liveMixin = components.liveMixin;
        components.creature.traitsAnimator = components.creature.GetComponentInChildren<Animator>();
        components.creature.sizeDistribution = _sizeDistribution;
        components.creature.eyeFOV = _eyeFov;

        RoarAbility roar = null;
        if (!string.IsNullOrEmpty(_roarAbilitySettings.AudioClipPrefix))
        {
            roar = prefab.AddComponent<RoarAbility>();
            roar.minRoarDistance = _roarAbilitySettings.MinRoarDistance;
            roar.maxRoarDistance = _roarAbilitySettings.MaxRoarDistance;
            roar.animationName = _roarAbilitySettings.AnimationName;
            roar.clipPrefix = _roarAbilitySettings.AudioClipPrefix;
            roar.createCurrent = _roarAbilitySettings.CreateCurrentOnRoar;
            roar.currentStrength = _roarAbilitySettings.CurrentStrength;

            if (_roarAbilitySettings.RoarActionPriority > 0f)
            {
                RoarRandomAction roarAction = prefab.AddComponent<RoarRandomAction>();
                roarAction.roarIntervalMin = _roarAbilitySettings.MinTimeBetweenRoars;
                roarAction.roarIntervalMax = _roarAbilitySettings.MaxTimeBetweenRoars;
                roarAction.evaluatePriority = _roarAbilitySettings.RoarActionPriority;
            }
        }
#if SN1
        components.lastTarget = prefab.AddComponent<LastTarget_New>();
        components.lastTarget.roar = roar;
#else
        components.lastTarget = prefab.AddComponent<LastTarget>();
#endif
        if (_aggressivenessToSmallVehicles.Aggression > 0f)
        {
            AggressiveToPilotingVehicle atpv = prefab.AddComponent<AggressiveToPilotingVehicle>();
            atpv.aggressionPerSecond = _aggressivenessToSmallVehicles.Aggression;
            atpv.range = _aggressivenessToSmallVehicles.MaxRange;
            atpv.creature = components.creature;
            atpv.lastTarget = components.lastTarget;
        }
        if (_attackSettings.EvaluatePriority > 0f)
        {
            AttackLastTarget actionAtkLastTarget = prefab.AddComponent<AttackLastTarget>();
            actionAtkLastTarget.evaluatePriority = _attackSettings.EvaluatePriority;
            actionAtkLastTarget.swimVelocity = _attackSettings.ChargeVelocity;
            actionAtkLastTarget.aggressionThreshold = 0.02f;
            actionAtkLastTarget.minAttackDuration = _attackSettings.MinAttackLength;
            actionAtkLastTarget.maxAttackDuration = _attackSettings.MaxAttackLength;
            actionAtkLastTarget.pauseInterval = _attackSettings.AttackInterval;
            actionAtkLastTarget.rememberTargetTime = _attackSettings.RememberTargetTime;
            actionAtkLastTarget.priorityMultiplier = ECCHelpers.Curve_Flat();
            actionAtkLastTarget.lastTarget = components.lastTarget;
        }
        components.swimRandom = prefab.AddComponent<SwimRandom>();
        components.swimRandom.swimRadius = _swimRandomSettings.SwimRadius;
        components.swimRandom.swimVelocity = _swimRandomSettings.SwimVelocity;
        components.swimRandom.swimInterval = _swimRandomSettings.SwimInterval;
        components.swimRandom.evaluatePriority = _swimRandomSettings.EvaluatePriority;
        components.swimRandom.priorityMultiplier = ECCHelpers.Curve_Flat();
        if (_avoidObstaclesSettings.evaluatePriority > 0f)
        {
            AvoidObstacles avoidObstacles = prefab.AddComponent<AvoidObstacles>();
            avoidObstacles.avoidTerrainOnly = _avoidObstaclesSettings.terrainOnly;
            avoidObstacles.avoidanceDistance = _avoidObstaclesSettings.avoidDistance;
            avoidObstacles.scanDistance = _avoidObstaclesSettings.avoidDistance;
            avoidObstacles.priorityMultiplier = ECCHelpers.Curve_Flat();
            avoidObstacles.evaluatePriority = _avoidObstaclesSettings.evaluatePriority;
            avoidObstacles.swimVelocity = _swimRandomSettings.SwimVelocity;
        }
#if SN1
        if (_canBeInfected)
        {
            components.infectedMixin = prefab.AddComponent<InfectedMixin>();
            components.infectedMixin.renderers = prefab.GetComponentsInChildren<Renderer>(true);
        }
#endif
        if (_pickupable)
        {
            components.pickupable = prefab.EnsureComponent<Pickupable>();
            if (_viewModelSettings.ReferenceHoldingAnimation != TechType.None)
            {
                HeldFish heldFish = prefab.EnsureComponent<HeldFish>();
                heldFish.animationName = _viewModelSettings.ReferenceHoldingAnimation.ToString().ToLower();
                heldFish.mainCollider = prefab.GetComponent<Collider>();
                heldFish.pickupable = components.pickupable;
            }
            else
            {
                ECCLog.AddMessage("Creature {0} is Pickupable but has no applied ViewModelSettings. This is necessary to be held and placed in an aquarium.", TechType);
            }
            if (!string.IsNullOrEmpty(_viewModelSettings.ViewModelName))
            {
                var fpsModel = prefab.EnsureComponent<FPModel>();
                fpsModel.propModel = prefab.SearchChild(_viewModelSettings.WorldModelName);
                if (fpsModel.propModel == null)
                {
                    ECCLog.AddMessage("Error finding World model. No child of name {0} exists in the hierarchy of item {1}.", _viewModelSettings.WorldModelName, TechType);
                }
                else
                {
                    prefab.EnsureComponent<AquariumFish>().model = fpsModel.propModel;
                    if (fpsModel.propModel.GetComponentInChildren<AnimateByVelocity>() == null)
                    {
                        AnimateByVelocity animateByVelocity = fpsModel.propModel.AddComponent<AnimateByVelocity>();
                        animateByVelocity.animator = components.creature.GetAnimator();
                        animateByVelocity.animationMoveMaxSpeed = _maxVelocityForSpeedParameter;
                        animateByVelocity.useStrafeAnimation = _animateByVelocitySettings.UseStrafeAnimation;
                        animateByVelocity.animationMaxPitch = _animateByVelocitySettings.AnimationMaxPitch;
                        animateByVelocity.animationMaxTilt = _animateByVelocitySettings.AnimationMaxTilt;
                        animateByVelocity.dampTime = _animateByVelocitySettings.DampTime;
                        animateByVelocity.levelOfDetail = components.behaviourLOD;
                        animateByVelocity.rootGameObject = fpsModel.propModel;
                    }
                }
                fpsModel.viewModel = prefab.SearchChild(_viewModelSettings.ViewModelName);
                if (fpsModel.viewModel == null)
                {
                    ECCLog.AddMessage("Error finding View model. No child of name {0} exists in the hierarchy of item {1}.", ViewModelSettings.ViewModelName, TechType);
                }
            }
        }
        Eatable eatable = null;
        if (_eatableSettings.CanBeEaten)
        {
            eatable = _eatableSettings.MakeItemEatable(prefab);
        }
        if (_swimInSchoolSettings.EvaluatePriority > 0f)
        {
            SwimInSchool swimInSchool = prefab.AddComponent<SwimInSchool>();
            swimInSchool.priorityMultiplier = ECCHelpers.Curve_Flat();
            swimInSchool.evaluatePriority = _swimInSchoolSettings.EvaluatePriority;
            swimInSchool.swimInterval = _swimInSchoolSettings.SwimInterval;
            swimInSchool.swimVelocity = _swimInSchoolSettings.SwimVelocity;
            swimInSchool.schoolSize = _swimInSchoolSettings.SchoolSize;
            swimInSchool.percentFindLeaderRespond = _swimInSchoolSettings.FindLeaderChance;
            swimInSchool.chanceLoseLeader = _swimInSchoolSettings.LoseLeaderChance;
            swimInSchool.kBreakDistance = _swimInSchoolSettings.BreakDistance;
        }
        components.animateByVelocity = prefab.AddComponent<AnimateByVelocity>();
        components.animateByVelocity.animator = components.creature.GetAnimator();
        components.animateByVelocity.animationMoveMaxSpeed = _maxVelocityForSpeedParameter;
        components.animateByVelocity.useStrafeAnimation = _animateByVelocitySettings.UseStrafeAnimation;
        components.animateByVelocity.animationMaxPitch = _animateByVelocitySettings.AnimationMaxPitch;
        components.animateByVelocity.animationMaxTilt = _animateByVelocitySettings.AnimationMaxTilt;
        components.animateByVelocity.dampTime = _animateByVelocitySettings.DampTime;
        components.animateByVelocity.levelOfDetail = components.behaviourLOD;
        components.animateByVelocity.rootGameObject = prefab;

        components.creatureDeath = prefab.AddComponent<CreatureDeath>();
        components.creatureDeath.useRigidbody = components.rigidbody;
        components.creatureDeath.liveMixin = components.liveMixin;
        components.creatureDeath.eatable = eatable;
        components.creatureDeath.respawnInterval = _respawnSettings.RespawnDelay;
        components.creatureDeath.respawn = _respawnSettings.CanRespawn;
#if SN1
        PrefabDatabase.TryGetPrefab("d8c7c300-cf01-448e-9bea-829b67ddfbbc", out var respawnerPrefab);
        components.creatureDeath.respawnerPrefab = respawnerPrefab;
#endif
        var deadAnimationOnEnable = prefab.AddComponent<DeadAnimationOnEnable>();
        deadAnimationOnEnable.enabled = false;
        deadAnimationOnEnable.animator = components.creature.GetAnimator();
        deadAnimationOnEnable.liveMixin = components.liveMixin;
        deadAnimationOnEnable.enabled = true;
        if (_stayAtLeashSettings.EvaluatePriority > 0f)
        {
            var stayAtLeash = prefab.AddComponent<StayAtLeashPosition>();
            stayAtLeash.evaluatePriority = _stayAtLeashSettings.EvaluatePriority;
            stayAtLeash.priorityMultiplier = ECCHelpers.Curve_Flat(1f);
            stayAtLeash.swimVelocity = _swimRandomSettings.SwimVelocity;
            stayAtLeash.leashDistance = _stayAtLeashSettings.MaxDistance;
        }
        if (_scannerRoomScannable)
        {
            ECCHelpers.MakeObjectScannerRoomScannable(prefab, true);
        }

        return components;
    }
    /// <summary>
    /// Makes the creature aggressive to creatures matching ecoTarget.
    /// </summary>
    /// <param name="maxRange">The absolute max range.</param>
    /// <param name="maxSearchRings">More rings means further search distance.</param>
    /// <param name="ecoTarget">What kind of creatures it will attack.</param>
    /// <param name="hungerThreshold">How hungry it has to be to attack something.</param>
    /// <param name="aggressionSpeed">How fast it becomes aggravated.</param>
    /// <returns></returns>
    protected AggressiveWhenSeeTarget MakeAggressiveTo(float maxRange, int maxSearchRings, EcoTargetType ecoTarget, float hungerThreshold, float aggressionSpeed)
    {
        AggressiveWhenSeeTarget aggressiveWhenSeeTarget = prefab.AddComponent<AggressiveWhenSeeTarget>();
        aggressiveWhenSeeTarget.maxRangeMultiplier = ECCHelpers.Curve_Flat();
        aggressiveWhenSeeTarget.distanceAggressionMultiplier = ECCHelpers.Curve_Flat();
        aggressiveWhenSeeTarget.maxRangeScalar = maxRange;
        aggressiveWhenSeeTarget.maxSearchRings = maxSearchRings;
#if SN1
        aggressiveWhenSeeTarget.lastScarePosition = prefab.GetComponent<LastScarePosition>();
#endif
        aggressiveWhenSeeTarget.lastTarget = prefab.GetComponent<LastTarget>();
        aggressiveWhenSeeTarget.targetType = ecoTarget;
        aggressiveWhenSeeTarget.hungerThreshold = hungerThreshold;
        aggressiveWhenSeeTarget.aggressionPerSecond = aggressionSpeed;
        return aggressiveWhenSeeTarget;
    }
    /// <summary>
    /// Creates a trail, which is used for procedural animation of tail-like objects.
    /// </summary>
    /// <param name="trailParent">The root segment, which does not move. The first child of this object and all children of the first child are used for the trail.</param>
    /// <param name="components">The reference to all the components.</param>
    /// <param name="segmentSnapSpeed">How fast each segment snaps back into the default position. A higher value gives a more rigid appearance.</param>
    /// <param name="maxSegmentOffset">How far each segment can be from the original position.</param>
    /// <param name="multiplier">The total strength of the movement. A value too low or too high will break the trail completely.</param>
    protected TrailManager CreateTrail(GameObject trailParent, CreatureComponents components, float segmentSnapSpeed, float maxSegmentOffset = -1f, float multiplier = 1f)
    {
        trailParent.gameObject.SetActive(false);

        TrailManagerECC trail = trailParent.AddComponent<TrailManagerECC>();
        trail.trails = trailParent.transform.GetChild(0).GetComponentsInChildren<Transform>();
        trail.rootTransform = prefab.transform;
        trail.rootSegment = trail.transform;
        trail.levelOfDetail = components.behaviourLOD;
        trail.segmentSnapSpeed = segmentSnapSpeed;
        trail.maxSegmentOffset = maxSegmentOffset;
        trail.allowDisableOnScreen = false;
        AnimationCurve decreasing = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f * multiplier), new Keyframe(1f, 0.75f * multiplier) });
        trail.pitchMultiplier = decreasing;
        trail.rollMultiplier = decreasing;
        trail.yawMultiplier = decreasing;

        trailParent.gameObject.SetActive(true);
        return trail;
    }
    /// <summary>
    /// Creates a trail manager, which is used for procedural animation of tail-like objects.
    /// </summary>
    /// <param name="trailRoot">The root segment, which does not move.</param>
    /// <param name="trails">Any objects that are simulated.</param>
    /// <param name="components">The reference to all the components.</param>
    /// <param name="segmentSnapSpeed">How fast each segment snaps back into the default position. A higher value gives a more rigid appearance.</param>
    /// <param name="maxSegmentOffset">How far each segment can be from the original position.</param>
    protected TrailManager CreateTrail(GameObject trailRoot, Transform[] trails, CreatureComponents components, float segmentSnapSpeed, float maxSegmentOffset = -1f)
    {
        trailRoot.gameObject.SetActive(false);

        TrailManagerECC trail = trailRoot.AddComponent<TrailManagerECC>();
        trail.trails = trails;
        trail.rootTransform = prefab.transform;
        trail.rootSegment = trail.transform;
        trail.levelOfDetail = components.behaviourLOD;
        trail.segmentSnapSpeed = segmentSnapSpeed;
        trail.maxSegmentOffset = maxSegmentOffset;
        trail.allowDisableOnScreen = false;
        AnimationCurve decreasing = new AnimationCurve(new Keyframe[] { new Keyframe(0f, 0.25f), new Keyframe(1f, 0.75f) });
        trail.pitchMultiplier = decreasing;
        trail.rollMultiplier = decreasing;
        trail.yawMultiplier = decreasing;

        trailRoot.gameObject.SetActive(true);
        return trail;
    }

    #region Abstracts

    /// <summary>
    /// Add CreatureActions and things of that like here.
    /// </summary>
    /// <param name="components"></param>
    public abstract void AddCustomBehaviour(CreatureComponents components);

    /// <summary>
    /// Should match the EcoTargetType whenever possible. Does not do much on its own.
    /// </summary>
    public abstract BehaviourType BehaviourType
    {
        get;
    }

    /// <summary>
    /// How far this creature can be loaded in.
    /// </summary>
    public abstract LargeWorldEntity.CellLevel CellLevel
    {
        get;
    }

    /// <summary>
    /// Settings on random swimming.
    /// </summary>
    public abstract SwimRandomData SwimRandomSettings { get; }

    /// <summary>
    /// What other creatures recognize this creature as. Should match with BehaviourType whenever possible.
    /// </summary>
    public abstract EcoTargetType EcoTargetType
    {
        get;
    }

    /// <summary>
    /// This is where you set the LiveMixin data.
    /// </summary>
    /// <param name="liveMixinData"></param>
    public abstract void SetLiveMixinData(ref LiveMixinData liveMixinData);

    #endregion

    #region Overrideable
    /// <summary>
    /// Instance of the PhysicMaterial class to be used for the creature's colliders. By default uses <see cref="ECCHelpers.FrictionlessPhysicMaterial"/>
    /// </summary>
    public virtual PhysicMaterial PhysicMaterial
    {
        get
        {
            return ECCHelpers.FrictionlessPhysicMaterial;
        }
    }

    /// <summary>
    /// Settings related to more complex animations.
    /// </summary>
    public virtual AnimateByVelocityData AnimateByVelocitySettings
    {
        get
        {
            return new AnimateByVelocityData();
        }
    }
    /// <summary>
    /// Higher values make creatures move more slowly.
    /// </summary>
    public virtual float UnderwaterDrag
    {
        get
        {
            return 0.1f;
        }
    }
    /// <summary>
    /// Settings that determine basic attributes of the creature.
    /// </summary>
    public virtual CreatureTraitsData TraitsSettings
    {
        get
        {
            if (EnableAggression)
            {
                return new CreatureTraitsData(0.05f, 0.05f, 0.1f); //Aggressive fish are hungrier and less scared
            }
            else
            {
                return new CreatureTraitsData(0.01f, 0.1f, 0.25f);
            }
        }
    }

    /// <summary>
    /// The type of sound effects this creature will use in the Inventory. By default is ItemSoundsType.Fish.
    /// </summary>
    public virtual ItemSoundsType ItemSounds
    {
        get
        {
            return ItemSoundsType.Fish;
        }
    }

    /// <summary>
    /// Whether the LiveMixinData damageEffect and deathEffect is automatically set. True by default.
    /// </summary>
    public virtual bool UseBloodEffects => true;

    /// <summary>
    /// Settings related to how this creature is rendered.
    /// </summary>
    public virtual UBERMaterialProperties MaterialSettings
    {
        get
        {
            return new UBERMaterialProperties(8f, 1f);
        }
    }
    /// <summary>
    /// Settings related to how creatures respond to wandering out of their spawn zone.
    /// </summary>
    public virtual StayAtLeashData StayAtLeashSettings
    {
        get
        {
            return new StayAtLeashData();
        }
    }
    /// <summary>
    /// Settings related to fish that are held.
    /// </summary>
    public virtual HeldFishData ViewModelSettings
    {
        get
        {
            return new HeldFishData();
        }
    }

    /// <summary>
    /// Settings related to respawning. By default a creature respawns after 300 seconds.
    /// </summary>
    public virtual RespawnData RespawnSettings
    {
        get
        {
            return new RespawnData(true, 300f);
        }
    }
    /// <summary>
    /// Settings related to edibles.
    /// </summary>
    public virtual EatableData EatableSettings
    {
        get
        {
            return new EatableData();
        }
    }
    /// <summary>
    /// Used only for the 'speed' parameter on the Animator. By default is equal to SwimRandomSettings.SwimVelocity + 1.
    /// </summary>
    public virtual float MaxVelocityForSpeedParameter
    {
        get
        {
            return SwimRandomSettings.SwimVelocity + 1f;
        }
    }

    /// <summary>
    /// The horizontal turn speed. Default value is 0.6f.
    /// </summary>
    public virtual float TurnSpeedHorizontal
    {
        get
        {
            return 0.6f;
        }
    }
    /// <summary>
    /// The vertical turn speed (applies when looking up or down). Default value is 3f.
    /// </summary>
    public virtual float TurnSpeedVertical
    {
        get
        {
            return 3f;
        }
    }
    /// <summary>
    /// Whether the creature can be picked up and held, or not. Pickupable fish can also be placed in the Small Aquarium if ViewModelSettings are set correctly. False by default.
    /// </summary>
    public virtual bool Pickupable
    {
        get
        {
            return false;
        }
    }
    /// <summary>
    /// Whether the creature is immune to brine or not. False by default. Mainly used for Lost River creatures.
    /// </summary>
    public virtual bool AcidImmune
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// Possible sizes for this creature. Randomly picks a value in the range of 0 to 1. This value can not go above 1. Flat curve at 1 by default.
    /// </summary>
    public virtual AnimationCurve SizeDistribution
    {
        get
        {
            return ECCHelpers.Curve_Flat(1f);
        }
    }
    /// <summary>
    /// If set to true, the Scanner Room can scan for this creature. False by default.
    /// </summary>
    public virtual bool ScannerRoomScannable
    {
        get
        {
            return false;
        }
    }

#if SN1
    /// <summary>
    /// Whether this creature can randomly spawn with Kharaa symptoms. True by default.
    /// </summary>
    public virtual bool CanBeInfected
    {
        get
        {
            return true;
        }
    }
#endif

    /// <summary>
    /// Settings related to random roaring, using the Unity audio system. Please note it is recommended to create your own audio controller for the creature, using FMOD if possible.
    /// </summary>
    [System.Obsolete("It is recommended to create your own audio using SMLHelper's FMOD tools.")]
    public virtual RoarAbilityData RoarAbilitySettings { get; }

    /// <summary>
    /// How aggressive the creature is to small vehicles. If set to zero (default value), it will not be aggressive to small vehicles.
    /// </summary>
    public virtual SmallVehicleAggressivenessSettings AggressivenessToSmallVehicles
    {
        get
        {
            return new SmallVehicleAggressivenessSettings(0f, 0f);
        }
    }

    /// <summary>
    /// Settings based around how this creature attacks its LastTarget.
    /// </summary>
    public virtual AttackLastTargetSettings AttackSettings
    {
        get
        {
            return new AttackLastTargetSettings(0f, 0f, 0f, 0f, 0f, 0f);
        }
    }
#if SN1
    /// <summary>
    /// Settings for growth in Alien Containment.
    /// </summary>
    public virtual WaterParkCreatureParameters WaterParkParameters
    {
        get
        {
            return default;
        }
    }
#else
        /// <summary>
        /// Settings for growth in Alien Containment.
        /// </summary>
        public virtual WaterParkCreatureDataConstructor WaterParkParameters
        {
            get
            {
                return new WaterParkCreatureDataConstructor(0.02f, 0.3f, 0.5f, 1f, true, false);
            }
        }
        /// <summary>
        /// A BZ-specific struct for editing water park creature data.
        /// </summary>
        public struct WaterParkCreatureDataConstructor
        {
            internal float initialSize;
            internal float maxSize;
            internal float outsideSize;
            internal float daysToGrow;
            internal bool isPickupableOutside;
            internal bool canBreed;
            internal AssetReferenceGameObject eggOrChildPrefab;
            internal AssetReferenceGameObject adultPrefab;

            /// <summary>
            /// Constructor.
            /// </summary>
            /// <param name="initialSize">Size multiplier when born.</param>
            /// <param name="maxSize">Size multiplier after <paramref name="daysToGrow"/> days.</param>
            /// <param name="outsideSize">Size multiplier when released.</param>
            /// <param name="daysToGrow">How many days it takes to reach full size.</param>
            /// <param name="isPickupableOutside"></param>
            /// <param name="canBreed">If it can breed or not.</param>
            /// <param name="eggOrChildPrefab">The object created when they breed. Can be set to null if they do not breed.</param>
            /// <param name="adultPrefab">The prefab for growing up. Can be set to null if this is unwanted.</param>
            public WaterParkCreatureDataConstructor(float initialSize, float maxSize, float outsideSize, float daysToGrow, bool isPickupableOutside, bool canBreed = false, AssetReferenceGameObject eggOrChildPrefab = null, AssetReferenceGameObject adultPrefab = null)
            {
                this.initialSize = initialSize;
                this.maxSize = maxSize;
                this.outsideSize = outsideSize;
                this.daysToGrow = daysToGrow;
                this.isPickupableOutside = isPickupableOutside;
                this.canBreed = canBreed;
                this.eggOrChildPrefab = eggOrChildPrefab;
                this.adultPrefab = adultPrefab;
            }
        }
#endif

    /// <summary>
    /// The mass of the Rigidbody.
    /// </summary>
    public virtual float Mass
    {
        get
        {
            return 1f;
        }
    }
    /// <summary>
    /// Gravity above water. A positive value portrays downward force while a negative value portrays upward force.
    /// </summary>
    public virtual float AboveWaterGravity
    {
        get
        {
            return 9.81f;
        }
    }
    /// <summary>
    /// Gravity below water. A positive value portrays downward force while a negative value portrays upward force.
    /// </summary>
    public virtual float UnderwaterGravity
    {
        get
        {
            return 0f;
        }
    }
    /// <summary>
    /// The SurfaceType to be applied to the main collider.
    /// </summary>
    public virtual VFXSurfaceTypes SurfaceType
    {
        get
        {
            return VFXSurfaceTypes.organic;
        }
    }

    /// <summary>
    /// For big creatures, you might want to increase the values. Default values are 30, 60, 100.
    /// </summary>
    public virtual BehaviourLODLevelsStruct BehaviourLODSettings
    {
        get
        {
            return new BehaviourLODLevelsStruct(30f, 60f, 100f);
        }
    }

    /// <summary>
    /// The FOV used for detecting things, such as targets, on a scale from 0 to 1. Is 0.25 by default. A value of -1 means a given object is ALWAYS in view.
    /// </summary>
    public virtual float EyeFov
    {
        get
        {
            return 0.25f;
        }
    }

    /// <summary>
    /// Settings on how this creature will avoid terrain and/or obstacles. You will likely have to do some manual field tweaking in your AddCustomBehaviour to get the desired results.
    /// </summary>
    public virtual AvoidObstaclesData AvoidObstaclesSettings
    {
        get
        {
            return new AvoidObstaclesData(0f, false, 0f);
        }
    }

    /// <summary>
    /// Total power output of this creature. All ECC creatures can be put in the bioreactor.
    /// </summary>
    public virtual float BioReactorCharge
    {
        get
        {
            return 200f;
        }
    }

    /// <summary>
    /// Settings on shoaling behaviours.
    /// </summary>
    public virtual SwimInSchoolData SwimInSchoolSettings
    {
        get
        {
            return new SwimInSchoolData();
        }
    }

    #endregion

    #region Cached values to prevent creating unnecessary references for each spawned creature

    private LargeWorldEntity.CellLevel _cellLevel;
    private SwimRandomData _swimRandomSettings;
    private EcoTargetType _ecoTargetType;
    private PhysicMaterial _physicMaterial;
    private AnimateByVelocityData _animateByVelocitySettings;
    private float _underwaterDrag;
    private CreatureTraitsData _traitsSettings;
    private bool _useBloodEffects;
    private UBERMaterialProperties _materialSettings;
    private StayAtLeashData _stayAtLeashSettings;
    private HeldFishData _viewModelSettings;
    private RespawnData _respawnSettings;
    private EatableData _eatableSettings;
    private float _maxVelocityForSpeedParameter;
    private float _turnSpeedHorizontal;
    private float _turnSpeedVertical;
    private bool _pickupable;
    private AnimationCurve _sizeDistribution;
    private bool _scannerRoomScannable;
    private bool _canBeInfected;
    private RoarAbilityData _roarAbilitySettings;
    private SmallVehicleAggressivenessSettings _aggressivenessToSmallVehicles;
    private AttackLastTargetSettings _attackSettings;
    private float _mass;
    private float _aboveWaterGravity;
    private float _underwaterGravity;
    private VFXSurfaceTypes _surfaceType;
    private BehaviourLODLevelsStruct _behaviourLODSettings;
    private float _eyeFov;
    private AvoidObstaclesData _avoidObstaclesSettings;
    private SwimInSchoolData _swimInSchoolSettings;

    #endregion

    #region Caching Logic

    private bool prefabPropertiesCached = false;
    private void CachePrefabProperties() // caches properties that are called multiple times during prefab initialization
    {
        _cellLevel = CellLevel;
        _swimRandomSettings = SwimRandomSettings;
        _ecoTargetType = EcoTargetType;
        _physicMaterial = PhysicMaterial;
        _animateByVelocitySettings = AnimateByVelocitySettings;
        _underwaterDrag = UnderwaterDrag;
        _traitsSettings = TraitsSettings;
        _useBloodEffects = UseBloodEffects;
        _materialSettings = MaterialSettings;
        _stayAtLeashSettings = StayAtLeashSettings;
        _viewModelSettings = ViewModelSettings;
        _respawnSettings = RespawnSettings;
        _eatableSettings = EatableSettings;
        _maxVelocityForSpeedParameter = MaxVelocityForSpeedParameter;
        _turnSpeedHorizontal = TurnSpeedHorizontal;
        _turnSpeedVertical = TurnSpeedVertical;
        _pickupable = Pickupable;
        _sizeDistribution = SizeDistribution;
        _scannerRoomScannable = ScannerRoomScannable;
#if SN1
        _canBeInfected = CanBeInfected;
#endif
        _roarAbilitySettings = RoarAbilitySettings;
        _aggressivenessToSmallVehicles = AggressivenessToSmallVehicles;
        _attackSettings = AttackSettings;
        _mass = Mass;
        _aboveWaterGravity = AboveWaterGravity;
        _underwaterGravity = UnderwaterGravity;
        _surfaceType = SurfaceType;
        _behaviourLODSettings = BehaviourLODSettings;
        _eyeFov = EyeFov;
        _avoidObstaclesSettings = AvoidObstaclesSettings;
        _swimInSchoolSettings = SwimInSchoolSettings;

        prefabPropertiesCached = true;
    }

#endregion

#region Unused

    /// <summary>
    /// The creature prefab used for reference. Easier than declaring every stat manually.
    /// </summary>
    [System.Obsolete("Doesn't do anything anymore.")]
    public virtual TechType CreatureTraitsReference { get; }

    /// <summary>
    /// (Incorrectly) determines how fast the creature turns while swimming. Default value is 1f.
    /// </summary>
    [System.Obsolete("The field set by this value is not used. Please use the TurnSpeedHorizontal and TurnSpeedVertical properties instead.")]
    public virtual float TurnSpeed
    {
        get
        {
            return 1f;
        }
    }

    /// <summary>
    /// Used to determine whether AttackSettings and AggressivenessToSmallVehicles actually apply.
    /// </summary>
    [System.Obsolete("This property is redundant. The functionality was removed in ECC 1.1.5")]
    public virtual bool EnableAggression
    {
        get
        {
            return false;
        }
    }

    /// <summary>
    /// If set to true, the creature will use WalkBehaviour rather than the usual SwimBehaviour. Only override this if you know what you're doing! It will create errors otherwise. Automatically adds <see cref="OnSurfaceMovement"/> because it is required.
    /// </summary>
    public virtual bool UseWalkBehaviour
    {
        get
        {
            return false;
        }
    }

#endregion

#region Ency Related Overridables
    /// <summary>
    /// The Title of the encyclopedia entry.
    /// </summary>
    public virtual string GetEncyTitle
    {
        get
        {
            return FriendlyName;
        }
    }

    /// <summary>
    /// The description of the encyclopedia entry.
    /// </summary>
    public virtual string GetEncyDesc
    {
        get
        {
            return "no description";
        }
    }

    /// <summary>
    /// Settings related to the encyclopedia entry.
    /// </summary>
    public virtual ScannableItemData ScannableSettings
    {
        get
        {
            return new ScannableItemData();
        }
    }
#endregion

#region Structs
    /// <summary>
    /// First person view model settings.
    /// </summary>
    public struct HeldFishData
    {
        public TechType ReferenceHoldingAnimation;
        public string WorldModelName;
        public string ViewModelName;
        /// <summary>
        /// First person view model settings.
        /// </summary>
        /// <param name="referenceHoldingAnimation">The TechType that is used to find the holding animation.</param>
        /// <param name="worldModelName">The name of the model used for the World View, which must be a child of the object.</param>
        /// <param name="viewModelName">The name of the model used for the First Person View, which must be a child of the object.</param>
        public HeldFishData(TechType referenceHoldingAnimation, string worldModelName, string viewModelName)
        {
            ReferenceHoldingAnimation = referenceHoldingAnimation;
            WorldModelName = worldModelName;
            ViewModelName = viewModelName;
        }
        /// <summary>
        /// First person view model settings.
        /// </summary>
        /// <param name="referenceHoldingAnimation">The TechType that is used to find the holding animation.</param>
        public HeldFishData(TechType referenceHoldingAnimation)
        {
            ReferenceHoldingAnimation = referenceHoldingAnimation;
            WorldModelName = null;
            ViewModelName = null;
        }
    }
    /// <summary>
    /// Settings related to respawning.
    /// </summary>
    public struct RespawnData
    {
        public bool CanRespawn;
        public float RespawnDelay;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="respawns">Whether this creature can respawn when killed or not.</param>
        /// <param name="respawnDelay">How long it takes for this creature to respawn, after death.</param>
        public RespawnData(bool respawns, float respawnDelay = 300f)
        {
            CanRespawn = respawns;
            RespawnDelay = respawnDelay;
        }
    }
    public struct BehaviourLODLevelsStruct
    {
        public float VeryClose;
        public float Close;
        public float Far;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="veryClose">Beyond this distance some animations may be removed.</param>
        /// <param name="close">Beyond this distance some functionalities may be less precise.</param>
        /// <param name="far">Beyond this distance trail animations will no longer exist.</param>
        public BehaviourLODLevelsStruct(float veryClose, float close, float far)
        {
            VeryClose = veryClose;
            Close = close;
            Far = far;
        }
    }

    public struct AttackLastTargetSettings
    {
        public float EvaluatePriority;
        public float ChargeVelocity;
        public float MinAttackLength;
        public float MaxAttackLength;
        public float AttackInterval;
        public float RememberTargetTime;

        public AttackLastTargetSettings(float actionPriority, float chargeVelocity, float minAttackLength, float maxAttackLength, float attackInterval, float rememberTargetTime)
        {
            EvaluatePriority = actionPriority;
            ChargeVelocity = chargeVelocity;
            MinAttackLength = minAttackLength;
            MaxAttackLength = maxAttackLength;
            AttackInterval = attackInterval;
            RememberTargetTime = rememberTargetTime;
        }
    }

    public struct AvoidObstaclesData
    {
        public bool terrainOnly;
        public float avoidDistance;
        public float evaluatePriority;

        public AvoidObstaclesData(float actionPriority, bool terrainOnly, float avoidDistance)
        {
            this.evaluatePriority = actionPriority;
            this.terrainOnly = terrainOnly;
            this.avoidDistance = avoidDistance;
        }
    }

    public struct SmallVehicleAggressivenessSettings
    {
        public float Aggression;
        public float MaxRange;

        public SmallVehicleAggressivenessSettings(float aggression, float maxRange)
        {
            Aggression = aggression;
            MaxRange = maxRange;
        }
    }
    public struct RoarAbilityData
    {
        public bool CanRoar;
        public float MinRoarDistance;
        public float MaxRoarDistance;
        /// <summary>
        /// All sounds starting with AudioClipPrefix in the asset bundle have a possibility to be played.
        /// </summary>
        public string AudioClipPrefix;
        public string AnimationName;
        /// <summary>
        /// The name of the Animator trigger parameter.
        /// </summary>
        public float MinTimeBetweenRoars;
        public float MaxTimeBetweenRoars;
        public float RoarActionPriority;
        public bool CreateCurrentOnRoar;
        public float CurrentStrength;

        public RoarAbilityData(bool canRoar, float roarSoundFalloffStart, float roarSoundMaxDistance, string audioClipPrefix, string animationName, float roarActionPriority = 0.5f, float minTimeBetweenRoars = 4f, float maxTimeBetweenRoars = 8f)
        {
            CanRoar = canRoar;
            MinRoarDistance = roarSoundFalloffStart;
            MaxRoarDistance = roarSoundMaxDistance;
            AudioClipPrefix = audioClipPrefix;
            AnimationName = animationName;
            MinTimeBetweenRoars = minTimeBetweenRoars;
            MaxTimeBetweenRoars = maxTimeBetweenRoars;
            RoarActionPriority = roarActionPriority;
            CreateCurrentOnRoar = false;
            CurrentStrength = 0f;
        }

        public RoarAbilityData(bool canRoar, float roarSoundFalloffStart, float roarSoundMaxDistance, string audioClipPrefix, string animationName, bool createCurrent, float currentStrength, float roarActionPriority = 0.5f, float minTimeBetweenRoars = 4f, float maxTimeBetweenRoars = 8f)
        {
            CanRoar = canRoar;
            MinRoarDistance = roarSoundFalloffStart;
            MaxRoarDistance = roarSoundMaxDistance;
            AudioClipPrefix = audioClipPrefix;
            AnimationName = animationName;
            MinTimeBetweenRoars = minTimeBetweenRoars;
            MaxTimeBetweenRoars = maxTimeBetweenRoars;
            RoarActionPriority = roarActionPriority;
            CreateCurrentOnRoar = createCurrent;
            CurrentStrength = currentStrength;
        }
    }

    public struct SwimRandomData
    {
        public bool SwimRandomly;
        public Vector3 SwimRadius;
        public float SwimVelocity;
        public float SwimInterval;
        public float EvaluatePriority;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="swimRandomly">Whether this action is added to the creature or not.</param>
        /// <param name="swimRadius">The distance this creature can wander in each direction.</param>
        /// <param name="swimVelocity">The speed at which this creature swims idly.</param>
        /// <param name="swimInterval">The time in seconds between each change in direction.</param>
        /// <param name="priority">The priority for this inidividual CreatureAction.</param>
        public SwimRandomData(bool swimRandomly, Vector3 swimRadius, float swimVelocity, float swimInterval, float priority)
        {
            SwimRandomly = swimRandomly;
            SwimRadius = swimRadius;
            SwimVelocity = swimVelocity;
            SwimInterval = swimInterval;
            EvaluatePriority = priority;
        }
    }
    /// <summary>
    /// Stores references to basic components of the creature. Do not rely on all of these to exist.
    /// </summary>
    public struct CreatureComponents
    {
        public PrefabIdentifier prefabIdentifier;
        public TechTag techTag;
        public LargeWorldEntity largeWorldEntity;
        public EntityTag entityTag;
        public SkyApplier skyApplier;
        public Renderer renderer;
        public EcoTarget ecoTarget;
        public VFXSurface vfxSurface;
        public BehaviourLOD behaviourLOD;
        public Rigidbody rigidbody;
#if SN1
        public LastScarePosition lastScarePosition;
#endif
        public WorldForces worldForces;
        public Creature creature;
        public LiveMixin liveMixin;
#if SN1
        public LastTarget_New lastTarget;
#else
            public LastTarget lastTarget;
#endif
        public SwimBehaviour swimBehaviour;
        public Locomotion locomotion;
        public SplineFollowing splineFollowing;
        public SwimRandom swimRandom;
        public InfectedMixin infectedMixin;
        public Pickupable pickupable;
        public AnimateByVelocity animateByVelocity;
        public CreatureDeath creatureDeath;
    }
    public struct SwimInSchoolData
    {
        public float EvaluatePriority;
        public float SwimVelocity;
        public float SwimInterval;
        public float SchoolSize;
        public float BreakDistance;
        public float FindLeaderChance;
        public float LoseLeaderChance;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="evaluatePriority">The priority of this CreatureAction.</param>
        /// <param name="swimVelocity">The speed at which the creature moves while schooling.</param>
        /// <param name="swimInterval">The time between each "swim".</param>
        /// <param name="schoolSize">The max distance each fish can move from its leader.</param>
        /// <param name="breakDistance">The distance at which an individual fish breaks off from the school.</param>
        /// <param name="findLeaderChance">The chance, per attempt, for a school to be formed.</param>
        /// <param name="loseLeaderChance">The chance, each second, for a school to be broken.</param>
        public SwimInSchoolData(float evaluatePriority, float swimVelocity, float swimInterval, float schoolSize, float breakDistance, float findLeaderChance, float loseLeaderChance)
        {
            EvaluatePriority = evaluatePriority;
            SwimVelocity = swimVelocity;
            SwimInterval = swimInterval;
            SchoolSize = schoolSize;
            BreakDistance = breakDistance;
            FindLeaderChance = findLeaderChance;
            LoseLeaderChance = loseLeaderChance;
        }
    }
    /// <summary>
    /// Settings based around StayAtLeash.
    /// </summary>
    public struct StayAtLeashData
    {
        public float EvaluatePriority;
        public float MaxDistance;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="evaluatePriority">The priority of the action when applicable.</param>
        /// <param name="maxDistance">The distance at which the creature will attempt to perform this action.</param>
        public StayAtLeashData(float evaluatePriority, float maxDistance)
        {
            EvaluatePriority = evaluatePriority;
            MaxDistance = maxDistance;
        }
    }
    /// <summary>
    /// Data related to the AnimateByVelocity component. Note that <see cref="CreatureAsset.MaxVelocityForSpeedParameter"/> is also related to this component. It was created before this struct existed, so is separate.
    /// </summary>
    public struct AnimateByVelocityData
    {
        internal bool UseStrafeAnimation;
        internal float AnimationMaxPitch;
        internal float AnimationMaxTilt;
        internal float DampTime;

        /// <summary>
        /// Constructor for these settings.
        /// </summary>
        /// <param name="useStrafeAnimation">Strafe animation consists of Up, Down, Left, Right, Forward, and Backwards animations, always relative to the creature's current rotation. The parameters used by this are 'speed_x'. 'speed_y', and 'speed_z', on a scale from -1 to 1. False by default.</param>
        /// <param name="animationMaxPitch">Pitch can be described by looking up and down. The parameter for pitch 'pitch' and is always on a scale from -1 to 1. When the creature has rotated by a pitch of <paramref name="animationMaxPitch"/> in one way, it will equal 1. If it rotated the opposite direction the same amount, it wouls equal -1.</param>
        /// <param name="animationMaxTilt">In this case, tilt is rotating left and right. This parameter has the same rules, basically, as <paramref name="animationMaxPitch"/>.</param>
        /// <param name="dampTime">A longer damp time means it takes longer for these strafe, pitch, and tilt animations to take effect.</param>
        public AnimateByVelocityData(bool useStrafeAnimation, float animationMaxPitch = 30f, float animationMaxTilt = 45f, float dampTime = 0.5f)
        {
            UseStrafeAnimation = useStrafeAnimation;
            AnimationMaxPitch = animationMaxPitch;
            AnimationMaxTilt = animationMaxTilt;
            DampTime = dampTime;
        }
    }
    /// <summary>
    /// Basic settings related to creature "traits".
    /// </summary>
    public struct CreatureTraitsData
    {
        /// <summary>
        /// The rate at which the creature gets hungrier. Predators often require higher levels of hunger to attack.
        /// </summary>
        public float HungerIncreaseRate;
        /// <summary>
        /// The rate at which this creature becomes passive while actively hunting.
        /// </summary>
        public float AggressionDecreaseRate;
        /// <summary>
        /// The rate at which this creature becomes less scared. Used in very specific circumstances, most notably when taking damage.
        /// </summary>
        public float ScaredDecreaseRate;

        /// <summary>
        /// The rate of change for each 'trait' is measured per second.
        /// </summary>
        /// <param name="hungerIncreaseRate">The rate at which the creature gets hungrier. Predators often require higher levels of hunger to attack.</param>
        /// <param name="aggressionDecreaseRate">The rate at which this creature becomes passive while actively hunting.</param>
        /// <param name="scaredDecreaseRate">The rate at which this creature becomes less scared. Used in very specific circumstances, most notably when taking damage.</param>
        public CreatureTraitsData(float hungerIncreaseRate = 0f, float aggressionDecreaseRate = 0.05f, float scaredDecreaseRate = 0.25f)
        {
            HungerIncreaseRate = hungerIncreaseRate;
            AggressionDecreaseRate = aggressionDecreaseRate;
            ScaredDecreaseRate = scaredDecreaseRate;
        }
    }
#endregion
}