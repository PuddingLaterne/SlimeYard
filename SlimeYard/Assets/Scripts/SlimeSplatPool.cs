using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeSplatPool : MonoBehaviour
{
    public static SlimeSplatPool Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<SlimeSplatPool>();
            return instance;
        }
    }

    private static SlimeSplatPool instance;
    private ObjectPool[] pools;

    public void Start()
    {
        pools = GetComponentsInChildren<ObjectPool>();   
        foreach(ObjectPool pool in pools)
        {
            pool.Init();
        }
    }

    public void Reset()
    {
        foreach (ObjectPool pool in pools)
        {
            pool.Reset();
        }
    }

    public GameObject CreateSlimeSplat(Color color, Vector3 position, float lifeTime)
    {
        int poolIndex = Random.Range(0, pools.Length);
        GameObject splatObject = pools[poolIndex].GetObjectFromPool();
        splatObject.transform.parent = transform;
        splatObject.transform.position = position;
        splatObject.SetActive(true);

        SlimeSplat splat = splatObject.GetComponent<SlimeSplat>();
        splat.LifeTime = lifeTime;
        splat.Color = color;

        return splatObject;
    }
}
