﻿using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using ECCLibrary.Internal;
using SMLHelper.V2.Handlers;
#if SN1
using Sprite = Atlas.Sprite;
using RecipeData = SMLHelper.V2.Crafting.TechData;
#endif
namespace ECCLibrary
{
    /// <summary>
    /// A basic AssetClass that does everything required for edible fish for you. This class should not be inherited from.
    /// </summary>
    public class EatableAsset : Craftable
    {
        TechType originalFish;
        GameObject model;
        EatableData eatableData;
        bool cured;
        GameObject prefab;
        Sprite sprite;
        ItemSoundsType soundType;
        UnityEngine.Sprite popup;

        /// <summary>
        /// Constructor for a cooked/cured version of the creature.
        /// </summary>
        /// <param name="classId">The TechType.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="description">The tooltip.</param>
        /// <param name="model">The prefab of the original fish.</param>
        /// <param name="originalFish">The TechType of the original fish.</param>
        /// <param name="eatableData">The data related to being edible.</param>
        /// <param name="cured">Whether the recipe requires salt. Also, only non-cured fish will be spawned by the Thermoblade.</param>
        /// <param name="sprite">The icon in the inventory.</param>
        public EatableAsset(string classId, string friendlyName, string description, GameObject model, TechType originalFish, EatableData eatableData, bool cured, Texture2D sprite) : base(classId, friendlyName, description)
        {
            this.model = model;
            this.originalFish = originalFish;
            this.eatableData = eatableData;
            this.cured = cured;
            this.sprite = ImageUtils.LoadSpriteFromTexture(sprite);
            this.soundType = ItemSoundsType.Default;
            OnFinishedPatching += () => 
            {
                //Thermoblade support
                if (!cured && originalFish != TechType.None)
                {
                    CraftDataHandler.SetCookedVariant(originalFish, TechType);
                }
                #if SN1
                ECCHelpers.PatchItemSounds(TechType, soundType);
#endif
                LanguageHandler.SetLanguageLine(string.Format("{0}_DiscoverMessage", ClassID), "NEW LIFEFORM DISCOVERED");
            };
        }

        /// <summary>
        /// Constructor for a cooked/cured version of the creature.
        /// </summary>
        /// <param name="classId">The TechType.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="description">The tooltip.</param>
        /// <param name="model">The prefab of the original fish.</param>
        /// <param name="originalFish">The TechType of the original fish.</param>
        /// <param name="eatableData">The data related to being edible.</param>
        /// <param name="cured">Whether the recipe requires salt. Also, only non-cured fish will be spawned by the Thermoblade.</param>
        /// <param name="sprite">The icon in the inventory.</param>
        /// <param name="soundType">The audio used for this creature in the inventory.</param>
        public EatableAsset(string classId, string friendlyName, string description, GameObject model, TechType originalFish, EatableData eatableData, bool cured, Texture2D sprite, ItemSoundsType soundType) : base(classId, friendlyName, description)
        {
            this.model = model;
            this.originalFish = originalFish;
            this.eatableData = eatableData;
            this.cured = cured;
            this.sprite = ImageUtils.LoadSpriteFromTexture(sprite);
            this.soundType = soundType;
            OnFinishedPatching += () => 
            {
                //Thermoblade support
                if (!cured && originalFish != TechType.None)
                {
                    CraftDataHandler.SetCookedVariant(originalFish, TechType);
                }
                #if SN1
                ECCHelpers.PatchItemSounds(TechType, soundType);
#endif
                LanguageHandler.SetLanguageLine(string.Format("{0}_DiscoverMessage", ClassID), "NEW LIFEFORM DISCOVERED");
            };
        }

        /// <summary>
        /// Constructor for a cooked/cured version of the creature.
        /// </summary>
        /// <param name="classId">The TechType.</param>
        /// <param name="friendlyName">The friendly name.</param>
        /// <param name="description">The tooltip.</param>
        /// <param name="model">The prefab of the original fish.</param>
        /// <param name="originalFish">The TechType of the original fish.</param>
        /// <param name="eatableData">The data related to being edible.</param>
        /// <param name="cured">Whether the recipe requires salt. Also, only non-cured fish will be spawned by the Thermoblade.</param>
        /// <param name="sprite">The icon in the inventory.</param>
        /// <param name="soundType">The audio used for this creature in the inventory.</param>
        /// <param name="unlockPopup">The icon shown when unlocked</param>
        public EatableAsset(string classId, string friendlyName, string description, GameObject model, TechType originalFish, EatableData eatableData, bool cured, Texture2D sprite, ItemSoundsType soundType, UnityEngine.Sprite unlockPopup) : base(classId, friendlyName, description)
        {
            this.model = model;
            this.originalFish = originalFish;
            this.eatableData = eatableData;
            this.cured = cured;
            this.sprite = ImageUtils.LoadSpriteFromTexture(sprite);
            this.soundType = soundType;
            this.popup = unlockPopup;
            OnFinishedPatching += () => 
            {
                //Thermoblade support
                if (!cured && originalFish != TechType.None)
                {
                    CraftDataHandler.SetCookedVariant(originalFish, TechType);
                }
#if SN1
                ECCHelpers.PatchItemSounds(TechType, soundType);
#endif
                LanguageHandler.SetLanguageLine(string.Format("{0}_DiscoverMessage", ClassID), "NEW LIFEFORM DISCOVERED");
            };
        }

