using System;
using OSK.Pooling;
using UnityEngine;

public class Sphere : MonoBehaviour
{
    
    public void Despawn(bool isGO)
    {
        if (isGO)
        {
            PoolManager.Instance.Despawn(gameObject, 1f);
        }
        else
        {
            PoolManager.Instance.Despawn(this, 1);
        }
    }
}
