using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;

public class PoolHelper : MonoBehaviour
{

    public static Dictionary<GameObject, ObjectPool<GameObject>> PoolDict = new Dictionary<GameObject, ObjectPool<GameObject>>(){};
    public static ObjectPool<GameObject> GetPool(GameObject _prefab)
    {
        ObjectPool<GameObject> pool;
        try
        {
            pool = PoolDict[_prefab];
            prefab = _prefab;
        }
        catch(KeyNotFoundException)
        {
            pool = CreateObjectPoolForPrefab(_prefab);
            PoolDict.Add(_prefab, pool);
        }
        return pool;
    }
    public static ObjectPool<GameObject> GetPool(string prefabName)
    {
        List<GameObject> keys = PoolDict.Keys.Where(key => key.name.Equals(prefabName)).ToList();
        return GetPool(keys[0]);
    }

    public static GameObject prefab;
    public static ObjectPool<GameObject> CreateObjectPoolForPrefab(GameObject _prefab)
    {
        prefab = _prefab;
        return new ObjectPool<GameObject>(CreateFunction, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, defaultCapacity:200);
    }

    static GameObject CreateFunction()
    {
        GameObject instantiation = Utility.InstantiateSameName(prefab);
        return instantiation;
    }

    static void OnReturnedToPool(GameObject obj)
    {
        obj.SetActive(false);
    }

    // Called when an item is taken from the pool using Get
    static void OnTakeFromPool(GameObject obj)
    {
        obj.SetActive(true);
    }

    // If the pool capacity is reached then any items returned will be destroyed.
    // We can control what the destroy behavior does, here we destroy the GameObject.
    static void OnDestroyPoolObject(GameObject obj)
    {
        Destroy(obj);
    }






}