        /// <summary>
        /// Already set by ECC by default. Only override this if you want unique recipes.
        /// </summary>
        /// <returns></returns>
        protected override RecipeData GetBlueprintRecipe()
        {
            if (cured)
            {
                return new RecipeData() { Ingredients = new List<Ingredient>() { new Ingredient(originalFish, 1), new Ingredient(TechType.Salt, 1) }, craftAmount = 1 };
            }
            else
            {
                return new RecipeData() { Ingredients = new List<Ingredient>() { new Ingredient(originalFish, 1) }, craftAmount = 1 };
            }
        }

#if SN1
        public override GameObject GetGameObject()
        {
            if(prefab == null)
            {
                prefab = GameObject.Instantiate(model);
                ECCHelpers.ApplySNShaders(prefab, MaterialSettings);
                prefab.EnsureComponent<TechTag>().type = TechType;
                prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                var pickupable = prefab.EnsureComponent<Pickupable>();
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
                prefab.EnsureComponent<Rigidbody>().useGravity = false;
                prefab.EnsureComponent<WorldForces>();
                GameObject craftModel = prefab.SearchChild("CraftModel");
                if(craftModel != null)
                {
                    var vfxFab = craftModel.AddComponent<VFXFabricating>();
                    Renderer renderer = craftModel.GetComponentInChildren<Renderer>();
                    vfxFab.scaleFactor = craftModel.transform.localScale.x;
                    vfxFab.eulerOffset = craftModel.transform.localEulerAngles;
                    vfxFab.posOffset = new Vector3(0f, renderer.bounds.extents.y, 0f);
                    vfxFab.localMinY = -renderer.bounds.extents.y;
                    vfxFab.localMaxY = renderer.bounds.extents.y;
                }
                else
                {
                    ECCLog.AddMessage("No child of name CraftModel found in crafted item {0}. Using default cube model.", TechType);
                    pickupable.cubeOnPickup = true;
                }
                eatableData.MakeItemEatable(prefab);
            }
            return prefab;
        }
#endif
        public override IEnumerator GetGameObjectAsync(IOut<GameObject> gameObject)
        {
            if (prefab == null)
            {
                prefab = GameObject.Instantiate(model);
                ECCHelpers.ApplySNShaders(prefab, MaterialSettings);
                prefab.EnsureComponent<TechTag>().type = TechType;
                prefab.EnsureComponent<PrefabIdentifier>().ClassId = ClassID;
                var pickupable = prefab.EnsureComponent<Pickupable>();
                prefab.EnsureComponent<LargeWorldEntity>().cellLevel = LargeWorldEntity.CellLevel.Near;
                prefab.EnsureComponent<Rigidbody>().useGravity = false;
                prefab.EnsureComponent<WorldForces>();
                GameObject craftModel = prefab.SearchChild("CraftModel");
                if (craftModel != null)
                {
                    var vfxFab = craftModel.AddComponent<VFXFabricating>();
                    Renderer renderer = craftModel.GetComponentInChildren<Renderer>();
                    vfxFab.scaleFactor = craftModel.transform.localScale.x;
                    vfxFab.eulerOffset = craftModel.transform.localEulerAngles;
                    vfxFab.posOffset = new Vector3(0f, renderer.bounds.extents.y, 0f);
                    vfxFab.localMinY = -renderer.bounds.extents.y;
                    vfxFab.localMaxY = renderer.bounds.extents.y;
                }
                else
                {
                    ECCLog.AddMessage("No child of name CraftModel found in crafted item {0}. Using default cube model.", TechType);
#if SN1
                    pickupable.cubeOnPickup = true;
#else
                    pickupable.isLootCube = true;
#endif
                }
                eatableData.MakeItemEatable(prefab);
            }
            yield return null;
            gameObject.Set(prefab);
        }
        
        /// <summary>
        /// The sprite for this item. Set automatically by ECC but can be overriden.
        /// </summary>
        /// <returns></returns>
        protected override Sprite GetItemSprite()
        {
            return sprite;
        }

        /// <summary>
        /// By default, crafted at Fabricator.
        /// </summary>
        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

        /// <summary>
        /// Set automatically by ECC but can be overriden. Not that you should.
        /// </summary>
        public override string[] StepsToFabricatorTab
        {
            get
            {
                if (cured)
                {
                    return new string[] { "Survival", "CuredFood" };
                }
                else
                {
                    return new string[] { "Survival", "CookedFood" };
                }
            }
        }

        /// <summary>
        /// The material settings of the physical food item.
        /// </summary>
        public UBERMaterialProperties MaterialSettings = new UBERMaterialProperties(8f, 1f);

        /// <summary>
        /// Automatically set for both SN1 and BZ. Only override if you want something special.
        /// </summary>
        public override TechCategory CategoryForPDA
        {
            get
            {
#if SN1
                if (cured)
                {
                    return TechCategory.CuredFood;
                }
                else
                {
                    return TechCategory.CookedFood;
                }
#else
                return TechCategory.FoodAndDrinks;
#endif
            }
        }

        /// <summary>
        /// By default <see cref="TechGroup.Survival"/>.
        /// </summary>
        public override TechGroup GroupForPDA
        {
            get
            {
                return TechGroup.Survival;
            }
        }

        /// <summary>
        /// By default, this is the original fish.
        /// </summary>
        public override TechType RequiredForUnlock
        {
            get
            {
                return originalFish;
            }
        }

        /// <summary>
        /// No idea what this does.
        /// </summary>
        [System.Obsolete("Not used anywhere")]
        public override string DiscoverMessage => "NotificationCreatureDiscovered";
    }
}
