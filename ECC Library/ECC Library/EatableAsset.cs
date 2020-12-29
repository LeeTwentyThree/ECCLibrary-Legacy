using SMLHelper.V2.Assets;
using SMLHelper.V2.Crafting;
using SMLHelper.V2.Utility;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ECCLibrary.Internal;

namespace ECCLibrary
{
    public class EatableAsset : Craftable
    {
        TechType originalFish;
        GameObject model;
        EatableData eatableData;
        bool cured;
        GameObject prefab;
        Atlas.Sprite sprite;

        public EatableAsset(string classId, string friendlyName, string description, GameObject model, TechType originalFish, EatableData eatableData, bool cured, Texture2D sprite) : base(classId, friendlyName, description)
        {
            this.model = model;
            this.originalFish = originalFish;
            this.eatableData = eatableData;
            this.cured = cured;
            this.sprite = ImageUtils.LoadSpriteFromTexture(sprite);
        }
            
        protected override TechData GetBlueprintRecipe()
        {
            if (cured)
            {
                return new TechData() { Ingredients = new List<Ingredient>() { new Ingredient(originalFish, 1), new Ingredient(TechType.Salt, 1) }, craftAmount = 1 };
            }
            else
            {
                return new TechData() { Ingredients = new List<Ingredient>() { new Ingredient(originalFish, 1) }, craftAmount = 1 };
            }
        }

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
                var worldForces = prefab.EnsureComponent<WorldForces>();
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
                var worldForces = prefab.EnsureComponent<WorldForces>();
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
                    pickupable.cubeOnPickup = true;
                }
                eatableData.MakeItemEatable(prefab);
            }
            yield return null;
            gameObject.Set(prefab);
        }

        protected override Atlas.Sprite GetItemSprite()
        {
            return sprite;
        }

        public override CraftTree.Type FabricatorType => CraftTree.Type.Fabricator;

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

        public virtual UBERMaterialProperties MaterialSettings
        {
            get
            {
                return new UBERMaterialProperties(8f, 1f);
            }
        }
    }
}
