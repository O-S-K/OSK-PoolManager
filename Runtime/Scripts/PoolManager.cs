using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using Object = UnityEngine.Object;

namespace OSK.Pooling
{
    [Serializable]
    public class SettingGroupPool
    {
        [GUIColor(0.7f, 0.9f, 1f)]
        [FoldoutGroup("$groupName", expanded: true)]
        [LabelText("Group Name")]
        public string groupName;

        [FoldoutGroup("$groupName")]
        [TableList(AlwaysExpanded = true)]
        public List<SettingPool> pools = new();
    }

    [Serializable]
    public class SettingPool
    {
        [TableColumnWidth(100, Resizable = false)]
        [PreviewField(60, ObjectFieldAlignment.Left)]
        [HideLabel]
        public GameObject prefab;

        [TableColumnWidth(120)]
        public Transform parent;

        [TableColumnWidth(60)]
        public int size = 10;

        public SettingPool(GameObject prefab, Transform parent, int size)
        {
            this.prefab = prefab;
            this.parent = parent;
            this.size = size;
        }
    }

    public class PoolManager : MonoBehaviour
    {
        public static PoolManager Instance { get; private set; }

        [Title("Pool Manager")]
        [SerializeReference]
        [ListDrawerSettings(Expanded = true)]
        public Dictionary<string, Dictionary<Object, ObjectPool<Object>>> k_GroupPrefabLookup = new();
        [ListDrawerSettings(Expanded = true)]
        [SerializeReference] public Dictionary<Object, ObjectPool<Object>> k_InstanceLookup = new();

        [Space]
        [Title("Preload Settings")]
        [SerializeField] private bool usePreloadSettings = true;
        [ListDrawerSettings(Expanded = true, ShowPaging = false)]
        [GUIColor(0.9f,1f,1f)]
        [SerializeField] private List<SettingGroupPool> preloadSettings = new();


        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }

