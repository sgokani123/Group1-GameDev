using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    [Header("Pool Settings")]
    [SerializeField] private GameObject prefab;
    [SerializeField] private int prewarmCount = 20;
    [SerializeField] private Transform poolParent;

    private readonly Queue<GameObject> pool = new Queue<GameObject>();

    public void Init(GameObject prefabToPool, int prewarm, Transform parent = null)
    {
        prefab = prefabToPool;
        prewarmCount = Mathf.Max(0, prewarm);
        poolParent = parent;

        // Prewarm
        for (int i = 0; i < prewarmCount; i++)
        {
            var obj = CreateNew();
            Return(obj);
        }
    }

    GameObject CreateNew()
    {
        var obj = Instantiate(prefab);
        if (poolParent != null) obj.transform.SetParent(poolParent);
        obj.SetActive(false);
        return obj;
    }

    public GameObject Get()
    {
        var obj = pool.Count > 0 ? pool.Dequeue() : CreateNew();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        if (poolParent != null) obj.transform.SetParent(poolParent);
        pool.Enqueue(obj);
    }
}