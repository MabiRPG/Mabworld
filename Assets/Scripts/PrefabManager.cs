using System.Collections.Generic;
using UnityEngine;

/// <summary>
///     Handles a pool of prefab objects
/// </summary>
public class PrefabManager : ScriptableObject
{
    // Prefab to instantiate
    private GameObject prefab;
    // Dict of available prefabs
    public Dictionary<object, GameObject> prefabs = new Dictionary<object, GameObject>();

    /// <summary>
    ///     Sets the prefab to instantiate.
    /// </summary>
    /// <param name="prefab"></param>
    public void SetPrefab(GameObject prefab)
    {
        this.prefab = prefab;
    }

    /// <summary>
    ///     Allocates a free prefab instance, if available.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="transform"></param>
    /// <returns>Prefab GameObject</returns>
    public GameObject GetFree(object key, Transform transform)
    {
        GameObject obj;

        // If prefab has relevant key, return immediately.
        if (prefabs.ContainsKey(key))
        {
            obj = prefabs[key];
            obj.SetActive(true);
            return obj;
        }

        // Search for any inactive prefabs and return.
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

        // If no inactive prefabs, instantiate and add to list.
        obj = Instantiate(prefab, transform);
        prefabs.Add(key, obj);
        return obj;
    }

    /// <summary>
    ///     Sets the state of all prefabs.
    /// </summary>
    /// <param name="state"></param>
    public void SetActiveAll(bool state)
    {
        foreach (KeyValuePair<object, GameObject> x in prefabs)
        {
            x.Value.SetActive(state);
        }
    }
}