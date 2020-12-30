using ECCLibrary;
using ECCLibrary.Internal;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ECCLibrary
{
    public static class StaticCreatureSpawns
    {
        public static void RegisterStaticSpawn(StaticSpawn spawnData)
        {
            foreach(var spawn in staticSpawns)
            {
                if(spawn.uniqueIdentifier == spawnData.uniqueIdentifier)
                {
                    ECCLog.AddMessage("Multiple StaticSpawns that share the Unique Identifier '{0}'.", spawnData.uniqueIdentifier);
                }
            }
            staticSpawns.Add(spawnData);
        }
        internal static readonly List<StaticSpawn> staticSpawns = new List<StaticSpawn>();

        internal static bool HasSpawnedInCurrentSave(string id)
        {
            using (StreamReader sr = new StreamReader(ECCPatch.staticCreaturesPath))
            {
                string text = sr.ReadToEnd();
                if (string.IsNullOrEmpty(text))
                {
                    return false;
                }
                string[] alreadySpawned = text.Split(',');
                foreach (string str in alreadySpawned)
                {
                    if (str == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static void InstantiateAllPrefabSpawners()
        {
            foreach(StaticSpawn spawnData in staticSpawns)
            {
                CreatePrefabSpawner(spawnData);
            }
        }
        private static void CreatePrefabSpawner(StaticSpawn spawnData)
        {
            if (HasSpawnedInCurrentSave(spawnData.uniqueIdentifier))
            {
                return;
            }
            GameObject spawnObject = new GameObject(string.Format("spawner-{0}", spawnData.uniqueIdentifier));
            spawnObject.AddComponent<StaticCreatureSpawnObject>().InitializeInstance(spawnData);
        }
    }
    internal class StaticCreatureSpawnObject : MonoBehaviour
    {
        public StaticSpawn mySpawnData;
        const float checkDelay = 1f;
        const float doubleCheckDelay = 1f;

        public void InitializeInstance(StaticSpawn spawnData)
        {
            mySpawnData = spawnData;
            StartCoroutine(SpawnSchedule(Random.value));
        }

        private IEnumerator SpawnSchedule(float initialDelay)
        {
            yield return new WaitForSeconds(initialDelay);
            for (; ; )
            {
                yield return new WaitForSeconds(checkDelay);
                if (InSpawningDistance())
                {
                    if (StaticCreatureSpawns.HasSpawnedInCurrentSave(mySpawnData.uniqueIdentifier))
                    {
                        Destroy(gameObject);
                        yield break;
                    }
                    yield return new WaitForSeconds(doubleCheckDelay);
                    if (InSpawningDistance())
                    {
                        Spawn();
                    }
                }
            }
        }

        private bool InSpawningDistance()
        {
            if (Vector3.Distance(MainCamera.camera.transform.position, mySpawnData.position) > mySpawnData.maxDistance)
            {
                return false;
            }
            return true;
        }

        void Spawn()
        {
            GameObject obj = UWE.Utils.InstantiateDeactivated(mySpawnData.prefab.GetGameObject(), mySpawnData.position, Quaternion.identity);
            LargeWorldEntity lwe = obj.GetComponent<LargeWorldEntity>();
            bool active = LargeWorld.main.streamer.cellManager.RegisterEntity(lwe);
            if (active)
            {
                obj.SetActive(true);
                Destroy(gameObject);
                using (StreamWriter sw = new StreamWriter(ECCPatch.staticCreaturesPath))
                {
                    sw.Write(mySpawnData.uniqueIdentifier + ",");
                }
            }
            else //Cells take a little while to load in, this is for in a case where that happens
            {
                Destroy(obj);
            }
        }
    }
    /// <summary>
    /// Settings based around a creature that will always spawn in the same position(s), and is separate from the LootDistributionData.
    /// </summary>
    public struct StaticSpawn
    {
        public CreatureAsset prefab;
        public Vector3 position;
        public string uniqueIdentifier;
        public float maxDistance;

        public StaticSpawn(CreatureAsset prefab, Vector3 position, string uniqueIdentifier, float maxDistance)
        {
            this.prefab = prefab;
            this.position = position;
            this.uniqueIdentifier = uniqueIdentifier;
            this.maxDistance = maxDistance;
        }
    }
}
