using System.Collections.Generic;
using UnityEngine;

public class PrefabAlloc : ScriptableObject
{
    private GameObject prefab;
    private Dictionary<object, GameObject> prefabs = new Dictionary<object, GameObject>();

    public void SetPrefab(GameObject newPrefab)
    {
        prefab = newPrefab;
    }

    public GameObject GetFree(object key)
    {
        if (prefabs.ContainsKey(key))
        {
            return prefabs[key];
        }

        GameObject obj;

        foreach (KeyValuePair<object, GameObject> x in prefabs)
        {
            if (x.Value.activeSelf == false)
            {
                obj = x.Value;
                prefabs.Remove(x.Key);
                prefabs.Add(key, obj);
                return obj;
            }
        }

        obj = Instantiate(prefab);
        prefabs.Add(key, obj);
        return obj;
    }
}