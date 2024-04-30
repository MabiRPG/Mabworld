using System.Collections.Generic;
using UnityEngine;

public class PrefabManager : ScriptableObject
{
    private GameObject prefab;
    private Dictionary<object, GameObject> prefabs = new Dictionary<object, GameObject>();

    public void SetPrefab(GameObject newPrefab)
    {
        prefab = newPrefab;
    }

    public GameObject GetFree(object key, Transform transform)
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
                obj.SetActive(true);
                prefabs.Remove(x.Key);
                prefabs.Add(key, obj);
                return obj;
            }
        }

        obj = Instantiate(prefab, transform);
        prefabs.Add(key, obj);
        return obj;
    }

    public void SetActiveAll(bool state)
    {
        foreach (KeyValuePair<object, GameObject> x in prefabs)
        {
            x.Value.SetActive(state);
        }
    }
}