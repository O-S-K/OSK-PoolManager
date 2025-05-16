using UnityEngine;
 
namespace OSK.Pooling
{
    [System.Serializable]
    public class PooleData
    {
        public Component prefab;
        public int size;
        public int maxCapacity;
    }
}
