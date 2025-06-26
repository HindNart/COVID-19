using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    [SerializeField] private int initialSize = 50;

    public GameObject Get(GameObject prefab)
    {
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
            for (int i = 0; i < initialSize; i++)
            {
                GameObject obj = Instantiate(prefab);
                obj.SetActive(false);
                obj.AddComponent<PooledObject>().Prefab = prefab;
                pools[prefab].Enqueue(obj);
            }
        }

        if (pools[prefab].Count == 0)
        {
            GameObject obj = Instantiate(prefab);
            obj.SetActive(false);
            obj.AddComponent<PooledObject>().Prefab = prefab;
            pools[prefab].Enqueue(obj);
        }

        GameObject pooledObj = pools[prefab].Dequeue();
        pooledObj.SetActive(true);
        return pooledObj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        GameObject prefab = obj.GetComponent<PooledObject>().Prefab;
        pools[prefab].Enqueue(obj);
    }
}

public class PooledObject : MonoBehaviour
{
    public GameObject Prefab { get; set; }
}