## 🔁 OSK-PoolManager

**OSK-PoolManager** is a robust object pooling system designed to improve performance by reusing GameObjects instead of repeatedly instantiating/destroying them.

It supports **group pooling**, **preload**, **spawn**, **despawn**, and **cleanup** of pooled objects — all with simple APIs and Odin Inspector UI integration.

---

## 🎮 Example Usage

```csharp
using System;
using OSK.Pooling;
using Sirenix.OdinInspector;
using UnityEngine;

public class TestPool : MonoBehaviour
{
    [Header("Pool")] 
    public string keyGroupPool = "TestGroup";
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
```

---

## ✅ Outstanding Features

- Group-based pooling with `groupKey` for better organization.
- Preload objects before use to reduce runtime lag.
- Supports both `Component` and `GameObject` based spawn.
- Quick cleanup: despawn all active, per group, or destroy all pools.
- Detect and clean destroyed objects in pools.
- Odin Inspector UI integration for rapid testing and debugging.

---

## 📦 1. Install Dependencies

- **Odin Inspector**:  
  [https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041](https://assetstore.unity.com/packages/tools/utilities/odin-inspector-and-serializer-89041)

---

## ⚙️ Setting Up Group Pool

```csharp
[Serializable]
public class SettingGroupPool
{
    public string groupName;

    [TableList]
    public List<SettingPool> pools = new();
}

[Serializable]
public class SettingPool
{
    public GameObject prefab;
    public Transform parent;
    public int size;

    public SettingPool(GameObject prefab, Transform parent, int size)
    {
        this.prefab = prefab;
        this.parent = parent;
        this.size = size;
    }
}
```

You can organize and preview pools cleanly in the Unity Inspector with Odin's `[TableList]`.

---

## 📌 Tips

- Use `Preload()` to prepare a pool at runtime and avoid frame spikes.
- Use `groupKey` to manage separate object categories (UI, enemies, bullets, etc.).
- `Despawn()` returns object to pool, can choose to deactivate or keep active.
- Use `CleanAllDestroyedInPools()` to avoid memory issues with externally destroyed objects.
- Extendable and easy to integrate into existing systems.

---

## 📞 Support

- **Email**:  [gamecoding1999@gmail.com](mailto:gamecoding1999@gmail.com)  
- **Facebook**: [OSK](https://www.facebook.com/xOskx/)