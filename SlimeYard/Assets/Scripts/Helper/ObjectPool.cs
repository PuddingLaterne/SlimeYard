using UnityEngine;
using System.Collections.Generic;

public class ObjectPool : MonoBehaviour
{
    public GameObject PoolObjectPrefab;
    public int StartObjectAmount = 16;
    public bool AllowExpansion = true;

    private List<GameObject> objectPool;

    public void Init()
    {
        objectPool = new List<GameObject>();
        for (int i = 0; i < StartObjectAmount; i++)
        {
            ExpandPool();
        }
    }

    public void Reset()
    {
        foreach (GameObject poolObject in objectPool)
        {
            poolObject.SetActive(false);
        }
    }

    public GameObject GetObjectFromPool()
    {
        foreach (GameObject poolObject in objectPool)
        {
            if (!poolObject.activeInHierarchy)
                return poolObject;
        }
        if (AllowExpansion)
        {
            ExpandPool();
            return objectPool[objectPool.Count - 1];
        }
        return null;
    }

    private void ExpandPool()
    {
        GameObject newPoolObject = Instantiate(PoolObjectPrefab);
       
        newPoolObject.SetActive(false);
        newPoolObject.transform.SetParent(transform);

        objectPool.Add(newPoolObject);        
    }
}