            if (!usePreloadSettings) return;
            foreach (var setting in preloadSettings)
            {
                foreach (var pool in setting.pools)
                {
                    Preload(setting.groupName, pool.prefab, pool.parent, pool.size);
                }
            }
        }
 

        public void Preload(string groupName, Object prefab, Transform parent, int size)
        {
            WarmPool(groupName, prefab, parent, size);
        }

        public T Query<T>(string groupName, T prefab) where T : Object
        {
            if (k_GroupPrefabLookup.TryGetValue(groupName, out var prefabPools))
            {
                if (prefabPools.TryGetValue(prefab, out var pool))
                {
                    return pool.GetItem() as T;
                }
            }

            return null;
        }

        public T Spawn<T>(string groupName, T prefab, Transform parent = null) where T : Object
        {
            return Spawn(groupName, prefab, parent, Vector3.zero, Quaternion.identity);
        }

        public T Spawn<T>(string groupName, T prefab, Transform parent, Transform transform) where T : Object
        {
            return Spawn(groupName, prefab, parent, transform.position, transform.rotation);
        }

        public T Spawn<T>(string groupName, T prefab, Transform parent, Vector3 position) where T : Object
        {
            return Spawn(groupName, prefab, parent, position, Quaternion.identity);
        }

        public T Spawn<T>(string groupName, T prefab, Transform parent, Vector3 position, Quaternion rotation)
            where T : Object
        {
            var instance = Spawn(groupName, prefab, parent, 1);
            if (instance is Component component)
            {
                component.transform.position = position;
                component.transform.rotation = rotation;
            }
            else if (instance is GameObject go)
            {
                go.transform.position = position;
                go.transform.rotation = rotation;
            }

            return instance;
        }

        public T Spawn<T>(string groupName, T prefab, Transform parent, int size) where T : Object
        {
            if (!IsGroupAndPrefabExist(groupName, prefab))
            {
                if (size <= 0)
                {
                    Debug.LogError("Pool size must be greater than 0.");
                    return null;
                }

                WarmPool(groupName, prefab, parent, size);
            }

            var pool = k_GroupPrefabLookup[groupName][prefab];
            var instance = pool.GetItem() as T;

            if (instance == null)
            {
                Debug.LogError($"Object from pool is null or destroyed. Group: {groupName}, Prefab: {prefab.name}");
                return null;
            }

            switch (instance)
            {
                case Component component:
                    component.gameObject.SetActive(true);
                    component.transform.SetParent(parent);
                    break;
                case GameObject go:
                    go.SetActive(true);
                    go.transform.SetParent(parent);
                    break;
            }

            if (!k_InstanceLookup.TryAdd(instance, pool))
            {
                Debug.LogWarning($"This object pool already contains the item provided: {instance}");
                return instance;
            }

            return instance;
        }

        private void WarmPool<T>(string group, T prefab, Transform parent, int size) where T : Object
        {
            if (IsGroupAndPrefabExist(group, prefab))
            {
                Debug.LogError($"Pool for prefab '{prefab.name}' in group '{group}' has already been created.");
                return;
            }

            if (size <= 0)
            {
                Debug.LogError("Pool size must be greater than 0.");
                return;
            }

            var pool = new ObjectPool<Object>(() =>
            {
                var go = InstantiatePrefab(prefab, parent);
                if (go is Component component)
                {
                    component.gameObject.SetActive(false);
                }
                else if (go is GameObject gameObject)
                {
                    gameObject.SetActive(false);
                }
                return go;
            }, size);
            if (!k_GroupPrefabLookup.ContainsKey(group))
            {
                k_GroupPrefabLookup[group] = new Dictionary<Object, ObjectPool<Object>>();
            }

            k_GroupPrefabLookup[group][prefab] = pool;
        }

        private Object InstantiatePrefab<T>(T prefab, Transform parent) where T : Object
        {
            return prefab is GameObject go
                ? Object.Instantiate(go, parent)
                : Object.Instantiate((Component)(object)prefab, parent);
        }

        private bool IsGroupAndPrefabExist(string groupName, Object prefab)
        {
            return k_GroupPrefabLookup.ContainsKey(groupName) &&
                   k_GroupPrefabLookup[groupName].ContainsKey(prefab);
        }

        public void Despawn(Object instance)
        {
            DeactivateInstance(instance);
            if (k_InstanceLookup.TryGetValue(instance, out var pool))
            {
                pool.ReleaseItem(instance);
                k_InstanceLookup.Remove(instance);
            }
            else
            {
                Debug.LogWarning($"{instance} not found in any pool.");
            }
        }

        public void Despawn(Object instance, float delay, bool unscaleTime = false)
        {
            if (delay <= 0)
            {
                Despawn(instance);
                return;
            }

            StartCoroutine(DODelayedCall(delay, () => Despawn(instance), unscaleTime));
        }

        private IEnumerator DODelayedCall(float delay, Action action, bool unscaleTime = false)
        {
            if (unscaleTime)
                yield return new WaitForSecondsRealtime(delay);
            else
                yield return new WaitForSeconds(delay);

            action?.Invoke();
        }

        public void DespawnAllInGroup(string groupName)
        {
            if (k_GroupPrefabLookup.TryGetValue(groupName, out var prefabPools))
            {
                foreach (var pool in prefabPools.Values)
                {
                    List<Object> toRemove = new();
                    foreach (var pair in k_InstanceLookup)
                    {
                        if (pair.Value == pool)
                        {
                            DeactivateInstance(pair.Key);
                            pool.ReleaseItem(pair.Key);
                            toRemove.Add(pair.Key);
                        }
                    }

                    foreach (var obj in toRemove)
                        k_InstanceLookup.Remove(obj);
                }
            }
        }

        public void DespawnAllActive()
        {
            foreach (var kv in k_InstanceLookup)
            {
                DeactivateInstance(kv.Key);
                kv.Value.ReleaseItem(kv.Key);
            }

            k_InstanceLookup.Clear();
        }

        private void DeactivateInstance(Object instance)
        {
            if (instance is Component component)
                component.gameObject.SetActive(false);
            else if (instance is GameObject go)
                go.SetActive(false);
        }

        public void DestroyAllInGroup(string groupName)
        {
            if (k_GroupPrefabLookup.TryGetValue(groupName, out var prefabPools))
            {
                foreach (var kvp in prefabPools.ToList()) // tạo bản sao để tránh modify khi foreach
                {
                    var pool = kvp.Value;
                    pool.DestroyAndClean();
                    pool.Clear();
                }

                k_GroupPrefabLookup.Remove(groupName);
            }
        }


        public void DestroyAllGroups()
        {
            foreach (var prefabPools in k_GroupPrefabLookup.Values)
            {
                foreach (var pool in prefabPools.Values)
                {
                    pool.DestroyAndClean();
                    pool.Clear();
                }
            }

            k_GroupPrefabLookup.Clear();
        }

        public void CleanAllDestroyedInPools()
        {
            foreach (var prefabPools in k_GroupPrefabLookup.Values)
            {
                foreach (var pool in prefabPools.Values)
                {
                    pool.DestroyAndClean();
                }
            }
        }

        public bool HasGroup(string groupName)
        {
            return k_GroupPrefabLookup.ContainsKey(groupName);
        }
        
        
        public string KeyGroupPoolSetting(string key)
        {
            foreach (var setting in preloadSettings)
            {
                if(setting.groupName == key)
                {
                    return setting.groupName;
                }
            }
            Debug.LogError($"Group name '{key}' not found in preload settings.");
            return "";
        }

    }
}