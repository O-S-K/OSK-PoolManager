using System;
using OSK.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestPool : MonoBehaviour
{
    [Header("Pool")] public string keyGroupPool = "TestGroup";
    public Sphere Sphere;

    [Button]
    public void PreloadSpawn()
    {
        PoolManager.Instance.Preload(keyGroupPool, Sphere, null, 10);
    }
    
    [Button]
    public void SpawnComponent()
    {
       var s = PoolManager.Instance.Spawn(keyGroupPool, Sphere);
       s.Despawn(false);
    }
    
    [Button]
    public void SpawnGO()
    {
        var s = PoolManager.Instance.Spawn(keyGroupPool, Sphere.gameObject);
        s.GetComponent<Sphere>().Despawn(true);
    }
    
    [Button]
    public void DespawnAllActive()
    {
        PoolManager.Instance.DespawnAllActive();
    }
    
    [Button]
    public void DespawnInGroup()
    {
        PoolManager.Instance.DespawnAllInGroup(keyGroupPool);
    }
    
    [Button]
    public void DestroyAllGroups()
    {
        PoolManager.Instance.DestroyAllGroups();
    }
    
    [Button]
    public void CleanAllDestroyedInPools()
    {
        PoolManager.Instance.CleanAllDestroyedInPools();
    }
}