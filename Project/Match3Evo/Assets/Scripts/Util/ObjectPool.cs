using System.Collections.Generic;
using UnityEngine;

namespace Match3_Evo
{
    public class ObjectPool : MonoBehaviour
    {
        public bool CreatePoolIfNotExisting = true;
        public List<PoolData> Pools = new List<PoolData>();
        private Dictionary<PoolObject, PoolData> hash = new Dictionary<PoolObject, PoolData>();

        private void Awake()
        {
            GM.Pool = this;

            for (int i = 0; i < Pools.Count; i++)
            {
                hash.Add(Pools[i].Prefab, Pools[i]);
            }
        }

        public GameObject GetObject(PoolObject prefab)
        {
            if (hash.ContainsKey(prefab))
            {
                return hash[prefab].GetObject();
            }
            else if(CreatePoolIfNotExisting)
            {
                PoolData pool = new PoolData();
                pool.Prefab = prefab;
                Pools.Add(pool);
                hash.Add(prefab, pool);
                return pool.GetObject();
            }
            return null;
        }

        public T GetObject<T>(PoolObject prefab) where T : Object
        {
            GameObject go = GetObject(prefab);
            return go == null ? null : go.GetComponentInChildren<T>();
        }

        public void Prewarm(PoolObject prefab, int count, int maxCapacity = 0)
        {
            PoolData pool;
            if (!hash.ContainsKey(prefab))
            {
                pool = new PoolData();
                pool.Prefab = prefab;
                Pools.Add(pool);
                hash.Add(prefab, pool);
            }
            else
            {
                pool = hash[prefab];
            }
            pool.MaxCapacity += maxCapacity;
            int createCount = count - pool.Objects.Count;
            for (int i = 0; i < createCount; i++)
            {
                PoolObject newObject = GameObject.Instantiate<PoolObject>(prefab);
                newObject.gameObject.SetActive(!pool.DisableObjectOnCreation);
                pool.Objects.Add(newObject);
            }
        }
    }

    [System.Serializable]
    public class PoolData
    {
        public bool DisableObjectOnCreation = true;
        public int MaxCapacity = 0;
        public PoolObject Prefab;
        public List<PoolObject> Objects = new List<PoolObject>();
        public int nextObject = 0;

        public GameObject GetObject()
        {

            if (MoveNext())
            {
                Objects[nextObject].ActiveObject = true;
                return Objects[nextObject].gameObject;
            }
            else if (MaxCapacity == 0 || Objects.Count < MaxCapacity)
            {
                PoolObject obj = GameObject.Instantiate<PoolObject>(Prefab);
                obj.gameObject.SetActive(!DisableObjectOnCreation);
                Objects.Add(obj);
                obj.ActiveObject = true;
                return obj.gameObject;
            }
            return null;
        }

        bool MoveNext()
        {
            for (int i = nextObject; i < Objects.Count; i++)
            {
                if (!Objects[i].ActiveObject)
                {
                    nextObject = i;
                    return true;
                }
            }
            for (int i = 0; i < nextObject; i++)
            {
                if (!Objects[i].ActiveObject)
                {
                    nextObject = i;
                    return true;
                }
            }
            nextObject = 0;
            return false;
        }
    }
}